using UnityEngine;

public class AttackColliders : MonoBehaviour
{
   public AttackData attackData;
   public float chargedDamage;
   public bool isHeavyAttack;
   public Transform playerTransform;
   private void OnTriggerEnter(Collider other)
   {
      if (isHeavyAttack)
      {
         //HashSet<GameObject> hitTargets = new HashSet<GameObject>();
               
         IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
                     
         // Ignore self
         if (other.transform.IsChildOf(transform.parent)) return;
                     
         // if (hitTargets.Contains(other.gameObject)) return;
                     
         if (damageable != null)
         {
            damageable.TakeDamage(attackData, playerTransform, chargedDamage);
            print(chargedDamage);
            // hitTargets.Add(other.gameObject);
         }
      }
      else
      {
         // HashSet<GameObject> hitTargets = new HashSet<GameObject>();

         IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

         // Ignore self
         if (other.transform.IsChildOf(transform.parent)) return;

         // if (hitTargets.Contains(other.gameObject)) return;

         if (damageable != null)
         {
            damageable.TakeDamage(attackData, playerTransform, attackData.damage);

            // hitTargets.Add(other.gameObject);
         }
      }
   }
}
