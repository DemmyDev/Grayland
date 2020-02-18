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

    string currentText = "";
    Text text;
    bool canDisplay = false;
    GameObject dialogueUI;

    #endregion


    void Start()
    {
        dialogueUI = UIController.UIControl.GetDialogueUI();
        text = dialogueUI.GetComponent<Text>();
    }


    // Call when enabling text
    IEnumerator TextWrite()
    {
        // Display text one character at a time
        for (int i = 0; i < dialogue.Length + 1; i++)
        {
            // Break the loop if canDisplay is false
            if (!canDisplay)
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

    // Call when disabling text
    void DisableText()
    {
        currentText = "";
        text.text = "";
    }

    // When the player collides with this object, start writing text
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            canDisplay = true;
            StartCoroutine(TextWrite());
        }
    }

    // When the player exits this object, erase text
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            canDisplay = false;
            DisableText();
        }
    }
}
