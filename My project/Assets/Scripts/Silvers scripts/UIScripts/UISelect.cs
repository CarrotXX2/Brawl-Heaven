using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Voeg deze namespace toe voor TextMeshPro

public class ButtonSelectionIndicator : MonoBehaviour
{
    public GameObject indicatorPrefab; // De afbeelding (prefab) die gespawnd wordt bij de geselecteerde knop
    public Vector2 offset = new Vector2(0, 0); // Offset om de afbeelding te positioneren ten opzichte van het geselecteerde punt
    public string targetTag = "Selected"; // De tag van het GameObject binnen de knop waar de indicator naartoe moet gaan

    public float animationSpeed = 2.0f; // Snelheid van de animatie
    public float scaleFactor = 1.2f;   // Hoeveel de button groter wordt

    public TMP_Text selectedObjectText; // Het TextMeshPro-veld om de naam van het geselecteerde object weer te geven

    private GameObject currentIndicator; // Huidige geïnstantieerde afbeelding
    private EventSystem eventSystem; // Unity's EventSystem om de geselecteerde knop te detecteren
    private Vector3 originalScale; // Originele schaal van de geselecteerde button
    private GameObject lastSelectedButton; // Laatst geselecteerde button

    void Start()
    {
        // Verkrijg de EventSystem in de scene
        eventSystem = EventSystem.current;

        // Spawn de indicator bij de start
        currentIndicator = Instantiate(indicatorPrefab, transform);

        // Verberg de indicator aanvankelijk
        currentIndicator.SetActive(false);

        // Update de indicator positie bij de start
        UpdateIndicator();
    }

    void Update()
    {
        // Update de indicator positie elke frame om de geselecteerde knop te volgen
        UpdateIndicator();

        // Animeer de geselecteerde button
        AnimateSelectedButton();

        // Update de tekst van het geselecteerde object
        UpdateSelectedObjectText();
    }

    void UpdateIndicator()
    {
        // Verkrijg de momenteel geselecteerde knop
        GameObject selectedButton = eventSystem.currentSelectedGameObject;

        // Als er geen knop is geselecteerd, verberg de indicator en stop
        if (selectedButton == null)
        {
            if (currentIndicator != null)
            {
                currentIndicator.SetActive(false);
            }
            return;
        }

        // Als de geselecteerde button verandert, reset de schaal van de vorige button
        if (lastSelectedButton != null && lastSelectedButton != selectedButton)
        {
            lastSelectedButton.transform.localScale = originalScale;
        }

        // Sla de geselecteerde button op
        lastSelectedButton = selectedButton;

        // Zoek het child GameObject met de opgegeven tag binnen de geselecteerde knop
        Transform target = selectedButton.transform.FindChildWithTag(targetTag);

        // Als er geen child met de opgegeven tag is, verberg de indicator en stop
        if (target == null)
        {
            if (currentIndicator != null)
            {
                currentIndicator.SetActive(false);
            }
            return;
        }

        // Als er een child met de opgegeven tag is, activeer de indicator (als deze uit staat)
        if (!currentIndicator.activeInHierarchy)
        {
            currentIndicator.SetActive(true);
        }

        // Positioneer de indicator bij het gevonden child GameObject
        RectTransform targetRect = target.GetComponent<RectTransform>();
        RectTransform indicatorRect = currentIndicator.GetComponent<RectTransform>();

        if (targetRect != null && indicatorRect != null)
        {
            // Stel de positie van de indicator in op de positie van het child GameObject + offset
            indicatorRect.position = targetRect.position + (Vector3)offset;
        }
    }

    void AnimateSelectedButton()
    {
        // Verkrijg de momenteel geselecteerde knop
        GameObject selectedButton = eventSystem.currentSelectedGameObject;

        // Als er geen knop is geselecteerd, stop
        if (selectedButton == null)
        {
            return;
        }

        // Als de originele schaal nog niet is opgeslagen, sla deze op
        if (originalScale == Vector3.zero)
        {
            originalScale = selectedButton.transform.localScale;
        }

        // Animeer de geselecteerde button
        float scale = Mathf.Sin(Time.time * animationSpeed) * 0.1f + 1.0f;
        selectedButton.transform.localScale = originalScale * scale * scaleFactor;
    }

    void UpdateSelectedObjectText()
    {
        // Verkrijg de momenteel geselecteerde knop
        GameObject selectedButton = eventSystem.currentSelectedGameObject;

        // Als er geen knop is geselecteerd, stop
        if (selectedButton == null)
        {
            return;
        }

        // Update de tekst van het TextMeshPro-veld met de naam van het geselecteerde object
        if (selectedObjectText != null)
        {
            selectedObjectText.text = selectedButton.name;
        }
    }
}

// Hulpmethode om een child GameObject met een specifieke tag te vinden
public static class TransformExtensions
{
    public static Transform FindChildWithTag(this Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }
}