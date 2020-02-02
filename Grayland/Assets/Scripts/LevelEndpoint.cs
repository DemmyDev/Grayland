using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEndpoint : MonoBehaviour
{
    [SerializeField, Tooltip("Time until the player is allowed to input a button to move on to the next level")]
    float timeToAllowNextLevel = 3f;

    bool isLevelEnded = false;
    Collider2D col;
    SpriteRenderer spr;

    GameObject levelCompleteUI, nextLevelUI;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        spr = GetComponent<SpriteRenderer>();
        levelCompleteUI = UIController.UIControl.GetCompleteUI();
        nextLevelUI = UIController.UIControl.GetNextLevelUI();

        levelCompleteUI.SetActive(false);
        nextLevelUI.SetActive(false);
    }

    void Update()
    {
        if (isLevelEnded && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Go to next level");
            StartCoroutine(LevelController.levelController.LoadLevel(true));
        }
    }

    public void EndLevel(PlayerController player)
    {
        AudioManager.am.Play("Endpoint");
        Invoke("NextLevel", timeToAllowNextLevel);
        levelCompleteUI.SetActive(true);
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
