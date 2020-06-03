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

    [SerializeField, Tooltip("List of level prefabs")]
    List<GameObject> levels;

    Vector2 spawnPoint;
    GameObject currentLevel, currentPlayer, currentEndpoint, currentDeathbox;
    int levelId = 1;
    float currentIntensity = 1f;
    bool isColorized = false;
    Animation anim;
    static public LevelController levelController;
    GrayscaleEffect grayscale;
    
    #endregion

    void Awake()
    {
        if (levelController == null) levelController = this;
        anim = transition.GetComponent<Animation>();
        grayscale = Camera.main.GetComponent<GrayscaleEffect>();
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

    public void LevelTransition(bool isNextLevel)
    {
        anim.Play("TransitionExit");
        // If true, adds the current level and moves on to the next level
        if (isNextLevel)
        {
            levelId = levelId + 1;
        }

        // Destroy current level
        if (currentLevel != null) Destroy(currentLevel);
        if (currentPlayer != null) Destroy(currentPlayer);
        if (currentEndpoint != null) Destroy(currentEndpoint);

        // Reset values just in case
        currentLevel = null;
        spawnPoint = Vector2.zero;
        currentEndpoint = null;
        currentDeathbox = null;

        if (levelId > levels.Count)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            // Instantiate level, set spawnpoint
            currentLevel = Instantiate(levels[levelId - 1]);
            spawnPoint = currentLevel.transform.Find("SpawnPoint").position;
            currentEndpoint = currentLevel.transform.Find("LevelEndpoint").gameObject;
            currentDeathbox = currentLevel.transform.Find("TilemapGroup").Find("Deathbox").gameObject;
            currentLevel.transform.parent = gameObject.transform;

            currentPlayer = Instantiate(player, spawnPoint, Quaternion.identity);
            currentPlayer.transform.position = spawnPoint;

            currentIntensity = 1f;
            grayscale.intensity = currentIntensity;
            isColorized = false;
            
            // oh my god fix all of this Find bullshit after PAX please
        }
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
}
