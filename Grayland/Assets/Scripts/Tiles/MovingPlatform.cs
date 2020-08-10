using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Transform platform;
    [SerializeField] Transform endPoint;
    Vector2 startPointPos, endPointPos;

    [SerializeField] float maxSpeed;
    [SerializeField] float smoothTime;

    // If true, move to startpoint. If false, move to endpoint.
    [SerializeField] bool moveToStart = false;

    // If true, platform stops current movement
    [SerializeField] bool canMove = true;

    // If true, the platform stops as soon as it reaches the endpoint
    [SerializeField] bool endPlatform = false;

    Vector2 target;
    Vector2 velocity = Vector2.zero;

    void Start()
    {
        startPointPos = platform.position;
        endPointPos = endPoint.position;
    }

    void Update()
    {
        if (endPlatform)
        {
            if (canMove)
            {
                target = endPointPos;
                Debug.Log("Before: " + platform.position);
                platform.position = Vector2.SmoothDamp(platform.position, target, ref velocity, smoothTime, maxSpeed);
                Debug.Log("After: " + platform.position);
                float distance = Vector2.Distance(platform.position, endPointPos);
                Debug.Log(distance);
                if (distance < .05)
                {
                    platform.position = endPointPos;
                    canMove = false;
                }
            }
        }
        else
        {
            if (canMove)
            {
                if (moveToStart)
                {
                    target = startPointPos;
                    platform.position = Vector2.SmoothDamp(platform.position, target, ref velocity, smoothTime, maxSpeed);
                    float distance = Vector2.Distance(platform.position, startPointPos);
                    if (distance < .05)
                        moveToStart = !moveToStart;
                }
                else if (!moveToStart)
                {
                    target = endPointPos;
                    platform.position = Vector2.SmoothDamp(platform.position, target, ref velocity, smoothTime, maxSpeed);
                    float distance = Vector2.Distance(platform.position, endPointPos);
                    if (distance < .05)
                        moveToStart = !moveToStart;
                }
            }
        }
    }

    /// <summary>
    /// Sets move states of platform.
    /// </summary>
    /// <param name="moveState">If true, move to startpoint. If false, move to endpoint</param>
    /// <param name="canMovePlatform">Stops movement if false. Continues movement if true</param>
    public void SetMoveState(bool moveState, bool canMovePlatform)
    {
        moveToStart = moveState;
        canMove = canMovePlatform;
    }

    public void SetCanMove(bool canMovePlatform)
    {
        canMove = canMovePlatform;
    }
}
