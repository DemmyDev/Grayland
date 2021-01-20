using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] bool isDemo = false;
    [SerializeField] Camera cam;
    [SerializeField] GameObject transition;
    [SerializeField] GameObject canvas;
    [SerializeField] PlayerController player;
    [SerializeField] LayerMask whatIsPlayer;
    
    bool transitioning = false, checkForPlayer = true, animCam = false, isOpening = true;
    float camEndSize, animTime = 0f;
    CanvasMenuAnim animCanvas;
    Vector3 camEndPos;
    Transform eyes;
    Animation anim;
    
    void Awake()
    {
        isOpening = true;
        animCanvas = canvas.GetComponent<CanvasMenuAnim>();
        player.SetMove(false);
        player.SetBlinking(false, true);

        camEndPos = new Vector3(0, 0, -10);
        camEndSize = 6.5f;

        eyes = player.GetEyes();
        StartCoroutine(OpenGame());

        anim = transition.GetComponent<Animation>();
        anim.Play("TransitionExit");
    }

    private void Update()
    {
        if ((Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump")) && isOpening)
        {
            isOpening = false;

            player.SetBlinking(true, false);
            player.SetMove(true);
            StopCoroutine(player.SetUpBlink());
            StartCoroutine(player.SetUpBlink());

            animCam = false;
            cam.transform.position = camEndPos;
            cam.orthographicSize = camEndSize;
            animCanvas.StopTextAnim();
        }
    }

    private void FixedUpdate()
    {
        RaycastHit2D playerCheck = Physics2D.BoxCast(transform.position, new Vector2(3f, 4f), 0f, Vector2.zero, 0f, whatIsPlayer);

        if (playerCheck && checkForPlayer)
        {
            checkForPlayer = false;
            StartCoroutine(Transition());
        }

        if (animCam)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, camEndPos, .125f);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, camEndSize, .125f);
            animTime += Time.deltaTime;

            if (animTime >= .995f)
            {
                cam.transform.position = camEndPos;
                cam.orthographicSize = camEndSize;
                animCam = false;
            }
        }
    }

    IEnumerator OpenGame()
    {
        yield return new WaitForSeconds(2f);

        if (isOpening) player.SetBlinking(true, false);

        yield return new WaitForSeconds(1.5f);

        if (isOpening)
        {
            animCam = true;
            StartCoroutine(player.SetUpBlink());
        }

        yield return new WaitForSeconds(1f);

        if (isOpening)
        {
            isOpening = false;
            animCam = false;
            animCanvas.StartTextAnim();
            player.SetMove(true);
        }
    }

    IEnumerator Transition()
    {
        player.ForceSetMoveInput(false, true, false, false);
        transitioning = true;
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);

        if (isDemo) SceneManager.LoadScene(1);
        else SceneManager.LoadScene(2);
    }
}
