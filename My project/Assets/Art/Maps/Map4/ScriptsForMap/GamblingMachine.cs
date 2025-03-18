using System.Collections;
using UnityEngine;

public class GamblingMachine : MonoBehaviour
{
    [Header("Wheels Config")]
    public Transform[] wheels;
    public ParticleSystem wheelParticle; 

    [Header("Spin Settings")]
    public float minSpinDuration = 1.5f;
    public float maxSpinDuration = 3f;
    public float maxSpinSpeed = 500f;
    public float deceleration = 250f;
    public float snapSpeed = 2f;

    [Header("Lever Settings")]
    public Transform lever;
    public float leverUpAngle = 0f;
    public float leverDownAngle = -45f;
    public float leverOvershootAngle = -50f;
    public float leverMoveTime = 0.2f;
    public float leverReturnTime = 0.3f;

    [Header("Control")]
    public bool startSpin = false;

    private bool isSpinning = false;

    void Update()
    {
        if (startSpin && !isSpinning)
        {
            startSpin = false;
            StartCoroutine(HandleLeverAndSpin());
        }
    }

    IEnumerator HandleLeverAndSpin()
    {
        isSpinning = true;
        yield return LeverAnimation(leverUpAngle, leverOvershootAngle, leverMoveTime * 0.5f);
        yield return LeverAnimation(leverOvershootAngle, leverDownAngle, leverMoveTime * 0.5f);
        StartCoroutine(SpinWheels());
        yield return new WaitForSeconds(0.5f);
        yield return LeverAnimation(leverDownAngle, leverUpAngle + 5f, leverReturnTime * 0.6f);
        yield return LeverAnimation(leverUpAngle + 5f, leverUpAngle, leverReturnTime * 0.4f);
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
            StartCoroutine(SpinWheel(wheels[i], wheelParticle, randomSpinDuration));
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(randomSpinDuration + 1f);
        isSpinning = false;
    }

    IEnumerator SpinWheel(Transform wheel, ParticleSystem particle, float duration)
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
        particle.Play(); // Speel de particle effect af
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle < 0) angle += 360;
        return angle;
    }
}
