using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndpoint : MonoBehaviour
{
    [SerializeField, Tooltip("Time until the player is allowed to input a button to move on to the next level")]
    float timeToAllowNextLevel = 3f;

    [SerializeField, Tooltip("UI enabled immediately after entering endpoint object")]
    GameObject levelBeatUI;

    [SerializeField, Tooltip("UI enabled after timeToAllownextLevel time")]
    GameObject nextLevelUI;

    bool isLevelEnded = false;
    LevelController levelControl;
    Collider2D col;
    SpriteRenderer spr;
    

    private void Start()
    {
        col = GetComponent<Collider2D>();
        spr = GetComponent<SpriteRenderer>();
        levelBeatUI.SetActive(false);
        nextLevelUI.SetActive(false);
    }

    void Update()
    {
        if (isLevelEnded && Input.GetKeyDown(KeyCode.Return))
        {
            levelControl.LoadLevel(0);
        }
    }

    public void GrabLevelController(LevelController controller)
    {
        levelControl = controller;
    }

    public void EndLevel(PlayerController player)
    {
        Invoke("NextLevel", timeToAllowNextLevel);
        levelBeatUI.SetActive(true);
        player.SetStickyChild(spr.sprite);
        spr.enabled = false;
        col.enabled = false;
    }

    void NextLevel()
    {
        isLevelEnded = true;
        nextLevelUI.SetActive(true);
    }
}
