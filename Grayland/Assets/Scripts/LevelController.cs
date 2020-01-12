using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class LevelController : MonoBehaviour
{
    #region Variables

    [SerializeField, Tooltip("Player prefab")]
    GameObject player;

    [SerializeField, Tooltip("Transition object (child of this object)")]
    GameObject transition;

    [SerializeField, Tooltip("Post-processing object")]
    PostProcessVolume postProcess;

    [SerializeField, Tooltip("List of level prefabs")]
    List<GameObject> levels;

    Vector2 spawnPoint;
    GameObject currentLevel, currentPlayer, currentEndpoint;
    int levelId = 1;
    float currentSat = -100;
    bool isColorized = false;
    Animation anim;
    static public LevelController levelController;
    
    #endregion

    void Awake()
    {
        if (levelController == null) levelController = this;
        anim = transition.GetComponent<Animation>();
        LevelTransition(false);
    }

    // Only use for lerping at the end of a level
    void Update()
    {
        if (isColorized)
        {
            currentSat = Mathf.Lerp(currentSat, 0f, Time.deltaTime);
            postProcess.profile.GetSetting<ColorGrading>().saturation.value = currentSat; // Yeah apparently having ColorGrading be stored as a variable results in a NullReference :(
        }
    }

    /// <summary>
    /// Destroys current level and either reloads the level or loads the next level
    /// </summary>
    /// <param name="isNextLevel">If true, goes to the next level in the levels list. If false, restarts current level</param>
    public IEnumerator LoadLevel(bool isNextLevel)
    {
        Debug.Log("Got to transition");
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);
        Debug.Log("Transition!");
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

        // Reset values
        currentLevel = null;
        spawnPoint = Vector2.zero;
        currentEndpoint = null;

        if (levelId > levels.Count)
        {
            Debug.Log("Last level! Do 'thanks for playing' stuff");
        }
        else
        {
            // Instantiate level, set spawnpoint
            currentLevel = Instantiate(levels[levelId - 1]);
            spawnPoint = currentLevel.transform.Find("SpawnPoint").position;
            currentEndpoint = currentLevel.transform.Find("LevelEndpoint").gameObject;
            currentLevel.transform.parent = gameObject.transform;

            currentPlayer = Instantiate(player, spawnPoint, Quaternion.identity);
            currentPlayer.transform.position = spawnPoint;

            currentSat = -100f;
            postProcess.profile.GetSetting<ColorGrading>().saturation.value = currentSat;
            isColorized = false;
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
}
