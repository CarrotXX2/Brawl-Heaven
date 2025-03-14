using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PainterCharacter : PlayerController
{    
    // TODO optimize colliders, mesh doesnt properly get applied on mesh filter
    [Header("Ultimate Stats")]
    [SerializeField] private float ultimateDuration;

    [Header("Drawing Logic")] 
    private List<GameObject> generatedObjects = new List<GameObject>();
    
    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject cursorInstance;
    
    [SerializeField] private float cursorSpeed;
    
    private Vector2 cursorPosition;
    private Vector2 cursorMoveInput;

    private bool startDrawing;
    
    private List<Vector3> drawnPoints = new List<Vector3>();
    
    [Space]
    private bool usingUltimate = false;
    
    protected override void Update()
    {
        if (!usingUltimate)
        {
            base.Update();
        }
        else
        {
            MoveUltimateCursor();
            
            if (startDrawing)
            {
                DrawShape();
            }
        }
    }
    
    public override void OnUltimateCast(InputAction.CallbackContext ctx) // Draw a shape and give it a property
    {
        if (!ctx.started) return;
        
        drawnPoints.Clear();
        rb.isKinematic = true; // Player cant move or fall while drawing 
        
        print("Cast Ultimate");
        
        cursorInstance = Instantiate(cursor, lineCastTransform.position, quaternion.identity);
        
        StartCoroutine(UltimateCoroutine());
    }
    private IEnumerator UltimateCoroutine()
    {
        usingUltimate = true;
        // Wait for an animation to finish playing before player is allowed to draw 
        startDrawing = true;
        
        yield return new WaitForSeconds(ultimateDuration + 5);
        
        if (Vector2.Distance(drawnPoints.First(), drawnPoints.Last()) < 5f && drawnPoints.Count >= 3)
        {
            drawnPoints.Add(drawnPoints.First()); // Close the loop
                
            GenerateMesh();
        }
        
        Destroy(cursorInstance);
        rb.isKinematic = false;
        usingUltimate = false;
        startDrawing = false;
    }
    
    private void MoveUltimateCursor()
    {
        cursorMoveInput = moveInput;
        
        cursorPosition += cursorMoveInput * cursorSpeed * Time.deltaTime;

        // Clamp cursor position to stay within screen bounds
       // cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
       // cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        // Move the cursor in UI
        cursorInstance.GetComponent<Transform>().position = cursorPosition;
    }   

     private void DrawShape()
     {
         if (drawnPoints.Count == 0 || Vector3.Distance(cursorPosition, drawnPoints[drawnPoints.Count - 1]) > 0.2f)
         {
             drawnPoints.Add(cursorPosition);
         }
     }
 
     private void GenerateMesh()
     {
         if (drawnPoints.Count < 3)
         {
             Debug.LogWarning("Need at least 3 points to create a mesh");
             return;
         }
 
         // Clean up any previously generated objects
         foreach (var obj in generatedObjects)
         {
             if (obj != null)
                 Destroy(obj);
         }
         generatedObjects.Clear();
 
         // Create parent object to hold everything
         GameObject parentObject = new GameObject("GeneratedMeshes");
         generatedObjects.Add(parentObject);
 
         // Decompose the shape into convex parts
         List<List<Vector3>> convexParts = ConvexDecomposition.DecomposeToConvexPolygons(drawnPoints);
 
         // Create a mesh for each convex part
         for (int partIndex = 0; partIndex < convexParts.Count; partIndex++)
         {
             List<Vector3> vertices = convexParts[partIndex];
             
             // Skip invalid parts
             if (vertices.Count < 3)
                 continue;
                 
             // Triangulate the convex part (simpler algorithm for convex shapes)
             List<int> triangles = ConvexDecomposition.TriangulateConvex(vertices);
             
             // Create mesh
             Mesh mesh = new Mesh();
             mesh.vertices = vertices.ToArray();
             mesh.triangles = triangles.ToArray();
             
             // Generate UVs
             GenerateUVs(mesh);
             
             mesh.RecalculateNormals();
             mesh.RecalculateTangents();
 
             // Create GameObject for this part
             GameObject partObject = new GameObject($"ConvexPart_{partIndex}");
             partObject.transform.parent = parentObject.transform;
             generatedObjects.Add(partObject);
 
             // Add components
             MeshFilter meshFilter = partObject.AddComponent<MeshFilter>();
             MeshRenderer meshRenderer = partObject.AddComponent<MeshRenderer>();
             
             // Assign the mesh
             meshFilter.mesh = mesh;
             
             // Set material (different color for each part to visualize)
             Material material = new Material(Shader.Find("Standard"));
             Color partColor = Color.HSVToRGB((float)partIndex / convexParts.Count, 0.7f, 0.9f);
             material.color = partColor;
             meshRenderer.material = material;
             
             // Add collider
             MeshCollider meshCollider = partObject.AddComponent<MeshCollider>();
             meshCollider.sharedMesh = mesh;
             meshCollider.convex = true;  // This will work now since each part is convex!
             meshCollider.isTrigger = true;
         }
     }
 
     private void GenerateUVs(Mesh mesh)
     {
         Vector3[] vertices = mesh.vertices;
         Vector2[] uvs = new Vector2[vertices.Length];
         
         // Find bounds of the mesh
         Bounds bounds = CalculateBounds(vertices);
         float width = bounds.size.x;
         float height = bounds.size.y;
         
         // Simple planar mapping for 2D shapes
         for (int i = 0; i < vertices.Length; i++)
         {
             // Normalize vertex coordinates to 0-1 range based on bounds
             float u = (vertices[i].x - bounds.min.x) / (width == 0 ? 1 : width);
             float v = (vertices[i].y - bounds.min.y) / (height == 0 ? 1 : height);
             
             uvs[i] = new Vector2(u, v);
         }
         
         mesh.uv = uvs;
     }
 
     private Bounds CalculateBounds(Vector3[] vertices)
     {
         if (vertices.Length == 0)
             return new Bounds(Vector3.zero, Vector3.zero);
         
         Vector3 min = vertices[0];
         Vector3 max = vertices[0];
         
         for (int i = 1; i < vertices.Length; i++)
         {
             min = Vector3.Min(min, vertices[i]);
             max = Vector3.Max(max, vertices[i]);
         }
         
         return new Bounds((min + max) * 0.5f, max - min);
     }
 
    private void ConfirmSelection()
    {
        
    }
    
}
