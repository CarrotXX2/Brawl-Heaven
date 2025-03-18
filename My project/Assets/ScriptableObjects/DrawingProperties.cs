using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawingProperties", menuName = "ScriptableObjects/DrawingProperties", order = 1)]
public class DrawingProperties : ScriptableObject
{
    public string name; // Name of property 
    
    [Header("Logic")]
    public float lifeTime; // The amount of time drawing stays in play 
    public float distanceThreshold; // The distance needed for the player to be considered in range
    
    [Header("Rendering")]
    public float outlineWidth;
    public Material mainMaterial; // Main material for the drawing
    public Material backGroundMaterial; // The drawing consist of 2 meshes to give it a proper outline 
}
