using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    
    [Header("Players")]
    public List<GameObject> players = new List<GameObject>();

    public List<GameObject> playersAlive = new List<GameObject>();
        
    [Header("Spawn logic")]    
    [SerializeField] private float respawnTime;
    [SerializeField] private Transform respawnPoint;
    
    [SerializeField] private Transform[] startSpawnPoints; // The first 4 spawnpoints when the game starts
    
    [Header("UI Panels")]
    [SerializeField] private GameObject playerWinPanel;
    
    private void Awake()
    {
        Instance = this;
    }
    public void SpawnPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = startSpawnPoints[i].position;
        }
    }
    public IEnumerator RespawnPlayer(GameObject player)
    {
        // do some cool stuff 
        yield return new WaitForSeconds(respawnTime);
        
        player.GetComponent<PlayerController>().touchedDeathZone = false;
        player.GetComponent<Rigidbody>().velocity = Vector2.zero;
        player.GetComponent<Rigidbody>().isKinematic = false;
        
        player.transform.position = respawnPoint.position;
    }

    public void PlayerDeath(GameObject player) // Removes the player from the list of alive players and checks who is still alive
    {
        playersAlive.Remove(player);
        
        if (playersAlive.Count == 1)
        {
            PlayerWin(playersAlive[0]);
        }
    }

    private void PlayerWin(GameObject player)
    {
        
    }
}
