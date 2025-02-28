using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    
    public List<Player> players = new List<Player>();
    public GameObject playerPrefab;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);
    }

    public void ConnectPlayer()
    {
        // players[0].device  = connectedDevice;
    }
    
    public void RemovePlayer(int playerID)
    {
        players.RemoveAt(playerID);
    }

    public void StartGame()
    {
        foreach (var player in players)
        {
            player.OnGameStart();
        }
    }

    public void ChooseCharacter()
    {
        foreach (var player in players)
        {
            player.characterPrefab = playerPrefab;
        }
    }
    
}
