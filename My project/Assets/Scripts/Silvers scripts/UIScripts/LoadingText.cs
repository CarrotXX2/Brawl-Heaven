using System.Collections;
using UnityEngine;
using TMPro;

public class LoadingText : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float animationSpeed = 0.5f; // Snelheid van de animatie in seconden

    private void Start()
    {
        StartCoroutine(AnimateLoadingText());
    }

    private IEnumerator AnimateLoadingText()
    {
        while (true)
        {
            loadingText.text = "loading.";
            yield return new WaitForSeconds(animationSpeed);

            loadingText.text = "loading..";
            yield return new WaitForSeconds(animationSpeed);

            loadingText.text = "loading...";
            yield return new WaitForSeconds(animationSpeed);
        }
    }
}