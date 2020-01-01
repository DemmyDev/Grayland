using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureTile : MonoBehaviour
{
    [SerializeField] MovingPlatform movingPlatform;
    [SerializeField] bool moveToStart; // True: Move to starting pos / False: Move to ending pos
    [SerializeField] bool canMove; // True: Platform is moving / False: Platform is not moving

    public void ChangeMoveState(bool inverse)
    {
        if (!inverse)
            movingPlatform.SetMoveState(moveToStart, canMove);
        else
            movingPlatform.SetMoveState(!moveToStart, !canMove);
    }
}
