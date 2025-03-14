using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(AttackData attackData, Transform enemyTransform);
    public void TakeDamage(AttackData attackData,float chargedDamage ,Transform enemyTransform);
}
