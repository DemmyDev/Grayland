using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject dialogueUI;
    [SerializeField] GameObject dialogueTri;
    [SerializeField] GameObject activationUI;
    [SerializeField] GameObject activationText;
    [SerializeField] GameObject pauseParent;
    [SerializeField] GameObject creditsParent;

    public static UIController UIControl;

    void Awake()
    {
        if (UIControl == null)
            UIControl = this;
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

    public GameObject GetPauseParent()
    {
        return pauseParent;
    }

    public GameObject GetCreditsParent()
    {
        return creditsParent;
    }
}
