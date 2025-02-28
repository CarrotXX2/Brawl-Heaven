using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LocalMultiplayerManager : MonoBehaviour
{
    public static LocalMultiplayerManager Instance;

    public List<int> playersConnected = new List<int>(); // All connected players
    
    private void Awake()
    {
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
       // GameplayManager.Instance.playersAlive = playersConnected;
    }
    
    public void ConnectPlayer()
    {
        playersConnected.Add(playersConnected.Count + 1);
    }

    public void DisconnectPlayer()
    {
        playersConnected.RemoveAt(playersConnected.Count - 1);
    }
    public void RemovePlayer(GameObject player)
    {
      //  playersConnected.Remove(player);
    }

    public void OnGameStart()
    {
        GameplayManager.Instance.SpawnPlayers();
    }
    
    
}
