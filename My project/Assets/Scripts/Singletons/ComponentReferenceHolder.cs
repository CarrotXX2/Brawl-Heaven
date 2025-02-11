using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ComponentReferenceHolder : MonoBehaviour
{
    public static ComponentReferenceHolder Instance;
    public CinemachineTargetGroup cmtg;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        
    }
}
