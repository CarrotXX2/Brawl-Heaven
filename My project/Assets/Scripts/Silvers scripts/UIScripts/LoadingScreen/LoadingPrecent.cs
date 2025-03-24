using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingBar : MonoBehaviour
{
    [Header("Loading Bar Settings")]
    public Image loadingBarImage; 
    public TextMeshProUGUI loadingText;
    public float maxWidth = 1586f; 

    [Header("Loading Control")]
    public bool startLoading = false; 
    public float baseFillSpeed = 0.5f; 
    public float speedVariation = 0.3f; 
    public float variationFrequency = 2f;

    [Header("Scene Transition Settings")]
    public Image fadeImage; 
    public float fadeDuration = 1f; 
    public GameObject stageGameObject;

    private float _currentProgress = 0f; 
    private float _timeElapsed = 0f; 
    private bool _isLoadingComplete = false;

    private void Update()
    {
        if (startLoading && !_isLoadingComplete)
        {
            // Bereken een dynamische snelheid met een sinus-achtige variatie
            float speedVariationFactor = Mathf.Sin(_timeElapsed * variationFrequency * Mathf.PI * 2f);
            float currentFillSpeed = baseFillSpeed + speedVariation * speedVariationFactor;

            // Verhoog de voortgang op basis van de dynamische snelheid
            _currentProgress += currentFillSpeed * Time.deltaTime;
            _currentProgress = Mathf.Clamp01(_currentProgress); // Zorg ervoor dat de waarde tussen 0 en 1 blijft

            // Update de breedte van de loading bar
            float width = Mathf.Lerp(0, maxWidth, _currentProgress);
            loadingBarImage.rectTransform.sizeDelta = new Vector2(width, loadingBarImage.rectTransform.sizeDelta.y);

            // Update het percentage in de tekst
            int percentage = Mathf.RoundToInt(_currentProgress * 100);
            loadingText.text = $"{percentage}%";

            // Update de verstreken tijd
            _timeElapsed += Time.deltaTime;

            if (width >= maxWidth && !_isLoadingComplete)
            {
                _isLoadingComplete = true;
                StartCoroutine(FadeAndSwitchScene());
            }
        }
    }

    private IEnumerator FadeAndSwitchScene()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        // Fade-in animatie
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return null;
        }

        if (stageGameObject != null)
        {
            string sceneName = stageGameObject.name;

            foreach (var player in PlayerManager.Instance.players)
            {
                player.OnGameStart();
            }
            SceneManager.LoadScene(sceneName);
        }
    }
    public void ResetLoadingBar()
    {
        _currentProgress = 0f;
        _timeElapsed = 0f;
        _isLoadingComplete = false;
        loadingBarImage.rectTransform.sizeDelta = new Vector2(0, loadingBarImage.rectTransform.sizeDelta.y);
        loadingText.text = "0%";
    }

    public void StartLoading()
    {
        startLoading = true;
    }
}