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

    [Header("Lives Settings")]
    public int maxLives = 3;
    public int currentLives = 3;
    public TMP_Text livesText;
    public string livesPrefix = "Lives: ";
    public string livesSymbol = "♥";
    public Color livesColor = new Color(1f, 0.2f, 0.2f); // Bright red for hearts
    public bool showNumeric = false;  // If true, shows "Lives: 3" format
    public bool showSymbols = false; // If true, shows "Lives: ♥♥♥" format

    [Header("Testing Settings")]
    public float damageAmount = 50f;
    public bool startCountUp = false;
    public bool decrementLife = false;

    private TMP_Text damageText;
    private float currentDisplayDamage;
    private Vector3 originalTextPosition;
    private Coroutine countCoroutine;

    private void Start()
    {
        damageText = GetComponent<TMP_Text>();
        originalTextPosition = damageText.transform.localPosition;
        currentDisplayDamage = 0;
        UpdateText();
        
        // Initialize lives display if assigned
        if (livesText != null)
        {
            UpdateLivesDisplay();
        }
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
        
        if (decrementLife)
        {
            decrementLife = false;
            DecrementLife();
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
    
    // Lives management functions
    public void DecrementLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            UpdateLivesDisplay();
            
            // Optional - shake the lives display when losing a life
            if (livesText != null)
            {
                StartCoroutine(ShakeLivesText(baseShakeDuration, baseShakeIntensity * 0.5f));
            }
        }
    }
    
    public void AddLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            UpdateLivesDisplay();
        }
    }
    
    public void ResetLives()
    {
        currentLives = maxLives;
        UpdateLivesDisplay();
    }
    
    private void UpdateLivesDisplay()
    {
        if (livesText == null) return;
        
        livesText.color = livesColor;
        
        if (showSymbols)
        {
            string hearts = "";
            for (int i = 0; i < currentLives; i++)
            {
                hearts += livesSymbol;
            }
            livesText.text = livesPrefix + hearts;
        }
        else if (showNumeric)
        {
            livesText.text = livesPrefix + currentLives.ToString();
        }
        else
        {
            // Default case - show both
            livesText.text = livesPrefix + currentLives.ToString() + " " + livesSymbol;
        }
    }
    
    private IEnumerator ShakeLivesText(float duration, float intensity)
    {
        Vector3 originalLivesPosition = livesText.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            livesText.transform.localPosition = originalLivesPosition + Random.insideUnitSphere * intensity;
            yield return null;
        }

        livesText.transform.localPosition = originalLivesPosition;
    }
}