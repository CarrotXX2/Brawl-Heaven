using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Fighter/Attack")]
public class AttackData : ScriptableObject
{
    [Header("Name & Animation")]
    public string attackName; // Name of the attack 
    public AnimationClip animation; // Main attack animation
    public AnimationClip chargeAnimation; // Heavy attacks have a charge animation
    
    [Header("Attack Properties")]
    public float damage;

    public bool chargeAttack;
    public float minChargeDamage;
    public float maxChargeDamage;
    public float maxChargeTime;
    
    public float knockback;
    public Vector2 hitDirection; // Direction of the knockback, value should be between 0 and 1
    public float moveDuration;
    
    public bool unstoppable = false; // If true, move can't be cancelled when hit
    public float hitStun; // Recovery time needed after being hit
    
    [Header("Collider Properties")]
    public float startupTime; // How long it takes for the collider to be enabled
    public float activeTime; // How long the collider is active 

    [Header("Attack Movement")] 
    public bool moveOnAttack; // Check yes if attack should move the player towards a direction 
    public Vector2 movementDirection; // Value Should be 10 for best performance 
    public float moveForce; // Strength of the move 

}