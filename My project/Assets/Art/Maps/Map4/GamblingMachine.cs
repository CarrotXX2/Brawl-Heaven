using System.Collections;
using UnityEngine;

public class GamblingMachine : MonoBehaviour
{
    public Transform[] wheels; // Sleep hier de 4 wielen in
    public float minSpinDuration = 1.5f; // Minimale spin duur
    public float maxSpinDuration = 3f; // Maximale spin duur
    public float maxSpinSpeed = 500f; // Maximale snelheid
    public float deceleration = 250f; // Hoe snel hij afremt
    public float snapSpeed = 2f; // Snelheid van de smooth stop
    public bool startSpin = false; // Zet deze op true om te starten

    private bool isSpinning = false;

    void Update()
    {
        if (startSpin && !isSpinning)
        {
            startSpin = false; // Reset bool in de Inspector
            StartCoroutine(SpinWheels());
        }
    }

    IEnumerator SpinWheels()
    {
        isSpinning = true;

        // Genereer één willekeurige spin duration voor alle wielen
        float randomSpinDuration = Random.Range(minSpinDuration, maxSpinDuration);

        for (int i = 0; i < wheels.Length; i++)
        {
            StartCoroutine(SpinWheel(wheels[i], randomSpinDuration));
            yield return new WaitForSeconds(0.5f); // Wacht even voor het volgende wiel start
        }

        yield return new WaitForSeconds(randomSpinDuration + 1f); // Wacht tot alle wielen klaar zijn
        isSpinning = false;
    }

    IEnumerator SpinWheel(Transform wheel, float duration)
    {
        // Zorg ervoor dat de startpositie een veelvoud van 25 is
        float currentRotation = NormalizeAngle(wheel.eulerAngles.x);
        float startRotation = Mathf.Round(currentRotation / 25) * 25;
        wheel.eulerAngles = new Vector3(startRotation, wheel.eulerAngles.y, wheel.eulerAngles.z);

        float spinTime = duration;
        float speed = maxSpinSpeed;

        while (spinTime > 0)
        {
            wheel.Rotate(Vector3.left * speed * Time.deltaTime);
            spinTime -= Time.deltaTime;
            speed = Mathf.Max(speed - deceleration * Time.deltaTime, 50f); // Zorgt voor smooth afremmen
            yield return null;
        }

        // **Stoppen op de juiste positie (smooth animatie)**
        float targetRotation = Mathf.Round(NormalizeAngle(wheel.eulerAngles.x) / 25) * 25; // Dichtstbijzijnde 25 vinden
        float elapsedTime = 0f;
        Vector3 initialRotation = wheel.eulerAngles;
        Vector3 finalRotation = new Vector3(targetRotation, initialRotation.y, initialRotation.z);

        while (elapsedTime < 1f / snapSpeed)
        {
            wheel.eulerAngles = Vector3.Lerp(initialRotation, finalRotation, elapsedTime * snapSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // **Zet de rotatie exact op het icoon**
        wheel.eulerAngles = finalRotation;

        // **Extra controle: zorg ervoor dat de rotatie altijd een veelvoud van 25 is**
        float finalRotationNormalized = NormalizeAngle(wheel.eulerAngles.x);
        float finalRotationRounded = Mathf.Round(finalRotationNormalized / 25) * 25;
        wheel.eulerAngles = new Vector3(finalRotationRounded, wheel.eulerAngles.y, wheel.eulerAngles.z);
    }

    // Normalize the angle to be within 0-360 degrees
    float NormalizeAngle(float angle)
    {
        angle = angle % 360; // Zorg ervoor dat de hoek binnen -360 en 360 valt
        if (angle < 0) angle += 360; // Zorg ervoor dat de hoek altijd positief is
        return angle;
    }
}