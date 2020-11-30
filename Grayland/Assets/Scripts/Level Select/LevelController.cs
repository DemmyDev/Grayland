using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    #region Variables

    [SerializeField, Tooltip("Player prefab")]
    GameObject player;

    [SerializeField, Tooltip("Transition object (child of this object)")]
    GameObject transition;

    [SerializeField, Tooltip("Level set scriptable")]
    LevelSet levelSet;

    [SerializeField, Tooltip("Place a test level here. Keep empty if playing normally")]
    GameObject testLevel;

    Vector2 spawnPoint;
    GameObject currentLevel, currentPlayer, currentEndpoint, currentDeathbox;
    int levelId = 1;
    float currentIntensity = 1f;
    bool isColorized = false;
    Animation anim;
    static public LevelController levelController;
    GrayscaleEffect grayscale;
    LevelEnd levelEnd;
    
    #endregion

    void Awake()
    {
        if (levelController == null) levelController = this;
        anim = transition.GetComponent<Animation>();
        grayscale = Camera.main.GetComponent<GrayscaleEffect>();
        levelId = levelSet.levelToLoad;
        LevelTransition(false);
    }

    // Only use for lerping at the end of a level
    void Update()
    {
        if (isColorized)
        {
            if (currentIntensity > .05f)
                currentIntensity = Mathf.Lerp(currentIntensity, 0f, Time.deltaTime);
            else
                currentIntensity = 0f;
                  
            grayscale.intensity = currentIntensity;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // ANALYTICS: Had to restart level
            StartCoroutine(LoadLevel(false));
        }

        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            // ANALYTICS: Stopped playing at levelId
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Destroys current level and either reloads the level or loads the next level
    /// </summary>
    /// <param name="isNextLevel">If true, goes to the next level in the levels list. If false, restarts current level</param>
    public IEnumerator LoadLevel(bool isNextLevel)
    {
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);
        anim.Stop("TransitionEnter");
        LevelTransition(isNextLevel);
    }

    public IEnumerator SceneTransition(int sceneNum)
    {
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);
        anim.Stop("TransitionEnter");
        SceneManager.LoadScene(sceneNum);
    }

    public void LevelTransition(bool isNextLevel)
    {
        anim.Play("TransitionExit");
        // If true, adds the current level and moves on to the next level
        if (isNextLevel) levelId++;

        // Destroy current level
        if (currentLevel != null) Destroy(currentLevel);
        if (currentPlayer != null) Destroy(currentPlayer);
        if (currentEndpoint != null) Destroy(currentEndpoint);

        // Reset values, just in case
        currentLevel = null;
        spawnPoint = Vector2.zero;
        currentEndpoint = null;
        currentDeathbox = null;
        levelEnd = null;

        if (levelId > levelSet.levels.Count && testLevel == null)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            // Instantiate level
            if (testLevel == null) currentLevel = Instantiate(levelSet.levels[levelId - 1].prefab);
            else currentLevel = Instantiate(testLevel);

            // Get level's information
            // oh my god fix all of this Find() bullshit ASAP
            spawnPoint = currentLevel.transform.Find("SpawnPoint").position;
            currentEndpoint = currentLevel.transform.Find("LevelEndpoint").gameObject;
            currentDeathbox = currentLevel.transform.Find("TilemapGroup").Find("Deathbox").gameObject;
            currentLevel.transform.parent = gameObject.transform;
            levelEnd = currentLevel.transform.Find("LevelEnd").GetComponent<LevelEnd>();

            // Spawn player
            currentPlayer = Instantiate(player, spawnPoint, Quaternion.identity);
            currentPlayer.transform.position = spawnPoint;

            currentIntensity = 1f;
            grayscale.intensity = currentIntensity;
            isColorized = false;

            UIController.UIControl.GetActivationUI().SetActive(false);
            UIController.UIControl.GetDialogueTri().SetActive(false);
            UIController.UIControl.GetDialogueUI().SetActive(false);
        }
    }

    /// <summary>
    /// Sets state of the current level, and of the next level if applicable
    /// </summary>
    /// <param name="stateNum">Level is locked = 0, level is unlocked = 1, level is completed = 2</param>
    public void SetLevelState(int stateNum)
    {
        // Set state of current level
        levelSet.levels[levelId - 1].state = (Level.LevelState)stateNum;

        // Setting string name for ReadWriteSave. Must be consistent through all other scripts
        string levelState = "level" + levelId + "State"; // EX: level1State
        ReadWriteSaveManager.Instance.SetData(levelState, stateNum);

        // If current level is complete, set next level to unlocked
        if (stateNum == 2 && levelId < levelSet.levels.Count)
        {
            levelSet.levels[levelId].state = (Level.LevelState)1;

            int n = levelId + 1;
            levelState = "level" + n + "State"; // EX: level1State
            ReadWriteSaveManager.Instance.SetData(levelState, 1);
        }

        ReadWriteSaveManager.Instance.Write();
        // Have check for unlocking new sections
    }

    /// <summary>
    /// Gets the current level ID
    /// </summary>
    /// <returns>Current level ID</returns>
    public int GetCurrentLevelId()
    {
        return levelId;
    }

    public PlayerController GetPlayerController()
    {
        return currentPlayer.GetComponent<PlayerController>();
    }

    public void DisableSaturation()
    {
        isColorized = true;
    }

    public GameObject GetDeathbox()
    {
        if (currentDeathbox != null)
            return currentDeathbox;
        else
            return null;
    }

    public bool GetIsColorized()
    {
        return isColorized;
    }

    public LevelEnd GetLevelEnd()
    {
        return levelEnd;
    }
}
