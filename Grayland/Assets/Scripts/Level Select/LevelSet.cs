using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSet", menuName = "Level Set", order = 2)]
public class LevelSet : ScriptableObject
{
    [SerializeField, Tooltip("List of level prefabs")]
    public List<Level> levels;

    [Tooltip("From level selection menu")]
    public int levelToLoad = 0;
}
