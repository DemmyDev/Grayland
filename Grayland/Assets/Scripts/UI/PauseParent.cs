using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseParent : MonoBehaviour
{
    [SerializeField] List<GameObject> selectables;

    Animator animator;

    int selectNum = 0, prevNum, minNum, maxNum;
    bool isPaused = false;
    bool canInput = true;
    bool isSelecting = false;
    TextMeshProUGUI prevText, selectText;

    void Awake()
    {
        animator = GetComponent<Animator>();

        minNum = 0;
        maxNum = 1;

        selectables[0].GetComponent<TextMeshProUGUI>().alpha = 1f;
        selectables[1].GetComponent<TextMeshProUGUI>().alpha = .1f;
    }

    void Update()
    {
        // Player pauses game through input
        if (!isPaused && Input.GetButtonDown("Pause") && canInput)
        {
            StartCoroutine(StartPause());
        }
        // Player unpauses game through input
        else if (isPaused && Input.GetButtonDown("Pause") && canInput)
        {
            StartCoroutine(StopPause());
        }
        
        // Player selects option
        if (isPaused && (Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump")) && canInput)
        {
            switch (selectNum)
            {
                // Return
                case 0:
                    StartCoroutine(StopPause());
                    break;
                // Quit
                case 1:
                    animator.ResetTrigger("Pause");
                    animator.SetTrigger("Unpause");
                    isPaused = false;
                    canInput = false;
                    Time.timeScale = 1f;
                    StartCoroutine(LevelController.levelController.SceneTransition(0));
                    break;
                default:
                    Debug.LogError("No option available. Idk why. This should be impossible");
                    break;
            }
        }

        // Game is currently paused
        if (isPaused && canInput)
        {
            // Input is down
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                prevNum = selectNum;
                selectNum++;
                if (selectNum > maxNum) selectNum = 0;
                StartCoroutine(ChangeSelection());
            }
            // Input is up
            else if (Input.GetAxisRaw("Vertical") > 0)
            {
                prevNum = selectNum;
                selectNum--;
                if (selectNum < minNum) selectNum = selectables.Count - 1;
                StartCoroutine(ChangeSelection());
            }
        }

        /*
        if (isSelecting)
        {
            prevText = selectables[prevNum].GetComponent<TextMeshProUGUI>();
            selectText = selectables[selectNum].GetComponent<TextMeshProUGUI>();

            prevText.CrossFadeAlpha(.1f, .075f, true);
            selectText.CrossFadeAlpha(1, .1f, true);
            
        }
        */
    }

    public IEnumerator StartPause()
    {
        Time.timeScale = 0f;
        isPaused = true;
        canInput = false;
        animator.ResetTrigger("Unpause");
        animator.SetTrigger("Pause");

        yield return new WaitForSecondsRealtime(.33f);

        canInput = true;
        
    }

    public IEnumerator StopPause()
    {
        animator.ResetTrigger("Pause");
        animator.SetTrigger("Unpause");
        isPaused = false;
        canInput = false;

        yield return new WaitForSecondsRealtime(.33f);

        Time.timeScale = 1f;
        canInput = true;
    }

    public IEnumerator ChangeSelection()
    {
        /*
        // Should work but animator and TMP are not cooperating with each other
        selectables[prevNum].GetComponent<Animator>().ResetTrigger("Select");
        selectables[prevNum].GetComponent<Animator>().SetTrigger("Unselect");
        selectables[selectNum].GetComponent<Animator>().ResetTrigger("Unselect");
        selectables[selectNum].GetComponent<Animator>().SetTrigger("Select");
        */

        isSelecting = true;
        canInput = false;

        prevText = selectables[prevNum].GetComponent<TextMeshProUGUI>();
        selectText = selectables[selectNum].GetComponent<TextMeshProUGUI>();

        prevText.alpha = .1f;
        selectText.alpha = 1f;

        yield return new WaitForSecondsRealtime(.15f);

        isSelecting = false;
        canInput = true;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }
}
