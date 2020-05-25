using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSetClass
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

    public bool isPlayer;
    [TextArea]
    public string dialogue = "";

    [Header("All")]
    public bool PlayOnEnable = true;
    public AnimationMode textAnimMode = AnimationMode.Jitter;

    public AnimationCurve VertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));

    public float AngleMultiplier = 1.0f;
    public float SpeedMultiplier = 1.0f;
    public float CurveScale = 1.0f;

    [Header("Dangle")]
    [Range(.01f, .1f)] public float DangleRefresh = .05f;
    public Vector2 DanglingRange = new Vector2(10f, 25f);
    public Vector2 DanglingSpeed = new Vector2(1f, 3f);

    [Header("Shake")]
    public float ScaleMultiplier = 1.0f;
    public float RotationMultiplier = 1.0f;
}
