using System.Collections;
using UnityEngine;

public class ShapeKeyAnimator : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer; // Referentie naar de SkinnedMeshRenderer
    public float animationDuration = 2.0f; // Duur van de animatie van 0 naar 1
    public float reverseAnimationDuration = 2.0f; // Duur van de animatie van 1 naar 0
    public float delayBeforeReverse = 1.0f; // Wachtijd na animatie van 0 naar 1
    public float delayAfterReverse = 1.0f; // Wachtijd na animatie van 1 naar 0
    public Transform spawnPosition; // Positie waar de prefab gespawnd wordt
    public Transform hidePosition; // Verberglocatie voor vorige prefabs
    public GameObject[] prefabs; // Array van prefabs
    public float swapInterval = 5.0f; // Hoe vaak (in seconden) de prefabs worden geswapt
    public bool triggerSwap = false; // Bool om direct een swap te triggeren

    private int currentPrefabIndex = 0;
    private GameObject[] prefabPool; // Pool van gepoolde prefabs
    private GameObject currentPrefab;
    private bool isSwappingEnabled = true;

    void Start()
    {
        if (skinnedMeshRenderer == null || prefabs == null || prefabs.Length == 0 || spawnPosition == null || hidePosition == null)
        {
            Debug.LogError("Niet alle vereiste variabelen zijn ingesteld in de Inspector!");
            return;
        }

        // Maak een pool van prefabs
        InitializePrefabPool();

        StartCoroutine(SwapAndAnimate());
    }

    void Update()
    {
        // Controleer of de trigger bool is geactiveerd
        if (triggerSwap)
        {
            triggerSwap = false; // Reset de bool
            StopAllCoroutines(); // Stop alle lopende coroutines
            StartCoroutine(SwapAndAnimate()); // Start een nieuwe swap en animatie
        }
    }

    IEnumerator SwapAndAnimate()
    {
        while (isSwappingEnabled)
        {
            // Wacht tot de volgende swap
            yield return new WaitForSeconds(swapInterval);

            // Voer de prefab-swap uit
            SwitchPrefab();

            // Start de blendshape-animatie (van 0 naar 1)
            yield return StartCoroutine(ChangeShapeKey(0, 1, animationDuration));

            // Wacht voor de terugkeer animatie
            yield return new WaitForSeconds(delayBeforeReverse);

            // Start de blendshape-animatie (van 1 naar 0)
            yield return StartCoroutine(ChangeShapeKey(1, 0, reverseAnimationDuration));

            // Wacht na de terugkeer animatie
            yield return new WaitForSeconds(delayAfterReverse);
        }
    }

    IEnumerator ChangeShapeKey(float startValue, float endValue, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float blendValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            skinnedMeshRenderer.SetBlendShapeWeight(0, blendValue * 100); // Pas de shapekey aan
            yield return null;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(0, endValue * 100); // Zorg ervoor dat de eindwaarde exact is
    }

    void InitializePrefabPool()
    {
        // Maak een pool van prefabs
        prefabPool = new GameObject[prefabs.Length];
        for (int i = 0; i < prefabs.Length; i++)
        {
            prefabPool[i] = Instantiate(prefabs[i], hidePosition.position, hidePosition.rotation); // Spawn en verberg direct
            prefabPool[i].SetActive(false); // Zet ze inactief
        }
    }

    void SwitchPrefab()
    {
        if (currentPrefab != null)
        {
            // Verplaats het huidige object naar de verberglocatie en zet het inactief
            currentPrefab.transform.position = hidePosition.position;
            currentPrefab.SetActive(false);
        }

        // Wissel naar de volgende prefab in de pool
        currentPrefabIndex = (currentPrefabIndex + 1) % prefabPool.Length;
        currentPrefab = prefabPool[currentPrefabIndex];

        // Verplaats de nieuwe prefab naar de spawnpositie en zet het actief
        currentPrefab.transform.position = spawnPosition.position;
        currentPrefab.SetActive(true);
    }
}