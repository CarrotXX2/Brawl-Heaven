using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class SettingsIngame : MonoBehaviour
{
    public Image fadeImage; // Verwijs naar je fade Image UI element
    public string nextSceneName; // Scene die geladen moet worden

    public void MainMenuLoad()
    {
        StartCoroutine(LoadSceneWithFade());
    }

    private IEnumerator LoadSceneWithFade()
    {
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeToBlack());
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeToBlack()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color endColor = Color.black;

        while (elapsedTime < fadeDuration)
        {
            fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = endColor;
    }
}