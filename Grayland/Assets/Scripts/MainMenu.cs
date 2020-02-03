using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    bool transitioning = false;
    Animation anim;
    
    void Awake()
    {
        anim = GetComponent<Animation>();
        anim.Play("TransitionExit");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !transitioning)
            StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        transitioning = true;
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene(1);
    }
}
