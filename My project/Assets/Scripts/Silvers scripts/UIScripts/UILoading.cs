using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoading : MonoBehaviour
{
    public RectTransform rectTransform; // Referentie naar de RectTransform die verplaatst moet worden
    public float startX = 0f; // Beginpositie op de X-as
    public float endX = 100f; // Eindpositie op de X-as
    public float moveDuration = 2f; // Tijd in seconden om van startX naar endX te bewegen
    public float resetDelay = 3f; // Tijd in seconden voordat de positie gereset wordt

    private Vector2 originalPosition; // Oorspronkelijke positie van de RectTransform
    private float elapsedTime = 0f;
    private bool isMoving = false;

    void Start()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        originalPosition = rectTransform.anchoredPosition; // Sla de oorspronkelijke positie op
    }

    void Update()
    {
        if (isMoving)
        {
            elapsedTime += Time.deltaTime;

            // Bereken de nieuwe X positie
            float newX = Mathf.Lerp(startX, endX, elapsedTime / moveDuration);
            rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);

            // Als de beweging voltooid is
            if (elapsedTime >= moveDuration)
            {
                isMoving = false;
                elapsedTime = 0f;

                // Start de reset timer
                Invoke("ResetPosition", resetDelay);
            }
        }
    }

    // Start de beweging
    public void StartMove()
    {
        if (!isMoving)
        {
            isMoving = true;
            elapsedTime = 0f;
        }
    }

    // Reset de positie naar de oorspronkelijke positie
    private void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }
}