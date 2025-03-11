using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnMesh : MonoBehaviour
{
    public bool property, property2, property3, property4, property5, property6, property7, property8, property9, property10;
    public float lifeTime;
    void Start()
    {
        PlayerManager.Instance.AddDrawing(gameObject.GetComponent<MeshCollider>());
        //Destroy(gameObject, lifeTime);
        
    }

    private void OnDestroy()
    {
        PlayerManager.Instance.RemoveDrawing(gameObject.GetComponent<MeshCollider>());
    }
}
