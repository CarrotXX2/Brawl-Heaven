using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class CurtainController : MonoBehaviour
{
    [Header("BlendShape Instellingen")]
    public int blendShapeIndex = 0;

    [Header("Animatie Instellingen")]
    public float transitionSpeed = 25f; // Hoe snel het gordijn opent/sluit
    public float holdTimeOpen = 2f;     // Tijd dat het gordijn open blijft
    public float holdTimeClosed = 1f;   // Tijd dat het gordijn dicht blijft

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private float targetWeight = 0f;   // Doelwaarde (0% of 100%)
    private float currentWeight = 0f;  // Huidige waarde van de shape key
    private float velocity = 0f;       // Voor SmoothDamp
    private float holdTimer = 0f;
    private bool isHolding = false;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        currentWeight = 0f; // Start met gordijn dicht
        targetWeight = 100f; // Eerst openen
    }

    void Update()
    {
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            float holdDuration = (targetWeight == 0f) ? holdTimeClosed : holdTimeOpen;

            if (holdTimer >= holdDuration)
            {
                isHolding = false;
                holdTimer = 0f;
                targetWeight = (targetWeight == 100f) ? 0f : 100f; // Wissel tussen open en dicht
            }
        }
        else
        {
            // Soepele overgang met SmoothDamp
            currentWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref velocity, 1f / transitionSpeed);
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, currentWeight);

            if (Mathf.Abs(currentWeight - targetWeight) < 0.5f) // Zodra bijna bereikt, stop en houd vast
            {
                currentWeight = targetWeight;
                isHolding = true;
                velocity = 0f; // Reset snelheid voor volgende cyclus
            }
        }
    }
}
