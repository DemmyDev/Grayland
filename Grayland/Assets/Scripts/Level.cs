using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level
{
    [Tooltip("Object of level")]
    public GameObject prefab;

    public enum LevelState { Locked = 0, Unlocked = 1, Completed = 2 };

    [Tooltip("Level locked = 0, level unlocked = 1, level completed = 2")]
    public LevelState state;
}
