using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectID : MonoBehaviour
{
    [Tooltip("Level ID: Range 1 - ???, Scene: Range 2 - 5")] public int selectNum = 1;
    [SerializeField, Tooltip("If true, selects a scene. If false, selects a level")] bool isSceneSelect = false;
    [SerializeField] LayerMask whatIsPlayer;
    [SerializeField] bool left;
    [SerializeField] bool right;
    [SerializeField] bool up;
    [SerializeField] bool down;
    
    bool checkForPlayer = true;
    LevelSelector levelSelectParent;
    LevelSceneSelector sceneSelectParent;

    void Start()
    {
        levelSelectParent = gameObject.GetComponentInParent<LevelSelector>();
    }

    private void FixedUpdate()
    {
        RaycastHit2D playerCheck = Physics2D.BoxCast(transform.position, new Vector2(3f, 4f), 0f, Vector2.zero, 0f, whatIsPlayer);
        
        if (playerCheck && checkForPlayer)
        {
            checkForPlayer = false;

            levelSelectParent.LevelTransition(selectNum, isSceneSelect, left, right, up, down);
        }
    }
}
