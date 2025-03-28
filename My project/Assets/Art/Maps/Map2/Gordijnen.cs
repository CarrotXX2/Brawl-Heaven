using System.Collections;
using UnityEngine;

public class ShapeKeyAnimator : MonoBehaviour
{
    [Header("Mesh Settings")]
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public int shapeKeyIndex = 0;

    [Header("Animation Settings")]
    public AnimationCurve zeroToHundredCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve hundredToZeroCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float zeroToHundredTime = 2.0f;
    public float hundredToZeroTime = 2.0f;
    public float holdTimeAtMax = 0.5f;
    public float holdTimeAtMin = 1.0f;
    public float delayBetweenCycles = 3.0f;

    [Header("Prefab Cycling")]
    public Transform spawnPosition;
    public Transform hidePosition;
    public GameObject[] scenePrefabs;
    public bool manualTrigger = false;
    public bool autoCyclePrefabs = true;

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
            if (autoCyclePrefabs)
            {
                CyclePrefab();
            }

            // ShapeKey: 0 → 100 with custom curve
            yield return StartCoroutine(AnimateShapeKey(0, 100, zeroToHundredTime, zeroToHundredCurve));

            // Hold at max value
            yield return new WaitForSeconds(holdTimeAtMax);

            // ShapeKey: 100 → 0 with custom curve
            yield return StartCoroutine(AnimateShapeKey(100, 0, hundredToZeroTime, hundredToZeroCurve));

            // Hold at min value
            yield return new WaitForSeconds(holdTimeAtMin);

            // Delay between full cycles
            yield return new WaitForSeconds(delayBetweenCycles);
        }
    }

    IEnumerator AnimateShapeKey(float start, float end, float duration, AnimationCurve curve)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = curve.Evaluate(t);
            float value = Mathf.Lerp(start, end, curveValue);
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

    // Public methods for external control
    public void StartAnimationSequence()
    {
        StopAllCoroutines();
        StartCoroutine(MainAnimationSequence());
    }

    public void StopAnimation()
    {
        StopAllCoroutines();
    }

    public void SetShapeKeyValue(float value)
    {
        skinnedMeshRenderer.SetBlendShapeWeight(shapeKeyIndex, Mathf.Clamp(value, 0, 100));
    }
}