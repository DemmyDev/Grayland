﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreativeSpore.SuperTilemapEditor;

public class LevelEndpoint : MonoBehaviour
{
    [SerializeField, Tooltip("Time until the player is allowed to input a button to move on to the next level")]
    float timeToAllowNextLevel = 3f;

    bool isLevelEnded = false;
    Collider2D col;
    SpriteRenderer spr;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        spr = GetComponent<SpriteRenderer>();
        LevelController.levelController.GetLevelEnd().enabled = false;
    }

    public void EndLevel(PlayerController player)
    {
        AudioManager.am.Play("Endpoint");

        STETilemap deathbox = LevelController.levelController.GetDeathbox().GetComponent<STETilemap>();
        if (deathbox != null)
        {
            deathbox.ColliderType = eColliderType.None;
            deathbox.Refresh(false, true);
        }
        
        LevelController.levelController.GetLevelEnd().enabled = true;
        LevelController.levelController.SetLevelState(2);
        LevelController.levelController.GetLevelEnd().MoveEndPlatform();

        player.SetStickyChild(spr.sprite);
        spr.enabled = false;
        col.enabled = false;
    }
}
