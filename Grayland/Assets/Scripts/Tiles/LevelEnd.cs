using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    [SerializeField, Tooltip("End platform object")]
    MovingPlatform movingPlat;

    bool left = false, right = false, up = false, down = false, startTransition = false, isMovingBarrier = false;
    PlayerController player;
    Vector2 velocity = Vector2.zero;

    void Start()
    {
        // There are only four Vector2 positions an endpoint can be located at:
        // 22,0 (end on right side)
        // -22,0 (end on left side)
        // 0,12 (end on top side)
        // 0,-12 (end on bottom side)
        if (transform.position.x == 22f)
            right = true;
        else if (transform.position.x == -22f)
            left = true;
        else if (transform.position.y == 12f)
            up = true;
        else if (transform.position.y == -12f)
            down = true;
        else
            Debug.LogError("Invalid LevelEnd position");

        player = LevelController.levelController.GetPlayerController();
    }

    void Update()
    {
        if (!startTransition)
        {
            if (right)
            {
                if (player.transform.position.x >= 22f)
                    StartLevelTransition();
            }
            else if (left)
            {
                if (player.transform.position.x <= -22f)
                    StartLevelTransition();
            }
            else if (up)
            {
                if (player.transform.position.y >= 12f)
                    StartLevelTransition();
            }
            else if (down)
            {
                if (player.transform.position.y <= -12f)
                    StartLevelTransition();
            }
        }
    }

    void StartLevelTransition()
    {
        player.ForceSetMoveInput(left, right, up, down);
        startTransition = true;
        StartCoroutine(LevelController.levelController.LoadLevel(true));
    }

    public void MoveEndPlatform()
    {
        movingPlat.SetCanMove(true);
    }
}
