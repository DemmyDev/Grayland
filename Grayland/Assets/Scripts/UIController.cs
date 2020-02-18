using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject completeUI;
    [SerializeField] GameObject nextLevelUI;
    [SerializeField] GameObject dialogueUI;

    public static UIController UIControl;

    void Awake()
    {
        if (UIControl == null)
            UIControl = this;
    }

    public GameObject GetCompleteUI()
    {
        return completeUI;
    }

    public GameObject GetNextLevelUI()
    {
        return nextLevelUI;
    }

    public GameObject GetDialogueUI()
    {
        return dialogueUI;
    }
}
