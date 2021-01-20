using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSet", menuName = "Dialogue Set", order = 1)]
public class DialogueSet : ScriptableObject
{
    public enum AnimationMode
    {
        Color = 0,
        Wave = 1,
        Jitter = 2,
        Dangling = 3,
        Reveal = 4,
        ShakeA = 5,
        ShakeB = 6,
        Warp = 7,
        None = 8,
    }
    
    [Header("All")]
    public bool isPlayer;
    [TextArea]
    public string text;

    public float pitchMin;
    public float pitchMax;

    [Range(-.08f, .08f), Tooltip("Direction the eyes look in. Must be value between -.08 and .08. If 0, eyes will look towards the player")]
    public float eyePosX = 0f;

    [Tooltip("How long there is to wait before. Value should at least be .5 if being used. If 0, there is no delay")]
    public float delayTypeTime = 0f;
}
