using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Fighter/Attack")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public AnimationClip animation;
    
    public BoxCollider[] colliders;
    
    public float damage;
    public float knockback;
    public float startupTime;
    public float activeTime;
    public float moveDuration;
}