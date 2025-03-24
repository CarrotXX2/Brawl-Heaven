using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    
    [Header("Lists")]
    public List<Player> players = new List<Player>(); // Keeps track of every connected player up to 4
    
    public GameObject playerPrefab; // this is just for testing 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

   
   /* float GetDistanceToColliderEdge(GameObject player, MeshCollider meshCollider) 
    {
        // Since non-convex mesh cant be a trigger the "Collision" detection is based on distance 
        
        Vector3 closestPoint = meshCollider.ClosestPoint(player.transform.position);
        return Vector3.Distance(player.transform.position, closestPoint);
    }*/
}
