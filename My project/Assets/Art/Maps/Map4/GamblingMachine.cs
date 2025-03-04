using System.Collections;
using UnityEngine;

public class GamblingMachine : MonoBehaviour
{
    [Header("Wheels Config")]
    public Transform[] wheels; // Sleep hier de 4 wielen in

    [Header("Spin Settings")]
    public float minSpinDuration = 1.5f; // Minimale spin duur
    public float maxSpinDuration = 3f; // Maximale spin duur
    public float maxSpinSpeed = 500f; // Maximale snelheid
    public float deceleration = 250f; // Hoe snel hij afremt
    public float snapSpeed = 2f; // Snelheid van de smooth stop

    [Header("Lever Settings")]
    public Transform lever; // De hendel Transform
    public float leverUpAngle = 0f; // Hoek wanneer de hendel omhoog staat
    public float leverDownAngle = -45f; // Hoek wanneer de hendel omlaag is getrokken
    public float leverOvershootAngle = -50f; // Kleine overshoot voor animatie
    public float leverMoveTime = 0.2f; // Tijd voor de hendelbeweging omlaag
    public float leverReturnTime = 0.3f; // Tijd voor de hendelbeweging omhoog

    [Header("Control")]
    public bool startSpin = false; // Zet deze op true om te starten

    private bool isSpinning = false;

    void Update()
    {
        if (startSpin && !isSpinning)
        {
            startSpin = false; // Reset bool in de Inspector
            StartCoroutine(HandleLeverAndSpin());
        }
    }

    IEnumerator HandleLeverAndSpin()
    {
        isSpinning = true;

        // Beweeg hendel omlaag met animatie effect
        yield return LeverAnimation(leverUpAngle, leverOvershootAngle, leverMoveTime * 0.5f); // Snelle overshoot
        yield return LeverAnimation(leverOvershootAngle, leverDownAngle, leverMoveTime * 0.5f); // Terug naar eindpositie

        // Direct starten met spinnen na hendelbeweging
        StartCoroutine(SpinWheels());

        // Wacht een kleine tijd voordat de hendel teruggaat
        yield return new WaitForSeconds(0.5f);

        // Beweeg hendel omhoog met lichte schokkerige animatie
        yield return LeverAnimation(leverDownAngle, leverUpAngle + 5f, leverReturnTime * 0.6f); // Snelle terugkeer
        yield return LeverAnimation(leverUpAngle + 5f, leverUpAngle, leverReturnTime * 0.4f); // Kleine bounce

        // Reset zodat de hendel opnieuw gebruikt kan worden
        isSpinning = false;
    }

    IEnumerator LeverAnimation(float startAngle, float endAngle, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, elapsedTime / duration);
            lever.localEulerAngles = new Vector3(angle, lever.localEulerAngles.y, lever.localEulerAngles.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lever.localEulerAngles = new Vector3(endAngle, lever.localEulerAngles.y, lever.localEulerAngles.z);
    }

    IEnumerator SpinWheels()
    {
        isSpinning = true;
        float randomSpinDuration = Random.Range(minSpinDuration, maxSpinDuration);

        for (int i = 0; i < wheels.Length; i++)
        {
            StartCoroutine(SpinWheel(wheels[i], randomSpinDuration));
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(randomSpinDuration + 1f);
        isSpinning = false;
    }

    IEnumerator SpinWheel(Transform wheel, float duration)
    {
        float currentRotation = NormalizeAngle(wheel.eulerAngles.x);
        float startRotation = Mathf.Round(currentRotation / 25) * 25;
        wheel.eulerAngles = new Vector3(startRotation, wheel.eulerAngles.y, wheel.eulerAngles.z);

        float spinTime = duration;
        float speed = maxSpinSpeed;

        while (spinTime > 0)
        {
            wheel.Rotate(Vector3.left * speed * Time.deltaTime);
            spinTime -= Time.deltaTime;
            speed = Mathf.Max(speed - deceleration * Time.deltaTime, 50f);
            yield return null;
        }

        // Stoppen op de juiste positie (smooth animatie)
        float targetRotation = Mathf.Round(NormalizeAngle(wheel.eulerAngles.x) / 25) * 25;
        float elapsedTime = 0f;
        Vector3 initialRotation = wheel.eulerAngles;
        Vector3 finalRotation = new Vector3(targetRotation, initialRotation.y, initialRotation.z);

        while (elapsedTime < 1f / snapSpeed)
        {
            wheel.eulerAngles = Vector3.Lerp(initialRotation, finalRotation, elapsedTime * snapSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        wheel.eulerAngles = finalRotation;
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle < 0) angle += 360;
        return angle;
    }
}
