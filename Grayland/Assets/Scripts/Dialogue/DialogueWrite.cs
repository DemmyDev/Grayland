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

    VertexAttributeModifier vertAnim;
    LevelController levelControl;
    PlayerController player;
    [SerializeField] Transform eyes;
    [SerializeField] Transform targetPos;
    [SerializeField] Transform distanceCheckL;
    [SerializeField] Transform distanceCheckR;
    [SerializeField] Transform distanceCheckU;
    [SerializeField] Transform distanceCheckD;
    GameObject dialogueUI, dialogueBox, dialogueTri, dialogueParent, activationUI;
    Animation dialogueAnim, activationAnim;
    float initEyeSizeY;

    bool allowInput = true, isBlinking = false, playEnter = false, playExit = false;
    float posL, posR, posU, posD;
    int setCount = 0;

    private void Start()
    {
        levelControl = LevelController.levelController;
        player = levelControl.GetPlayerController();
        dialogueUI = UIController.UIControl.GetDialogueUI();
        dialogueTri = UIController.UIControl.GetDialogueTri();
        dialogueParent = dialogueTri.transform.parent.gameObject;
        activationUI = UIController.UIControl.GetActivationUI();
        activationAnim = activationUI.GetComponent<Animation>();
        dialogueAnim = dialogueParent.GetComponent<Animation>();
        vertAnim = dialogueUI.GetComponent<VertexAttributeModifier>();

        dialogueBox = dialogueUI.transform.parent.gameObject;
        dialogueBox.SetActive(false);
        dialogueTri.SetActive(false);
        initEyeSizeY = eyes.localScale.y;
        StartCoroutine(EyeBlink());

        activationUI.transform.position = new Vector2(transform.position.x, transform.position.y + 2f);

        posL = distanceCheckL.position.x;
        posR = distanceCheckR.position.x;
        posU = distanceCheckU.position.y;
        posD = distanceCheckD.position.y;
    }

    void Update()
    {
        Vector2 playerPos = player.transform.position;
        
        if ((playerPos.x > posL && playerPos.x < posR) && (playerPos.y > posD && playerPos.y < posU))
        {
            float dif = transform.position.x - player.transform.position.x;

            if (dif >= 0)
                eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, -.08f, .25f), 0f);
            else
                eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, .08f, .25f), 0f);

            // When within distance and player 
            if (Input.GetKeyDown(KeyCode.Return) && allowInput)
            {

                activationUI.SetActive(false);
                allowInput = false;
                player.SetMove(false);
                //player.transform.position = targetPos.position;
                StartCoroutine(PlayerToTargetPos());
            }
            else if (allowInput)
            {
                activationUI.SetActive(true);

                if (!playEnter)
                {
                    playEnter = true;
                    playExit = false;
                    activationAnim.Stop("ActivationExit");
                    activationAnim.Play("ActivationEnter");
                }
            }
        }
        else
        {
            eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, 0f, .25f), 0f);

            if (!playExit)
            {
                playEnter = false;
                playExit = true;
                activationAnim.Stop("ActivationEnter");
                activationAnim.Play("ActivationExit");
            }    
            //activationUI.SetActive(false);
        }

        Blinking();
    }

    // Start is called before the first frame update
    public IEnumerator WriteText()
    {
        //vertAnim.PlayAnimation(dialogueSets[setCount]);
        dialogueBox.SetActive(true);
        dialogueTri.SetActive(true);

        // Place text over player or NPC
        if (dialogueSets[setCount].isPlayer)
            dialogueParent.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 3f);
        else
            dialogueParent.transform.position = new Vector2(activationUI.transform.position.x, activationUI.transform.position.y + 1f);

        //dialogueTri.transform.position = new Vector2(dialogueBox.transform.position.x, dialogueBox.transform.position.y);

        m_textMeshPro = dialogueUI.GetComponent<TextMeshProUGUI>();
        m_textMeshPro.ForceMeshUpdate();

        dialogueAnim.Play("DialogueEnter");
        yield return new WaitForSeconds(1/5f);
        dialogueAnim.Stop("DialogueEnter");

        dialogueUI.GetComponent<TextMeshProUGUI>().text = dialogueSets[setCount].text;

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
                    dialogueUI.GetComponent<TextMeshProUGUI>().text = "";

                    dialogueAnim.Play("DialogueExit");
                    yield return new WaitForSeconds(1/6f);
                    dialogueAnim.Stop("DialogueExit");

                    dialogueBox.SetActive(false);
                    dialogueTri.SetActive(false);
                    StartCoroutine(WriteText());
                    yield break;
                }
                else
                {
                    // End the dialogue
                    dialogueAnim.Play("DialogueExit");
                    yield return new WaitForSeconds(1/6f);
                    dialogueAnim.Stop("DialogueExit");

                    StartCoroutine(EndDialogue());
                    yield break;
                }
            }

            counter += 1;

            AudioManager.am.Play("Type", Random.Range(dialogueSets[setCount].pitchMin, dialogueSets[setCount].pitchMax));

            if (Input.GetKey(KeyCode.Return))
            {
                yield return new WaitForSeconds(textSpeed / 2);
            }
            else
            {
                yield return new WaitForSeconds(textSpeed);
            }
        }
    }

    public IEnumerator EndDialogue()
    {
        // Reset set count and wait before allowing input
        //vertAnim.Stop();
        dialogueBox.SetActive(false);
        dialogueTri.SetActive(false);
        setCount = 0;
        player.SetMove(true);
        dialogueUI.GetComponent<TextMeshProUGUI>().text = "";
        yield return new WaitForSeconds(.5f);
        allowInput = true;

        playEnter = true;
        playExit = false;
        activationAnim.Stop("ActivationExit");
        activationAnim.Play("ActivationEnter");

        StartCoroutine(UIController.UIControl.GetActivationText().GetComponent<ThreeDotType>().Type());
    }

    void Blinking()
    {
        // Eyes blinking
        if (isBlinking)
        {
            eyes.localScale = new Vector2(eyes.localScale.x, Mathf.Lerp(eyes.localScale.y, 0f, .33f));

            if (eyes.localScale.y <= .025f)
            {
                isBlinking = false;
            }
        }
        else
            eyes.localScale = new Vector2(eyes.localScale.x, Mathf.Lerp(eyes.localScale.y, initEyeSizeY, .33f));

        // Hotfix for alleviating lerp issues relative to scale
        if (eyes.localScale.y >= .995f)
            eyes.localScale = new Vector2(eyes.localScale.x, initEyeSizeY);
    }

    IEnumerator EyeBlink()
    {
        int i = Random.Range(3, 6);
        yield return new WaitForSeconds(i);
        isBlinking = true;
        StartCoroutine(EyeBlink()); // Loops blink
    }

    IEnumerator PlayerToTargetPos()
    {
        yield return new WaitUntil(() => player.GetGrounded());
        player.ForceSetMoveInput(targetPos.position.x);
        yield return new WaitUntil(() => !player.MovingToTargetPos());
        player.SetEyesTargetPos(transform);
        StartCoroutine(WriteText());
    }
}
