using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CustomButtonHandler : MonoBehaviour, ICustomButtonHandler
{
  public UnityEvent unityEvent;
  public bool characterSelect;
  private bool playersReady;

  public void OnButtonClicked()
  {
    if (characterSelect == true)
    {
      bool allPlayersReady = true;

      foreach (var player in PlayerManager.Instance.players)
      {
        if (player.characterPrefab == null)
        {
          allPlayersReady = false;
          break; // No need to check other players if we found one who isn't ready
        }
      }

      if (allPlayersReady == true)
      {
        unityEvent?.Invoke();
      }
      else
      {
        // Optional: Add feedback to show some players haven't selected a character
        Debug.Log("Not all players have selected a character!");
      }
    }
    else
    {
      unityEvent?.Invoke();
    }
  }
}
