using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLeave : MonoBehaviour
{
    public RectTransform uiElement; // Het UI-element dat moet bewegen
    public float targetYPosition = 200f; // De doel-Y-positie
    public float animationDuration = 1.0f; // Hoe lang de animatie duurt

    private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Easy-in curve
    private Vector2 originalPosition; // De originele positie van het UI-element
    private Vector2 targetPosition; // De doelpositie van het UI-element
    private float startTime;
    private bool isAnimating = false;
    private bool isAtTargetPosition = false; // Geeft aan of het UI-element bij de doelpositie is

    void Start()
    {
        // Sla de originele positie van het UI-element op
        if (uiElement != null)
        {
            originalPosition = uiElement.anchoredPosition;
            targetPosition = new Vector2(uiElement.anchoredPosition.x, targetYPosition);
        }
    }

    void Update()
    {
        if (isAnimating)
        {
            // Bereken de progress van de animatie
            float t = (Time.time - startTime) / animationDuration;

            if (t < 1.0f)
            {
                // Pas de curve toe voor een easy-in effect
                float curveValue = movementCurve.Evaluate(t);

                // Interpoleer soepel tussen de huidige positie en de doelpositie
                if (isAtTargetPosition)
                {
                    uiElement.anchoredPosition = Vector2.Lerp(targetPosition, originalPosition, curveValue);
                }
                else
                {
                    uiElement.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, curveValue);
                }
            }
            else
            {
                // Zorg ervoor dat het UI-element precies op de doelpositie staat
                if (isAtTargetPosition)
                {
                    uiElement.anchoredPosition = originalPosition;
                }
                else
                {
                    uiElement.anchoredPosition = targetPosition;
                }

                isAnimating = false; // Stop de animatie
                isAtTargetPosition = !isAtTargetPosition; // Wissel de staat
            }
        }
    }

    // Deze methode kan worden aangeroepen door een UI-knop
    public void TogglePosition()
    {
        if (!isAnimating) // Voorkom dat de animatie opnieuw start terwijl deze al bezig is
        {
            startTime = Time.time;
            isAnimating = true;
        }
    }
}