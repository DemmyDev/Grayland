using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasMenuAnim : MonoBehaviour
{
    [SerializeField] GameObject titleText;
    [SerializeField] GameObject creditText;

    public void StartTextAnim()
    {
        titleText.GetComponent<Animation>().Play("TextFadeIn");
        creditText.GetComponent<Animation>().Play("TextFadeIn");
    }

    public void StopTextAnim()
    {
        titleText.GetComponent<Animation>().Stop();
        creditText.GetComponent<Animation>().Stop();

        titleText.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
        creditText.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
    }
}
