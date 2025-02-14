using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBox : MonoBehaviour
{
    public Vector3 startRotation = Vector3.zero; // Beginrotatie (XYZ)
    public Vector3 targetRotation = new Vector3(0, 90, 0); // Eindrotatie (XYZ)
    public float rotationDuration = 2.0f; // Hoe lang de rotatie duurt
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Easy-in curve
    public bool startRotationBool = false; // Bool om de rotatie te starten

    private Quaternion initialRotation; // Beginrotatie als Quaternion
    private Quaternion finalRotation; // Eindrotatie als Quaternion
    private float startTime;
    private bool isRotating = false;

    void Start()
    {
        // Zet de beginrotatie van het object
        transform.rotation = Quaternion.Euler(startRotation);
        initialRotation = Quaternion.Euler(startRotation);
        finalRotation = Quaternion.Euler(targetRotation);
    }

    void Update()
    {
        // Start de rotatie als de bool is aangezet
        if (startRotationBool && !isRotating)
        {
            StartRotation();
        }

        if (isRotating)
        {
            // Bereken de progress van de rotatie
            float t = (Time.time - startTime) / rotationDuration;

            if (t < 1.0f)
            {
                // Pas de curve toe voor een easy-in effect
                float curveValue = rotationCurve.Evaluate(t);

                // Interpoleer soepel tussen de begin- en eindrotatie
                transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, curveValue);
            }
            else
            {
                // Zorg ervoor dat de rotatie precies op de eindrotatie staat
                transform.rotation = finalRotation;
                isRotating = false; // Stop de rotatie
                startRotationBool = false; // Zet de bool terug op false
            }
        }
    }

    // Start de rotatie
    public void StartRotation()
    {
        startTime = Time.time;
        isRotating = true;
    }
}