﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    #region Variables

    [SerializeField, Tooltip("Player prefab")]
    GameObject player;

    [SerializeField, Tooltip("List of level prefabs")]
    List<GameObject> levels;

    Vector2 spawnPoint;
    GameObject currentLevel, currentPlayer;
    int levelId = 0;

    #endregion

    private void Awake()
    {
        LoadLevel(0);
    }

    /// <summary>
    /// Destroys current level and player and instantiates new level and player
    /// </summary>
    /// <param name="newLevelId">ID of level to be spawned</param>
    public void LoadLevel(int newLevelId)
    {
        // Destroy current level
        if (currentLevel != null) Destroy(currentLevel);
        if (currentPlayer != null) Destroy(currentPlayer);

        // Reset values
        levelId = newLevelId; // Shouldn't change if restarting the current level
        currentLevel = null;
        spawnPoint = Vector2.zero;

        // Instantiate level, set spawnpoint
        currentLevel = Instantiate(levels[levelId]);
        spawnPoint = currentLevel.transform.Find("SpawnPoint").position;
        currentLevel.transform.parent = gameObject.transform;

        currentPlayer = Instantiate(player, spawnPoint, Quaternion.identity);
        currentPlayer.transform.position = spawnPoint;
        currentPlayer.GetComponent<PlayerController>().GrabLevelController(this);
    }

    /// <summary>
    /// Gets the current level ID
    /// </summary>
    /// <returns>Current level ID</returns>
    public int GetCurrentLevelId()
    {
        return levelId;
    }
}