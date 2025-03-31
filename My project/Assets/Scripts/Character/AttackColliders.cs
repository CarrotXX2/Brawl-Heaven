using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackColliders : MonoBehaviour
{
   public AttackData attackData;
   public float chargedDamage;
   public bool isHeavyAttack;
   private void OnTriggerEnter(Collider other)
   {
      if (isHeavyAttack)
      {
         HashSet<GameObject> hitTargets = new HashSet<GameObject>();
               
         IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
                     
         // Ignore self
         if (other.transform.IsChildOf(transform.parent)) return;
                     
         if (hitTargets.Contains(other.gameObject)) return;
                     
         if (damageable != null)
         {
            damageable.TakeDamage(attackData, transform, chargedDamage);
                         
            hitTargets.Add(other.gameObject);
         }
      }
      else
      {
         HashSet<GameObject> hitTargets = new HashSet<GameObject>();

         IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

         // Ignore self
         if (other.transform.IsChildOf(transform.parent)) return;

         if (hitTargets.Contains(other.gameObject)) return;

         if (damageable != null)
         {
            damageable.TakeDamage(attackData, transform, attackData.damage);

            hitTargets.Add(other.gameObject);
         }
      }

   }
}
