using UnityEngine;
using TMPro;
using System.Collections;

public class EnhancedDamageDisplay : MonoBehaviour
{
    private int playerID;
    [Header("Color Settings")]
    public Color safeColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    public Color criticalColor = new Color(0.5f, 0f, 0f); // Dark red
    public float warningThreshold = 30f;
    public float dangerThreshold = 70f;
    public float criticalThreshold = 100f;

    [Header("Animation Settings")]
    public float countUpSpeed = 100f; // Damage units per second
    public float baseShakeDuration = 0.3f;
    public float baseShakeIntensity = 15f;

    [Header("Testing Settings")]
    public float damageAmount = 50f;
    public bool startCountUp = false;

    private TMP_Text damageText;
    private float currentDisplayDamage;
    private Vector3 originalTextPosition;
    private Coroutine countCoroutine;

    private void Awake()
    {
        damageText = GetComponent<TMP_Text>();
        originalTextPosition = damageText.transform.localPosition;
        currentDisplayDamage = 0;
        UpdateText();
    }

    public void SetPlayerIndex(int playerIndex)
    {
        playerID = playerIndex;
    }

    private void Update()
    {
        if (startCountUp)
        {
            startCountUp = false;
            UpdateDamage(damageAmount);
        }
        UpdateText();
    }


    public void UpdateDamage(float newDamage)
    {
        if (countCoroutine != null)
        {
            StopCoroutine(countCoroutine);
        }

        countCoroutine = StartCoroutine(CountToNewDamage(newDamage));
    }

    private IEnumerator CountToNewDamage(float targetDamage)
    {
        float shakeMultiplier = Mathf.Clamp01(targetDamage / criticalThreshold);
        StartCoroutine(ShakeText(baseShakeDuration * (1 + shakeMultiplier), baseShakeIntensity * (1 + shakeMultiplier)));

        float startDamage = currentDisplayDamage;
        float duration = Mathf.Abs(targetDamage - startDamage) / countUpSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentDisplayDamage = Mathf.Lerp(startDamage, targetDamage, elapsed / duration);
            UpdateText();
            yield return null;
        }

        currentDisplayDamage = targetDamage;
        UpdateText();
    }

    private void UpdateText()
    {
        damageText.text = $"%{Mathf.RoundToInt(currentDisplayDamage)}";

        if (currentDisplayDamage >= criticalThreshold)
        {
            damageText.color = criticalColor;
        }
        else if (currentDisplayDamage >= dangerThreshold)
        {
            float t = (currentDisplayDamage - dangerThreshold) / (criticalThreshold - dangerThreshold);
            damageText.color = Color.Lerp(dangerColor, criticalColor, t);
        }
        else if (currentDisplayDamage >= warningThreshold)
        {
            float t = (currentDisplayDamage - warningThreshold) / (dangerThreshold - warningThreshold);
            damageText.color = Color.Lerp(warningColor, dangerColor, t);
        }
        else
        {
            float t = currentDisplayDamage / warningThreshold;
            damageText.color = Color.Lerp(safeColor, warningColor, t);
        }
    }

    private IEnumerator ShakeText(float duration, float intensity)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            damageText.transform.localPosition = originalTextPosition + Random.insideUnitSphere * intensity;
            yield return null;
        }

        damageText.transform.localPosition = originalTextPosition;
    }
}
