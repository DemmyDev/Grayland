using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectLevelData : MonoBehaviour
{
    [SerializeField] LevelSet levelSet;
    [SerializeField, Tooltip("Check to true if you want to reset all data")] bool isClearingData;

    void Awake()
    {
        CollectData();
    }

    public void CollectData()
    {
        if (isClearingData)
        {
            PlayerPrefs.DeleteAll();

            // Set states for each level
            for (int i = 0; i < levelSet.levels.Count; i++)
            {
                // First level is always unlocked
                if (i == 0)
                {
                    levelSet.levels[i].state = (Level.LevelState)1;
                }
                // Each other level is locked
                else
                {
                    levelSet.levels[i].state = (Level.LevelState)0;
                }

                // Setting string name for PlayerPrefs. Must be consistent through all other scripts
                int n = i + 1;
                string levelState = "level" + n + "State"; // EX: level1State

                // Create value in PlayerPrefs
                PlayerPrefs.SetInt(levelState, (int)levelSet.levels[i].state);
            }

            isClearingData = false;
        }
        else
        {
            for (int i = 1; i < levelSet.levels.Count + 1; i++)
            {
                // Setting string name for PlayerPrefs. Must be consistent through all other scripts
                string levelState = "level" + i + "State"; // EX: level1State

                // Get value from PlayerPrefs and set value in scriptable
                levelSet.levels[i - 1].state = (Level.LevelState)PlayerPrefs.GetInt(levelState);
            }
        }
    }
}
