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
    
    [Header("Drawing management")]
    public List<PolygonCollider2D> drawings = new List<PolygonCollider2D>(); // Keeps track of every current drawing/ultimate on the field
    [SerializeField] private float distanceThreshold;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       Invoke("SpawnPlayers", 3);
    }

    private void Update()
    {
        foreach (var drawing in drawings)
        {
            foreach (var player in players)
            {
                
            }
        }
    }
    
    public void SpawnPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].SetActive(true);
            players[i].transform.position = startSpawnPoints[i].position;
            
            print($"Player {i} spawned");
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
    public void AddDrawing(PolygonCollider2D collider)
    {
        drawings.Add(collider);
    }

    public void RemoveDrawing(PolygonCollider2D collider)
    {
        drawings.Remove(collider);
    }


    private void PlayerWin(GameObject player)
    {
        
    }
}
