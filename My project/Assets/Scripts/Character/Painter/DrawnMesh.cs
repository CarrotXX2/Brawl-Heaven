using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
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

    private bool exploded;
    void Start()
    {
        Destroy(gameObject, drawingProperties.lifeTime);
        players = GameplayManager.Instance.playersAlive;
        
        GetComponent<MeshRenderer>().material = drawingProperties.mainMaterial;
        property = drawingProperties.property;

        switch (property)
        {
            case Property.SmokeScreen:
            {
                ParticleSystem.ShapeModule shape = smokeParticleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Mesh;
                shape.mesh = gameObject.GetComponent<MeshFilter>().mesh; // Set the mesh dynamically
                
                // Put the smokescreen more forward 
                ParticleSystem smokeParticle = Instantiate(smokeParticleSystem, transform.position + new Vector3(0,0,-0.4f), Quaternion.identity);
                Destroy(smokeParticle, 14);
            }
                break;
            case Property.Explosion:
            {
                 explosionTimer = Time.time + drawingProperties.explosionTimer;
            }
                break;
            case Property.Healingzone:
            {
                healingTimer += Time.time + drawingProperties.healingInterval;
            }
                break;
        }
    }

    private void Update()
    {
        switch (property)
        {
            case Property.SmokeScreen:
            {
                // Do nothing :)
            }
                break;
            case Property.Blacklhole: 
            {
                CalculateDistance();
                
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
                CalculateDistance();
                
                if (Time.time > explosionTimer && !exploded)
                {
                    foreach (var player in playersInRange)
                    {
                        // This is terrible but had to make some kind of workaround since I don't have the time to make 
                        // The functions more modular 
                        player.GetComponent<PlayerController>().TakeDamage(null, null, drawingProperties.damage);
                        player.GetComponent<PlayerController>().TakeKB(null,transform ,drawingProperties.knockBack);
                    }
                    exploded = true;
                }
            }
                break;
            case Property.Healingzone:
            {
                CalculateDistance();
                
                // After a set ammount of time, heal all players inside the drawing
                if (Time.time > healingTimer)
                {
                    foreach (GameObject player in playersInRange)
                    {
                        player.GetComponent<PlayerController>().Heal(drawingProperties.healingAmount);
                    }
                    
                    healingTimer = Time.time + drawingProperties.healingInterval;
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
