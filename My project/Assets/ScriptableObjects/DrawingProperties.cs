using UnityEngine;

public enum Property
{
    Blacklhole,
    Healingzone,
    SmokeScreen,
    Explosion,
}
[CreateAssetMenu(fileName = "DrawingProperties", menuName = "ScriptableObjects/DrawingProperties", order = 1)]
public class DrawingProperties : ScriptableObject
{
    public string propertyName; // Name of property 
    public Property property;
    
    [Header("Logic")]
    public float lifeTime; // The amount of time drawing stays in play 
    public float distanceThreshold; // The distance needed for the player to be considered in range
    
    [Header("Rendering")]
    public float outlineWidth;
    public Material mainMaterial; // Main material for the drawing
    public Material backGroundMaterial; // The drawing consist of 2 meshes to give it a proper outline 
    
    [Header("Healing")]
    public int healingAmount; 
    public float healingInterval; 
    
    [Header("BlackHole")]
    public float pullForce; // The strength at which the players gets pulled to the center 

    [Header("Explosion")] 
    public float damage; // Amount of damage the explosion deals to each player in range 
    public float knockBack; // Amount of knockback that gets apllied to each player in range
    public float explosionTimer;
}
