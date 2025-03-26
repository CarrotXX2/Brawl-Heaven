using System.Collections;
using UnityEngine;

public class ShapeKeyAnimator : MonoBehaviour
{
    [Header("Mesh Settings")]
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("ShapeKey1 Animation (Main)")]
    public float shapeKey1_ZeroToHundredTime = 2.0f;
    public float shapeKey1_HundredToZeroTime = 2.0f;
    public float delayAfterShapeKey1ReachesZero = 1.0f;

    [Header("ShapeKey2 Animation (Secondary)")]
    public float shapeKey2_ZeroToHundredTime = 1.5f;
    public float shapeKey2_HundredToZeroTime = 1.5f;
    public float delayBetweenShapeKey2Animations = 0.5f;

    [Header("Prefab Cycling")]
    public Transform spawnPosition;
    public Transform hidePosition;
    public GameObject[] scenePrefabs;
    public float fullCycleCooldown = 5.0f;
    public bool manualTrigger = false;

    private int currentPrefabIndex = 0;
    private GameObject currentPrefab;
    private bool isSystemActive = true;

    void Start()
    {
        if (!ValidateReferences()) return;
        InitializePrefabs();
        StartCoroutine(MainAnimationSequence());
    }

    void Update()
    {
        if (manualTrigger)
        {
            manualTrigger = false;
            StopAllCoroutines();
            StartCoroutine(MainAnimationSequence());
        }
    }

    bool ValidateReferences()
    {
        if (skinnedMeshRenderer == null || scenePrefabs.Length == 0)
        {
            Debug.LogError("Essential references missing!");
            return false;
        }
        return true;
    }

    IEnumerator MainAnimationSequence()
    {
        while (isSystemActive)
        {
            yield return new WaitForSeconds(fullCycleCooldown);
            CyclePrefab();

            // ShapeKey1: 0 → 100
            yield return StartCoroutine(AnimateShapeKey(0, 100, shapeKey1_ZeroToHundredTime, 0));

            // ShapeKey1: 100 → 0
            yield return StartCoroutine(AnimateShapeKey(100, 0, shapeKey1_HundredToZeroTime, 0));

            // Wacht na ShapeKey1 terug naar 0
            yield return new WaitForSeconds(delayAfterShapeKey1ReachesZero);

            // ShapeKey2: 0 → 100 → 0 (alleen wanneer ShapeKey1 op 0 staat)
            yield return StartCoroutine(AnimateShapeKey(0, 100, shapeKey2_ZeroToHundredTime, 1));
            yield return new WaitForSeconds(delayBetweenShapeKey2Animations);
            yield return StartCoroutine(AnimateShapeKey(100, 0, shapeKey2_HundredToZeroTime, 1));
        }
    }

    IEnumerator AnimateShapeKey(float start, float end, float duration, int shapeKeyIndex)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(start, end, elapsed / duration);
            skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyIndex, value);
            yield return null;
        }
        skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyIndex, end);
    }

    void InitializePrefabs()
    {
        foreach (var prefab in scenePrefabs)
            prefab.transform.position = hidePosition.position;

        currentPrefab = scenePrefabs[0];
        currentPrefab.transform.position = spawnPosition.position;
    }

    void CyclePrefab()
    {
        currentPrefab.transform.position = hidePosition.position;
        currentPrefabIndex = (currentPrefabIndex + 1) % scenePrefabs.Length;
        currentPrefab = scenePrefabs[currentPrefabIndex];
        currentPrefab.transform.position = spawnPosition.position;
    }
}