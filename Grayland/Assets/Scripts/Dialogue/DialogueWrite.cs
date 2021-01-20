using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueWrite : MonoBehaviour
{
    public DialogueSet[] dialogueSetsNoColor;
    public DialogueSet[] dialogueSetsColor;

    private TextMeshProUGUI m_textMeshPro;
    public float textSpeed = .05f;

    public enum DialogueType
    {
        Player = 0,
        NPC = 1,
    }

    //VertexAttributeModifier vertAnim;
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

    bool allowInput = true, isBlinking = false, playEnter = false, playExit = false, isTyping = false;
    float posL, posR, posU, posD, forcedEyeDirection = 0f;
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
        //vertAnim = dialogueUI.GetComponent<VertexAttributeModifier>();

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
            /*
            if (forcedEyeDirection == 0f)
            {
                float dif = transform.position.x - player.transform.position.x;

                if (dif >= 0)
                    eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, -.08f, .25f / 20), 0f);
                else
                    eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, .08f, .25f / 20), 0f);
            }
            else
            {
                eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, forcedEyeDirection, .25f / 20), 0f);
            }*/

            // When within distance and player 
            if (Input.GetButtonDown("Interact") && allowInput)
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
            //eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, 0f, .25f / 20), 0f);

            if (!playExit)
            {
                playEnter = false;
                playExit = true;
                activationAnim.Stop("ActivationEnter");
                activationAnim.Play("ActivationExit");
            }    
            //activationUI.SetActive(false);
        }

        //Blinking();
    }

    // Start is called before the first frame update
    public IEnumerator WriteText()
    {
        dialogueBox.SetActive(true);
        dialogueTri.SetActive(true);
        dialogueUI.SetActive(true);

        DialogueSet[] dialogueSets;
        if (levelControl.GetIsColorized())
            dialogueSets = dialogueSetsColor;
        else
            dialogueSets = dialogueSetsNoColor;

        // Place text over player or NPC
        if (dialogueSets[setCount].isPlayer)
            dialogueParent.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 3f);
        else
            dialogueParent.transform.position = new Vector2(activationUI.transform.position.x, activationUI.transform.position.y + 1f);

        //dialogueTri.transform.position = new Vector2(dialogueBox.transform.position.x, dialogueBox.transform.position.y);

        m_textMeshPro = dialogueUI.GetComponent<TextMeshProUGUI>();
        m_textMeshPro.ForceMeshUpdate();

        if (dialogueSets[setCount].delayTypeTime != 0)
        {
            dialogueBox.SetActive(false);
            dialogueTri.SetActive(false);
            dialogueUI.SetActive(false);
            yield return new WaitForSeconds(dialogueSets[setCount].delayTypeTime);
            dialogueBox.SetActive(true);
            dialogueTri.SetActive(true);
            dialogueUI.SetActive(true);
        }

        forcedEyeDirection = dialogueSets[setCount].eyePosX;

        dialogueAnim.Play("DialogueEnter");
        yield return new WaitForSeconds(1/5f);
        dialogueAnim.Stop("DialogueEnter");

        dialogueUI.GetComponent<TextMeshProUGUI>().text = dialogueSets[setCount].text;

        isTyping = true;
        StartCoroutine(TypeAudio(dialogueSets, textSpeed, setCount));

        int totalVisibleCharacters = dialogueSets[setCount].text.Length; // Get number of visible characters in the text object
        int counter = 0;

        while (true)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);

            m_textMeshPro.maxVisibleCharacters = visibleCount; // How many characters should be displayed right now?

            // When last character is revealed, wait for an input and then start next dialogue set or exit dialogue
            if (visibleCount >= totalVisibleCharacters)
            {
                isTyping = false;

                yield return new WaitUntil(() => Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump"));

                // If not end of dialogue sets
                if (setCount < (dialogueSets.Length - 1))
                {
                    // Move on to next dialogue set and write text again
                    setCount++;
                    //vertAnim.Stop();
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
                    forcedEyeDirection = 0f;

                    StartCoroutine(EndDialogue());
                    yield break;
                }
            }

            counter += 1;

            if (Input.GetButton("Interact") || Input.GetButton("Jump"))
            {
                yield return new WaitForSeconds(textSpeed / 4);
            }
            else
            {
                yield return new WaitForSeconds(textSpeed);
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 playerPos = player.transform.position;

        if ((playerPos.x > posL && playerPos.x < posR) && (playerPos.y > posD && playerPos.y < posU))
        {
            if (forcedEyeDirection == 0f)
            {
                float dif = transform.position.x - player.transform.position.x;

                if (dif >= 0)
                    eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, -.08f, .25f), 0f);
                else
                    eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, .08f, .25f), 0f);
            }
            else
            {
                eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, forcedEyeDirection, .25f), 0f);
            }
        }
        else
        {
            eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, 0f, .25f), 0f);
        }

        Blinking();
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

    IEnumerator TypeAudio(DialogueSet[] sets, float speed, int set)
    {
        AudioManager.am.Play("Type", Random.Range(sets[set].pitchMin, sets[set].pitchMax));
        yield return new WaitForSeconds(speed);
        AudioManager.am.Stop("Type");
        if (isTyping) StartCoroutine(TypeAudio(sets, speed, set));
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
