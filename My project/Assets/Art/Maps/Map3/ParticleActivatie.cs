using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParticleActivator : MonoBehaviour
{
    [Header("Particle Settings")]
    public ParticleSystem particleToActivate;  // ParticleSystem dat geactiveerd moet worden
    public float cooldownTime = 1f;           // Tijd tussen activaties

    private bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        // Controleer of cooldown actief is
        if (isOnCooldown) return;

        // Controleer of het een speler is
        if (IsPlayer(other.gameObject))
        {
            // Activeer het particle system
            if (particleToActivate != null)
            {
                particleToActivate.Play();
            }

            // Start cooldown
            StartCoroutine(Cooldown());
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        // Controleer of GameplayManager en playersAlive bestaan
        if (GameplayManager.Instance == null || GameplayManager.Instance.playersAlive == null)
            return false;

        // Loop door alle levende spelers
        foreach (var player in GameplayManager.Instance.playersAlive)
        {
            if (player != null && player.gameObject == obj)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }
}