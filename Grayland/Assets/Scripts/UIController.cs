using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject completeUI;
    [SerializeField] GameObject nextLevelUI;
    [SerializeField] GameObject dialogueUI;
    [SerializeField] GameObject dialogueTri;
    [SerializeField] GameObject activationUI;
    [SerializeField] GameObject activationText;

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

    public GameObject GetDialogueTri()
    {
        return dialogueTri;
    }

    public GameObject GetActivationUI()
    {
        return activationUI;
    }

    public GameObject GetActivationText()
    {
        return activationText;
    }
}
