using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnMesh : MonoBehaviour
{
    [Header("Player Management")]
    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> playersInside = new List<GameObject>();

    [Header("ScriptableObjects")] 
    public DrawingProperties drawingProperties; // When mesh gets created you can choose a property for the drawing

    void Start()
    {
        players = GameplayManager.Instance.playersAlive;
        
        GetComponent<MeshRenderer>().material = drawingProperties.mainMaterial;
    }

    private void Update()
    {
        foreach (var player in players)
        {
            PolygonCollider2D polygonCollider2D = GetComponent<PolygonCollider2D>();
                
            Vector3 playerPosition = player.GetComponent<PlayerController>().playerTransform.position;
            // Get the closest point of the collider from the player, Z-Axis isn't looked at
            Vector2 closestPoint = polygonCollider2D.ClosestPoint(playerPosition);

            float distance = Vector2.Distance(playerPosition, closestPoint);

            print(distance);

            // If distance is in a certain threshold player is inside the "Trigger Area" 

            if (distance <= drawingProperties.distanceThreshold)
            {
                if (IsPlayerInside(player))
                {
                    print("Player entered");
                    AddPlayer(player);
                }
            }
            else
            {
                if (IsPlayerInside(player))
                {
                    print("Player exited");
                    RemovePlayer(player);
                }
            }
        }
    }

    private void AddPlayer(GameObject player)
    {
        if (!playersInside.Contains(player))
        {
            playersInside.Add(player);
            // Do whatever else you need when a player enters
        }
    }
    
    private void RemovePlayer(GameObject player)
    {
        playersInside.Remove(player);
        // Do whatever else you need when a player exits
    }
    
    public bool IsPlayerInside(GameObject player)
    {
        return playersInside.Contains(player);
    }
    #region SmokeScreen

    public void StartSmokeScreen()
    {
        
    }    

    #endregion
    
    #region BlackHole

    public void StartBlackHole()
    {
        
    }
    #endregion

    #region Lava

    public void StartLava()
    {
        
    }

    #endregion
    private void OnDestroy()
    {
        GameplayManager.Instance.RemoveDrawing(gameObject.GetComponent<PolygonCollider2D>());
    }
}
