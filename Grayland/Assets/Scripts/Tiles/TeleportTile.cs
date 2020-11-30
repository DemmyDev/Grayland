using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTile : MonoBehaviour
{
    [SerializeField, Tooltip("Both teleporters cooperate with each other")] TeleportTile otherTeleporter;
    [SerializeField, Tooltip("The position where the player will spawn")] Transform teleportPos;

    /// <summary>
    /// Call to determine the position of where the player should be placed
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTeleportPos()
    {
        return teleportPos.position;
    }

    /// <summary>
    /// Get the teleporter for where the player is going to end up
    /// </summary>
    /// <returns></returns>
    public TeleportTile GetOtherTeleporter()
    {
        return otherTeleporter;
    }
}
