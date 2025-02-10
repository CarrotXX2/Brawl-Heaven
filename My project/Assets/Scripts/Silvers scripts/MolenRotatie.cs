using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolenRotatie : MonoBehaviour
{
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // As waaromheen geroteerd wordt (standaard om de Y-as)
    public float rotationSpeed = 50f; // Snelheid van de rotatie (graden per seconde)

    void Update()
    {
        // Roteer het object continu op basis van de opgegeven as en snelheid
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}