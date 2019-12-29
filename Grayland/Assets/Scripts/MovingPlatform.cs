using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Transform platform;
    [SerializeField] Transform endPoint;
    Vector2 startPointPos, endPointPos;

    [SerializeField] float platformSpeed;

    // If true, move to startpoint. If false, move to endpoint.
    bool moveToStart = false;

    // If true, platform stops current movement
    bool canMove = true;

    void Start()
    {
        startPointPos = platform.transform.position;
        endPointPos = endPoint.position;
    }

    void Update()
    {
        if (canMove)
        {
            if (moveToStart)
            {
                platform.position = new Vector2(Mathf.Lerp(platform.position.x, startPointPos.x, platformSpeed), Mathf.Lerp(platform.position.y, startPointPos.y, platformSpeed));
            }
            else if (!moveToStart)
            {
                platform.position = new Vector2(Mathf.Lerp(platform.position.x, endPointPos.x, platformSpeed), Mathf.Lerp(platform.position.y, endPointPos.y, platformSpeed));
            }
        }
    }

    /// <summary>
    /// Sets move states of platform.
    /// </summary>
    /// <param name="moveState">If true, move to startpoint. If false, move to endpoint</param>
    /// <param name="canMovePlatform">Stops </param>
    public void SetMoveState(bool moveState, bool canMovePlatform)
    {
        moveToStart = moveState;
        canMove = canMovePlatform;
    }
}
