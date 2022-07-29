using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] LayerMask whatIsPlayer;
    [SerializeField] GameObject transition;

    Animation anim;
    bool checkForPlayer = true;

    void Awake()
    {
        anim = transition.GetComponent<Animation>();
        anim.Play("TransitionExit");
    }

    void FixedUpdate()
    {
        RaycastHit2D playerCheck = Physics2D.BoxCast(transform.position, new Vector2(3f, 4f), 0f, Vector2.zero, 0f, whatIsPlayer);

        if (playerCheck && checkForPlayer)
        {
            checkForPlayer = false;
            StartCoroutine(Transition());
        }
    }

    IEnumerator Transition()
    {
        player.ForceSetMoveInput(false, false, true, false);
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);

        Application.Quit();
    }
}
