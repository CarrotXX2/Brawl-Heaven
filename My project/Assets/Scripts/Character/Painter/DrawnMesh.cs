using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DrawnMesh : MonoBehaviour
{
    [Header("Player Management")]
    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> playersInRange = new List<GameObject>();

    [Header("ScriptableObjects")] 
    public DrawingProperties drawingProperties; // When mesh gets created you can choose a property for the drawing
    public Property property;
    
    [Header("SmokeScreen Logic")]
    public ParticleSystem smokeParticleSystem;

    private bool smokeActivated;
    [Header("Healing Logic")]
    private float healingTimer;

    [Header("Explosion logic")] 
    private float explosionTimer;
    void Start()
    {
        Destroy(gameObject, drawingProperties.lifeTime);
        players = GameplayManager.Instance.playersAlive;
        
        GetComponent<MeshRenderer>().material = drawingProperties.mainMaterial;
        property = drawingProperties.property;
        
        healingTimer += Time.time + drawingProperties.healingInterval;
        explosionTimer = drawingProperties.explosionTimer;
    }

    private void Update()
    {
        CalculateDistance();
        
        switch (property)
        {
            case Property.SmokeScreen:
            {
                if (!smokeActivated)
                {
                     ParticleSystem.ShapeModule shape = smokeParticleSystem.shape;
                     shape.shapeType = ParticleSystemShapeType.Mesh;
                     shape.mesh = gameObject.GetComponent<MeshFilter>().mesh; // Set the mesh dynamically
                     ParticleSystem smokeParticle = Instantiate(smokeParticleSystem, transform);
                     smokeParticle.Play();
                     
                     smokeActivated = true;
                }
            }
                break;
            case Property.Blacklhole: 
            {
                // Pull all players towards the black hole for a short amount of time 
                foreach (GameObject player in playersInRange)
                {
                    // Get direction of black hole 
                    Vector3 directionToBlackHole = (transform.position - player.transform.position).normalized;
        
                    // Apply force toward black hole
                    player.GetComponent<Rigidbody>().AddForce(directionToBlackHole * drawingProperties.pullForce, ForceMode.Force);

                }
            }
                break;
            case Property.Explosion:
            {
                if (Time.time > explosionTimer)
                {
                    foreach (var player in playersInRange)
                    {
                        // This is terrible but had to make some kind of workaround since I don't have the time to make 
                        // The functions more modular 
                        player.GetComponent<PlayerController>().TakeDamage(null, null, drawingProperties.damage);
                        player.GetComponent<PlayerController>().TakeKB(null,null ,drawingProperties.knockBack);
                    }
                }
            }
                break;
            case Property.Healingzone:
            {
                // After a set ammount of time, heal all players inside the drawing
                if (Time.time > healingTimer)
                {
                    foreach (GameObject player in playersInRange)
                    {
                        player.GetComponent<PlayerController>().Heal(drawingProperties.healingAmount);
                    }
                    
                    healingTimer += Time.time + drawingProperties.healingInterval;
                }
                
            }
                break;
        }
    }

    private void CalculateDistance()
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
                if (!IsPlayerInside(player))
                {
                    print("Player In range");
                    AddPlayer(player);
                }
            }
            else
            {
                if (IsPlayerInside(player))
                {
                    print("Player out of range");
                    RemovePlayer(player);
                }
            }
        }
    }

    private void AddPlayer(GameObject player)
    {
        if (!playersInRange.Contains(player))
        {
            playersInRange.Add(player);
            // Do whatever else you need when a player enters
        }
    }
    
    private void RemovePlayer(GameObject player)
    {
        playersInRange.Remove(player);
        // Do whatever else you need when a player exits
    }
    
    public bool IsPlayerInside(GameObject player)
    {
        return playersInRange.Contains(player);
    }

    private void OnDestroy()
    {
        GameplayManager.Instance.RemoveDrawing(gameObject.GetComponent<PolygonCollider2D>());
    }
}
