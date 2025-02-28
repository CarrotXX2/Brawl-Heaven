using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PainterCharacter : PlayerController
{
    [Header("Ultimate Stats")]
    [SerializeField] private float ultimateDuration;
    
    [Header("Logic")]
    private bool usingUltimate = false;
    
    public void Ultimate() // Draw a shape and give it a property
    {
        StartCoroutine(UltimateCoroutine());
    }

    private IEnumerator UltimateCoroutine()
    {
        usingUltimate = true;
        
        yield return new WaitForSeconds(ultimateDuration);
        usingUltimate = false;
    }

    private void ConfirmSelection()
    {
        
    }
    
}
