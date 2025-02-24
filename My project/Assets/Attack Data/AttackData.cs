using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Fighter/Attack")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public AnimationClip animation;
    
    public float damage; 
    public float knockback;
    public float knockUp;
    public float hitStun; // Recovery time needed after being hit
    public Vector2 hitDirection; // Direction of the knockback
    
    
    public float startupTime; // How long it takes for the collider to be enabled
    public float activeTime; // How long the collider is active 
    public float moveDuration;
}