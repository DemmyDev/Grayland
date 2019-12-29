using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTile : MonoBehaviour
{
    [SerializeField] GameObject lockTile;
    /// <summary>
    /// Switches the active state of the lock
    /// </summary>
    public void ChangeLockState()
    {
        if (lockTile.activeSelf)
            lockTile.SetActive(false);
        else
            lockTile.SetActive(true);
    }
}
