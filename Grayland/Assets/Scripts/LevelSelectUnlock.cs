using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectUnlock : MonoBehaviour
{
    [SerializeField] LevelSet levelSet;
    [SerializeField] GameObject signObj;
    [SerializeField] public int levelId;

    void Start()
    {
        // Level is locked
        if (levelSet.levels[levelId - 1].state == (Level.LevelState)0)
        {
            gameObject.SetActive(true);
            signObj.SetActive(false);
        }
        // Level is unlocked or completed
        else
        {
            gameObject.SetActive(false);
            signObj.SetActive(true);
        }
        // Include separate check for completed levels in future
    }
}
