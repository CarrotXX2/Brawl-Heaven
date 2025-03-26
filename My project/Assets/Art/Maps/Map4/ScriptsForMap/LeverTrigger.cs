using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LeverTrigger : MonoBehaviour
{
    public event Action<GameObject> OnLeverPulled;

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other.gameObject))
        {
            OnLeverPulled?.Invoke(other.gameObject);
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        if (GameplayManager.Instance == null || GameplayManager.Instance.playersAlive == null)
            return false;

        foreach (var player in GameplayManager.Instance.playersAlive)
        {
            if (player != null && player.gameObject == obj)
            {
                return true;
            }
        }
        return false;
    }
}