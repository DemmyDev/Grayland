using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueWrite : MonoBehaviour
{
    public enum ActivateType
    {
        Input = 0,
        Trigger = 1,
    }

    [SerializeField, Tooltip("")] int charID;
    public DialogueSet[] dialogueSetsNoColor;
    public DialogueSet[] dialogueSetsColor;
    public DialogueSet[] dialogueSetsCutscene;

    private TextMeshProUGUI m_textMeshPro;
    public float textSpeed = .05f;

    LevelController levelControl;
    PlayerController player;
    [SerializeField] ActivateType activateType;
    [SerializeField] bool isCutscene = false;
    [SerializeField] Transform eyes;
    [SerializeField] Transform targetPos;
    [SerializeField] Transform distanceCheckL;
    [SerializeField] Transform distanceCheckR;
    [SerializeField] Transform distanceCheckU;
    [SerializeField] Transform distanceCheckD;
    [SerializeField] ParticleSystem npcParticle;
    GameObject dialogueUI, dialogueBox, dialogueTri, dialogueParent, activationUI;
    Animation dialogueAnim, activationAnim;
    float initEyeSizeY;

    bool allowInput = true, isBlinking = false, playEnter = false, playExit = false, isTyping = false, isMovingChar = false, hasDoneCutscene = false, isColoringPlayer = false, playParticle = false;
    float posL, posR, posU, posD, forcedEyeDirection = 0f;
    int setCount = 0;
    float moveDir = 0f, velocityX = 0, initPosX = 0, targetPosX = 0;
    string npcActivateType;

    private void Start()
    {
        npcActivateType = "npc" + charID + "ActivateType";
        activateType = (ActivateType)ReadWriteSaveManager.Instance.GetData(npcActivateType, (int)activateType);

        ReadWriteSaveManager.Instance.Write();

        levelControl = LevelController.levelController;
        player = levelControl.GetPlayerController();
        dialogueUI = UIController.UIControl.GetDialogueUI();
        dialogueTri = UIController.UIControl.GetDialogueTri();
        dialogueParent = dialogueTri.transform.parent.gameObject;
        activationUI = UIController.UIControl.GetActivationUI();
        activationAnim = activationUI.GetComponent<Animation>();
        dialogueAnim = dialogueParent.GetComponent<Animation>();

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

        npcParticle.gameObject.GetComponent<Renderer>().material.SetColor("_Color", GetComponent<SpriteRenderer>().color);
        npcParticle.gameObject.SetActive(false);
    }

    void Update()
    {
        Vector2 playerPos = player.transform.position;

        if ((playerPos.x > posL && playerPos.x < posR) && (playerPos.y > posD && playerPos.y < posU))
        {
            if (activateType == 0)
            {
                // When within distance and player 
                if (Input.GetButtonDown("Interact") && allowInput)
                {
                    activationUI.SetActive(false);
                    allowInput = false;
                    player.SetMove(false);
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
            else if (activateType == (ActivateType)1 && allowInput)
            {
                activationUI.SetActive(false);
                allowInput = false;
                player.SetMove(false);
                StartCoroutine(PlayerToTargetPos());
            }
        }
        else
        {
            if (!playExit)
            {
                playEnter = false;
                playExit = true;
                activationAnim.Stop("ActivationEnter");
                activationAnim.Play("ActivationExit");
            }    
        }
    }

    // Start is called before the first frame update
    public IEnumerator WriteText()
    {
        DialogueSet[] dialogueSets;

        if (isCutscene && !hasDoneCutscene)
            dialogueSets = dialogueSetsCutscene;
        else
        {
            if (levelControl.GetIsColorized())
                dialogueSets = dialogueSetsColor;
            else
                dialogueSets = dialogueSetsNoColor;
        }

        // Place text over player or NPC
        if (dialogueSets[setCount].isPlayer)
            dialogueParent.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 3f);
        else
            dialogueParent.transform.position = new Vector2(transform.position.x, transform.position.y + 3f);

        m_textMeshPro = dialogueUI.GetComponent<TextMeshProUGUI>();
        m_textMeshPro.ForceMeshUpdate();

        forcedEyeDirection = dialogueSets[setCount].eyePosX;

        // Dialogue
        if (dialogueSets[setCount].setType == (DialogueSet.SetType)0)
        {
            dialogueBox.SetActive(true);
            dialogueTri.SetActive(true);
            dialogueUI.SetActive(true);

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

            dialogueUI.GetComponent<TextMeshProUGUI>().text = "";

            dialogueAnim.Play("DialogueEnter");
            yield return new WaitForSeconds(1 / 5f);
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
                        yield return new WaitForSeconds(1 / 6f);
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
                        yield return new WaitForSeconds(1 / 6f);
                        dialogueAnim.Stop("DialogueExit");
                        forcedEyeDirection = 0f;

                        if (isCutscene && !hasDoneCutscene)
                        {
                            hasDoneCutscene = true;
                            activateType = 0;
                        }

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
        // Move
        else if (dialogueSets[setCount].setType == (DialogueSet.SetType)1)
        {
            npcParticle.gameObject.SetActive(true);
            moveDir = dialogueSets[setCount].moveDir;
            initPosX = transform.position.x;
            targetPosX = moveDir + initPosX;
            isMovingChar = true;

            yield return new WaitUntil(() => !isMovingChar);

            npcParticle.gameObject.SetActive(false);

            // Continue to next dialogue set
            if (setCount < (dialogueSets.Length - 1))
            {
                setCount++;
                StartCoroutine(WriteText());
            }
            // End dialogue
            else
            {
                forcedEyeDirection = 0f;
                if (isCutscene && !hasDoneCutscene)
                {
                    hasDoneCutscene = true;
                    activateType = 0;
                    ReadWriteSaveManager.Instance.SetData(npcActivateType, (int)activateType);
                    ReadWriteSaveManager.Instance.Write();
                }
                StartCoroutine(EndDialogue());
            }
        }
        // Color Player
        else if (dialogueSets[setCount].setType == (DialogueSet.SetType)2)
        {
            isColoringPlayer = true;

            yield return new WaitUntil(() => !isColoringPlayer);

            // Continue to next dialogue set
            if (setCount < (dialogueSets.Length - 1))
            {
                setCount++;
                StartCoroutine(WriteText());
            }
        }
        // Credits (only use in final scene)
        else if (dialogueSets[setCount].setType == (DialogueSet.SetType)3)
        {
            // Enable UI animation for credits
            // After animation is finished, go to main menu scene
            StartCoroutine(CreditsSequence());
        }
    }

    void FixedUpdate()
    {
        EyeDirection();
        Blinking();
        MoveChar();
        ColorPlayer();
    }

    public IEnumerator EndDialogue()
    {
        // Reset set count and wait before allowing input
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

    IEnumerator CreditsSequence()
    {
        GameObject credits = UIController.UIControl.GetCreditsParent();
        credits.SetActive(true);
        credits.GetComponent<Animation>().Play();

        yield return new WaitForSeconds(3f);

        credits.transform.GetChild(0).GetComponent<Animation>().Play();

        yield return new WaitForSeconds(5f);

        StartCoroutine(levelControl.LoadLevel(true));
    }

    IEnumerator TypeAudio(DialogueSet[] sets, float speed, int set)
    {
        AudioManager.am.Play("Type", Random.Range(sets[set].pitchMin, sets[set].pitchMax));
        yield return new WaitForSeconds(speed);
        AudioManager.am.Stop("Type");
        if (isTyping) StartCoroutine(TypeAudio(sets, speed, set));
    }

    void EyeDirection()
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

    void MoveChar()
    {
        if (isMovingChar)
        {
            float targetDistance = Mathf.Abs(targetPosX - transform.position.x);

            if (targetDistance <= .5f)
            {
                velocityX = Mathf.MoveTowards(velocityX, 0, 70 * Time.deltaTime);

                if (targetDistance <= 0f || velocityX == 0f)
                {
                    velocityX = 0f;
                    transform.position = new Vector2(targetPosX, transform.position.y);
                    isMovingChar = false;
                }
            }
            else
            {
                int dir = moveDir > 0 ? 1 : -1;
                velocityX = Mathf.MoveTowards(velocityX, 9 * dir, 75 * Time.deltaTime);
            }
        }

        Vector2 velocity = new Vector2(velocityX, 0f);
        transform.Translate(velocity * Time.deltaTime);

        // Plays particles if moving, stops particles if not moving
        if (velocity.x > -.25f && velocity.x < .25f)
        {
            playParticle = true;
            npcParticle.Stop();
        }
        else if (playParticle)
        {
            playParticle = false;
            npcParticle.Play();
        }
    }

    void ColorPlayer()
    {
        if (isColoringPlayer)
        {
            SpriteRenderer bodySprite = player.GetColorBody();
            Color bodyColor = bodySprite.color;

            if (bodyColor.a < 1f)
            {
                bodyColor.a = Mathf.MoveTowards(bodyColor.a, 1, Time.deltaTime);
                bodySprite.color = bodyColor;
            }
            else
            {
                bodyColor.a = 1f;
                bodySprite.color = bodyColor;
                isColoringPlayer = false;
            }
        }
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
