using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueWrite : MonoBehaviour
{
    public DialogueSet[] dialogueSets;

    private TextMeshProUGUI m_textMeshPro;
    public float textSpeed = .05f;

    public enum DialogueType
    {
        Player = 0,
        NPC = 1,
    }

    public float distanceToCheck = 5f;

    VertexAttributeModifier vertAnim;
    LevelController levelControl;
    PlayerController player;
    [SerializeField] GameObject activationUI;
    GameObject dialogueUI, dialogueBox;

    bool allowInput = true;
    int setCount = 0;

    private void Start()
    {
        levelControl = LevelController.levelController;
        player = levelControl.GetPlayerController();
        dialogueUI = UIController.UIControl.GetDialogueUI();
        vertAnim = dialogueUI.GetComponent<VertexAttributeModifier>();

        dialogueBox = dialogueUI.transform.parent.gameObject;
        dialogueBox.SetActive(false);
    }

    void Update()
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);

        if (distance <= distanceToCheck)
        {
            // When within distance and player 
            if (Input.GetKeyDown(KeyCode.Return) && allowInput)
            {
                activationUI.SetActive(false);
                dialogueUI.GetComponent<TextMeshProUGUI>().enabled = true;
                allowInput = false;
                player.SetMove(false);
                // Hide activation UI
                // Show dialogue UI
                StartCoroutine(WriteText());
            }
            else if (allowInput)
            {
                activationUI.SetActive(true);
            }
        }
        else if (distance > distanceToCheck)
        {
            activationUI.SetActive(false);
        }
    }

    // Start is called before the first frame update
    public IEnumerator WriteText()
    {
        //vertAnim.PlayAnimation(dialogueSets[setCount]);
        dialogueBox.SetActive(true);
        dialogueUI.GetComponent<TextMeshProUGUI>().text = dialogueSets[setCount].text;

        // Place text over player or NPC
        if (dialogueSets[setCount].isPlayer)
            dialogueBox.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 2f);
        else
            dialogueBox.transform.position = new Vector2(activationUI.transform.position.x, activationUI.transform.position.y);

        m_textMeshPro = dialogueUI.GetComponent<TextMeshProUGUI>();
        m_textMeshPro.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueSets[setCount].text.Length; // Get number of visible characters in the text object
        int counter = 0;

        while (true)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);

            m_textMeshPro.maxVisibleCharacters = visibleCount; // How many characters should be displayed right now?
            
            // When last character is revealed, wait for an input and then start next dialogue set or exit dialogue
            if (visibleCount >= totalVisibleCharacters)
            {
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

                // If not end of dialogue sets
                if (setCount < (dialogueSets.Length - 1))
                {
                    // Move on to next dialogue set and write text again
                    setCount++;
                    vertAnim.Stop();
                    dialogueBox.SetActive(false);
                    dialogueUI.GetComponent<TextMeshProUGUI>().text = "";
                    StartCoroutine(WriteText());
                    yield break;
                }
                else
                {
                    // End the dialogue
                    StartCoroutine(EndDialogue());
                    yield break;
                }
            }

            counter += 1;
            
            if (Input.GetKey(KeyCode.Return))
                yield return new WaitForSeconds(textSpeed / 2);
            else
                yield return new WaitForSeconds(textSpeed);
        }
    }
    public IEnumerator EndDialogue()
    {
        // Reset set count and wait before allowing input
        //vertAnim.Stop();
        dialogueBox.SetActive(false);
        setCount = 0;
        player.SetMove(true);
        dialogueUI.GetComponent<TextMeshProUGUI>().text = "";
        yield return new WaitForSeconds(.5f);
        allowInput = true;
    }
}
