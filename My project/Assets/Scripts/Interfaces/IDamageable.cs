using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage([CanBeNull] AttackData attackData, [CanBeNull] Transform enemyTransform, float damage);
    public void TakeDamage(AttackData attackData,float chargedDamage ,Transform enemyTransform);
}
