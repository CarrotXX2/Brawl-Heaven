using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int playerID;
    
    public GameObject characterPrefab; // The character the player selects in the player selection screen
    public PlayerInputManager inputManager;
    
    void Awake()
    {
        PlayerManager.Instance.AddPlayer(this);
    }

    public void OnGameStart()
    {
        Instantiate(characterPrefab, transform); // Instantiate the character 
    }

    public void DisconnectPlayer()
    {
        PlayerManager.Instance.RemovePlayer(playerID);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
    }
}
