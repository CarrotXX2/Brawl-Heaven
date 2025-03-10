using UnityEngine;

public class ShowLights : MonoBehaviour
{
    private Transform target; // De speler die gevolgd moet worden
    private Vector3 lastTargetPosition; // Laatste positie van de speler
    private bool isTargetBelowSpotlight = true; // Is de speler onder de spotlight?

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        lastTargetPosition = target.position; // Initialiseer de laatste positie
    }

    void Update()
    {
        if (target == null)
            return;

        // Controleer of de speler onder de spotlight is
        if (target.position.y < transform.position.y)
        {
            isTargetBelowSpotlight = true;
            lastTargetPosition = target.position; // Update de laatste positie
        }
        else
        {
            isTargetBelowSpotlight = false;
        }

        // Richt de spotlight op de speler of de laatste positie
        Vector3 targetPosition = isTargetBelowSpotlight ? target.position : lastTargetPosition;
        LookAtTarget(targetPosition);
    }

    void LookAtTarget(Vector3 targetPosition)
    {
        // Bereken de richting naar het doel
        Vector3 direction = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Pas de rotatie van de spotlight aan
        transform.rotation = targetRotation;
    }
}