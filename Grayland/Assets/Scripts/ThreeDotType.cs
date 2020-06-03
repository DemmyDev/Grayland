using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreeDotType : MonoBehaviour
{
    Text text;

    void Start()
    {
        text = GetComponent<Text>();
        StartCoroutine(Type());
    }

    public IEnumerator Type()
    {
        while (true)
        {
            text.text = ".";
            yield return new WaitForSeconds(.33f);
            text.text = "..";
            yield return new WaitForSeconds(.33f);
            text.text = "...";
            yield return new WaitForSeconds(.33f);
        }    
    }
}
