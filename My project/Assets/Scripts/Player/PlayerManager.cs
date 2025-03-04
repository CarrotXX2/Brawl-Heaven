using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    
    public List<Player> players = new List<Player>();
    public GameObject playerPrefab; // this is just for testing 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddPlayer(GameObject player)
    {
        player.GetComponent<Player>().playerID = players.Count;
        
        players.Add(player.GetComponent<Player>());
    }

    public void CharacterSelection()
    {
        foreach (var player in players)
        {
            player.OnCharacterSelectStart();
        }
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
