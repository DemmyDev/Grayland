using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WriteText : MonoBehaviour
{
    #region Variables

    [SerializeField, Tooltip("Time in-between character being written")]
    float delay = .05f;

    [SerializeField, Tooltip("The text that will be fully displayed")]
    string dialogue;

    [SerializeField, Tooltip("How close the player needs to be to the object to start displaying dialogue")]
    float distanceCheck;

    [SerializeField, Tooltip("Where the text will be aligned when writing")]
    TextAlignment alignment;

    [SerializeField, Tooltip("Font size of text")]
    int size;

    [SerializeField, Tooltip("Where the text object will be placed (RectTransform)")]
    Vector3 textPosition;

    [SerializeField, Tooltip("The size of the text object")]
    Vector3 textBoxScale;

    string currentText = "";
    Text text;
    bool isDisplaying = false;
    GameObject dialogueUI;
    LevelController levelControl;
    PlayerController player;

    #endregion


    void Start()
    {
        levelControl = LevelController.levelController;
        player = levelControl.GetPlayerController();
        dialogueUI = UIController.UIControl.GetDialogueUI();
        text = dialogueUI.GetComponent<Text>();
    }

    void Update()
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);

        // If player is outside of range
        if (distance >= distanceCheck && isDisplaying)
        {
            isDisplaying = false;
            currentText = "";
            text.text = "";
        }
        // If player is within range
        else if (distance < distanceCheck && !isDisplaying)
        {
            isDisplaying = true;
            StartCoroutine(TextWrite());
        }
    }


    // Call when enabling text
    IEnumerator TextWrite()
    {
        // Display text one character at a time
        for (int i = 0; i < dialogue.Length + 1; i++)
        {
            // Break the loop if isDisplaying is false
            if (!isDisplaying)
            {
                currentText = "";
                text.text = "";
                break;
            }

            currentText = dialogue.Substring(0, i);
            text.text = currentText;
            yield return new WaitForSeconds(delay);
        }
    }
}