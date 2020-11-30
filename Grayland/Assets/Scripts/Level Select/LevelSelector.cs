using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] List<LevelSelectID> levelSelectIDs;
    [SerializeField] LevelSet levelSet;
    [SerializeField] PlayerController player;
    [SerializeField] GameObject transition;

    Animation anim;

    private void Awake()
    {
        anim = transition.GetComponent<Animation>();
        anim.Play("TransitionExit");
    }

    IEnumerator Transition(int sceneNum, bool isSceneSelect)
    {
        anim.Play("TransitionEnter");
        yield return new WaitForSeconds(.5f);
        if (isSceneSelect) SceneManager.LoadScene(sceneNum);
        else SceneManager.LoadScene(1);
    }

    public void LevelTransition(int selectNum, bool isSceneSelect, bool left, bool right, bool up, bool down)
    {
        if (!isSceneSelect) levelSet.levelToLoad = selectNum;
        // Lock player movement
        player.ForceSetMoveInput(left, right, up, down);
        // Start level transition
        StartCoroutine(Transition(selectNum, isSceneSelect));
    }
}
