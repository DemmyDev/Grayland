using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThreeDotType : MonoBehaviour
{
    TextMeshPro TMP;

    void Start()
    {
        TMP = GetComponent<TextMeshPro>();
        StartCoroutine(Type());
    }

    public IEnumerator Type()
    {
        while (true)
        {
            TMP.text = ".";
            yield return new WaitForSeconds(.33f);
            TMP.text = "..";
            yield return new WaitForSeconds(.33f);
            TMP.text = "...";
            yield return new WaitForSeconds(.33f);
        }    
    }
}
