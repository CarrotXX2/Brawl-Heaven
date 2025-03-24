using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public interface IKnockbackable
{
    public void TakeKB([CanBeNull] AttackData attackData, Transform kbSource, float kb);
}
    
