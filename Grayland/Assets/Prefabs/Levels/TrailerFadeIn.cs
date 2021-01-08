using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerFadeIn : MonoBehaviour
{
    [SerializeField] KeyCode key;
    Animation anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            Debug.Log("Play animation");
            anim.Play("FadeIn");
        }
    }
}
