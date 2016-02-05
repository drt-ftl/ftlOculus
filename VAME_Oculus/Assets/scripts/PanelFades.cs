using UnityEngine;
using System.Collections;

public class PanelFades : MonoBehaviour {


    private float factor = 0.1f;

    public void FadeOut()
    {
        StartCoroutine("fadeOut");
    }

    public void FadeIn()
    {
        StartCoroutine("fadeIn");
    }

    IEnumerator fadeOut()
    {
        var z = transform.localPosition.z;
        for (float f = z; f <= 2.4f; f += factor)
        {
            var pos = Vector3.zero;
            pos.y = Screen.height / 2;
            pos.z = f;
            transform.localPosition = pos;
            yield return null;
        }
    }

    IEnumerator fadeIn()
    {
        var z = transform.localPosition.z;
        for (float f = z; f >= 0f; f -= factor)
        {
            var pos = Vector3.zero;
            pos.y = Screen.height / 2;
            pos.z = f;
            transform.localPosition = pos;
            yield return null;
        }
    }
}
