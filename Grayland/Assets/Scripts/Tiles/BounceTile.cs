using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceTile : MonoBehaviour
{
    [SerializeField, Tooltip("If true, the tile boosts the player on X. If false, it boosts on Y")] bool isXAxis;

    /// <summary>
    /// If true, the tile boosts the player on X. If false, it boosts on Y
    /// </summary>
    /// <returns></returns>
    public bool GetAxis()
    {
        return isXAxis;
    }
}
