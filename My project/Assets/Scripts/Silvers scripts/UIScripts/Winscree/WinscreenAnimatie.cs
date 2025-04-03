using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WinScreenAnimatie : MonoBehaviour
{
    [System.Serializable]
    public class PlayerImageData
    {
        public RectTransform imageTransform;
        public RectTransform placementLocation;
        public GameObject placementPrefabInstance;
        public Vector2 targetPosition;
        public Vector3 originalScale;
        public bool isWinner;
        public int playerIndex;
        public int placement;
    }

    [Header("Player Settings")]
    public List<RectTransform> playerImages = new List<RectTransform>();
    public float floatDuration = 2f;
    public float jumpHeight = 150f;
    public float jumpDuration = 0.8f;
    public float delayBetweenJumps = 0.4f;
    public float startYPosition = -300f;

    [Header("Final Positions (1st, 2nd, 3rd, 4th)")]
    public Vector2[] finalPositions = new Vector2[4];

    [Header("Placement Prefabs")]
    public GameObject firstPlacePrefab;
    public GameObject secondPlacePrefab;
    public GameObject thirdPlacePrefab;
    public GameObject fourthPlacePrefab;

    [Header("Placement Settings")]
    public string placementLocationName = "PlacementLocation";

    [Header("Winner Settings")]
    public float winnerCooldown = 1f;
    public float winnerScaleMultiplier = 1.4f;
    public AudioClip drumRollSound;
    public AudioClip victorySound;
    public float drumRollDuration = 1.5f;

    [Header("Scene Management")]
    public string nextSceneName = "MainMenu";
    public float sceneLoadDelay = 5f;

    [Header("Countdown Settings")]
    public TMP_Text countdownText;
    public float countdownDuration = 5f;
    public float countdownStartDelay = 0f;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Easing Settings")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve jumpCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.5f, 1),
        new Keyframe(1, 0)
    );
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debug Tool")]
    public int debugPlayerCount = 2;
    public bool useDebugMode = false;
    public bool activate = false;

    [Header("Player Placement Tracking")]
    // Dictionary to store the placement of each player when they die
    private Dictionary<int, int> playerPlacements = new Dictionary<int, int>();
    private int nextPlacement; // Start from 4th place (players who die first get worst placement)

    private List<PlayerImageData> playerData = new List<PlayerImageData>();
    private AudioSource audioSource;
    private Coroutine countdownCoroutine;

    // Singleton instance for easy access from other scripts
    private static WinScreenAnimatie _instance;
    public static WinScreenAnimatie Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (countdownText != null)
        {
            countdownText.text = "";
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }

        // Reset placements at start
        ResetPlacements();
    }

    void Start()
    {
        if (!useDebugMode)
        {
            InitializeWithRealPlayers();
        }

        nextPlacement = GameplayManager.Instance.players.Count;
    }

    // Call this when a player dies to assign them a placement
    public void AssignPlayerPlacement(int playerIndex)
    {
        // If player already has a placement, don't reassign
        if (playerPlacements.ContainsKey(playerIndex))
            return;

        // Assign current placement to this player
        playerPlacements[playerIndex] = nextPlacement;
        
        // Move to next placement (going backward: 4th, 3rd, 2nd...)
        nextPlacement--;
        
        // First place (winner) is assigned when only one player remains
        if (nextPlacement < 1)
            nextPlacement = 1;
            
        // Debug information
        Debug.Log($"Player {playerIndex} placed at position {playerPlacements[playerIndex]}");
    }

    // Call this to assign 1st place to the last player standing
    public void AssignWinner(int playerIndex)
    {
        playerPlacements[playerIndex] = 1; // 1st place
        Debug.Log($"Player {playerIndex} is the winner!");
        
        InitializeWithRealPlayers();
    }

    // Get a player's placement (returns 0 if not assigned yet)
    public int GetPlayerPlacement(int playerIndex)
    {
        if (playerPlacements.ContainsKey(playerIndex))
            return playerPlacements[playerIndex];
        return 0;
    }

    // Reset all placements (call at game start)
    public void ResetPlacements()
    {
        playerPlacements.Clear();
        nextPlacement = 4; // Adjust based on max players
    }

    void InitializeWithRealPlayers()
    {
        int playersAlive = GameplayManager.Instance.playersAlive.Count;
        
        // Apply the correct placements from our tracking
        SetupPlayerPositionsWithPlacements(playersAlive);
        
        StartCoroutine(FloatThenJump());
    }

    void Update()
    {
        if (useDebugMode && activate)
        {
            activate = false;
            SetupPlayerPositions(debugPlayerCount);
            StartCoroutine(FloatThenJump());
        }
    }

    // New method that uses the placements dictionary
    void SetupPlayerPositionsWithPlacements(int playerCount)
    {
        foreach (var data in playerData)
        {
            if (data.placementPrefabInstance != null)
            {
                Destroy(data.placementPrefabInstance);
            }
        }
        playerData.Clear();

        // Create a list to sort players by placement
        List<KeyValuePair<int, int>> sortedPlayers = new List<KeyValuePair<int, int>>();
        
        // Get all player placements
        foreach (var placement in playerPlacements)
        {
            sortedPlayers.Add(placement);
        }
        
        // Sort by placement (1st, 2nd, 3rd, 4th)
        sortedPlayers.Sort((a, b) => a.Value.CompareTo(b.Value));
        
        // Set up each player based on their placement
        for (int i = 0; i < sortedPlayers.Count && i < playerImages.Count; i++)
        {
            int playerIndex = sortedPlayers[i].Key;
            int placement = sortedPlayers[i].Value;
            
            PlayerImageData data = new PlayerImageData();
            data.imageTransform = playerImages[i];
            data.playerIndex = playerIndex;
            data.placement = placement;
            data.isWinner = (placement == 1);

            // Map placement to position in final positions array
            int positionIndex = placement - 1;
            if (positionIndex >= 0 && positionIndex < finalPositions.Length)
            {
                data.targetPosition = finalPositions[positionIndex];
            }

            data.placementLocation = FindPlacementLocation(data.imageTransform);
            data.originalScale = playerImages[i].localScale;
            playerImages[i].gameObject.SetActive(true);

            if (data.placementLocation != null)
            {
                GameObject prefabToInstantiate = GetPlacementPrefab(data.placement);
                if (prefabToInstantiate != null)
                {
                    data.placementPrefabInstance = Instantiate(
                        prefabToInstantiate,
                        data.placementLocation.position,
                        Quaternion.identity,
                        data.placementLocation
                    );

                    RectTransform rt = data.placementPrefabInstance.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = Vector2.zero;
                        rt.localScale = Vector3.one;
                        rt.localRotation = Quaternion.identity;
                    }
                }
            }

            playerData.Add(data);
            playerImages[i].anchoredPosition = new Vector2(
                data.targetPosition.x,
                startYPosition
            );
        }
    }

    void SetupPlayerPositions(int playerCount)
    {
        foreach (var data in playerData)
        {
            if (data.placementPrefabInstance != null)
            {
                Destroy(data.placementPrefabInstance);
            }
        }
        playerData.Clear();

        for (int i = 0; i < playerImages.Count && i < playerCount; i++)
        {
            PlayerImageData data = new PlayerImageData();
            data.imageTransform = playerImages[i];
            data.playerIndex = i;
            data.placement = i + 1;
            data.isWinner = (i == 0);

            if (finalPositions.Length > i)
            {
                data.targetPosition = finalPositions[i];
            }

            data.placementLocation = FindPlacementLocation(data.imageTransform);
            data.originalScale = playerImages[i].localScale;
            playerImages[i].gameObject.SetActive(true);

            if (data.placementLocation != null)
            {
                GameObject prefabToInstantiate = GetPlacementPrefab(data.placement);
                if (prefabToInstantiate != null)
                {
                    data.placementPrefabInstance = Instantiate(
                        prefabToInstantiate,
                        data.placementLocation.position,
                        Quaternion.identity,
                        data.placementLocation
                    );

                    RectTransform rt = data.placementPrefabInstance.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = Vector2.zero;
                        rt.localScale = Vector3.one;
                        rt.localRotation = Quaternion.identity;
                    }
                }
            }

            playerData.Add(data);
            playerImages[i].anchoredPosition = new Vector2(
                data.targetPosition.x,
                startYPosition
            );
        }
    }

    IEnumerator FloatThenJump()
    {
        yield return new WaitForSeconds(floatDuration);

        // Animate non-winners
        for (int i = playerData.Count - 1; i >= 1; i--)
        {
            yield return StartCoroutine(SmoothJumpAnimation(playerData[i], false));
            yield return new WaitForSeconds(delayBetweenJumps);
        }

        // Winner sequence
        yield return new WaitForSeconds(winnerCooldown);

        if (drumRollSound != null)
        {
            audioSource.PlayOneShot(drumRollSound);
        }

        yield return new WaitForSeconds(drumRollDuration);

        yield return StartCoroutine(SmoothJumpAnimation(playerData[0], true));

        if (victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        // Wacht eventuele delay voordat countdown start
        yield return new WaitForSeconds(countdownStartDelay);

        // Start countdown
        if (countdownText != null)
        {
            countdownCoroutine = StartCoroutine(ShowCountdown());
        }
        else
        {
            yield return new WaitForSeconds(sceneLoadDelay);

            if (fadeImage != null)
            {
                yield return StartCoroutine(FadeToBlack());
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator ShowCountdown()
    {
        float remainingTime = countdownDuration;

        while (remainingTime > 0)
        {
            countdownText.text = $"Going back to main menu in {Mathf.CeilToInt(remainingTime)}";
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        countdownText.text = "Loading...";


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

    IEnumerator FadeToBlack()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 0f;
        Color endColor = startColor;
        endColor.a = 1f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        fadeImage.color = endColor;
        yield return new WaitForSeconds(0.2f);
    }

    RectTransform FindPlacementLocation(RectTransform parent)
    {
        foreach (RectTransform child in parent)
        {
            if (child.name == placementLocationName)
            {
                return child;
            }
        }
        return null;
    }

    GameObject GetPlacementPrefab(int placement)
    {
        switch (placement)
        {
            case 1: return firstPlacePrefab;
            case 2: return secondPlacePrefab;
            case 3: return thirdPlacePrefab;
            case 4: return fourthPlacePrefab;
            default: return null;
        }
    }

    IEnumerator SmoothJumpAnimation(PlayerImageData data, bool isWinnerJump)
    {
        RectTransform image = data.imageTransform;
        Vector2 startPos = image.anchoredPosition;
        Vector2 targetPos = data.targetPosition;

        float elapsedTime = 0f;
        float currentJumpHeight = isWinnerJump ? jumpHeight * 2f : jumpHeight;
        float currentDuration = isWinnerJump ? jumpDuration * 1.5f : jumpDuration;

        if (isWinnerJump)
        {
            StartCoroutine(ScaleAnimation(image, data.originalScale, data.originalScale * winnerScaleMultiplier, currentDuration));
        }

        while (elapsedTime < currentDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / currentDuration);

            float easedProgress = movementCurve.Evaluate(progress);
            float currentY = Mathf.Lerp(startPos.y, targetPos.y, easedProgress);
            float jumpValue = jumpCurve.Evaluate(progress) * currentJumpHeight;

            image.anchoredPosition = new Vector2(
                targetPos.x,
                currentY + jumpValue
            );

            yield return null;
        }

        image.anchoredPosition = targetPos;
    }

    IEnumerator ScaleAnimation(RectTransform target, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float easedProgress = scaleCurve.Evaluate(progress);

            target.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            yield return null;
        }

        target.localScale = endScale;
    }
}