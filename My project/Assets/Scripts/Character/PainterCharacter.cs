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
    [Header("Ultimate Stats")]
    [SerializeField] private float ultimateDuration;

    [Header("Drawing Logic")] 
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
        
        cursorInstance = Instantiate(cursor, transform.position, quaternion.identity);
        
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
    private List<int> Triangulate(List<Vector3> vertices)
    {
        List<int> indices = new List<int>();
        if (vertices.Count < 3) return indices; // Need at least 3 points for a triangle

        List<int> remainingIndices = new List<int>();
        for (int i = 0; i < vertices.Count; i++)
            remainingIndices.Add(i);

        while (remainingIndices.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < remainingIndices.Count; i++)
            {
                int prev = remainingIndices[(i - 1 + remainingIndices.Count) % remainingIndices.Count];
                int curr = remainingIndices[i];
                int next = remainingIndices[(i + 1) % remainingIndices.Count];

                if (IsEar(vertices, prev, curr, next, remainingIndices))
                {
                    indices.Add(prev);
                    indices.Add(next);
                    indices.Add(curr);
                    
                    remainingIndices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound) break; // Prevent infinite loops if no ears are found (shouldn't happen in a valid shape)
        }

        // Add the final triangle
        if (remainingIndices.Count == 3)
        {
            indices.Add(remainingIndices[0]);
            indices.Add(remainingIndices[1]);
            indices.Add(remainingIndices[2]);
        }

        return indices;
    }

    private bool IsEar(List<Vector3> vertices, int prev, int curr, int next, List<int> remainingIndices)
    {
        Vector2 a = vertices[prev];
        Vector2 b = vertices[curr];
        Vector2 c = vertices[next];

        if (!IsConvex(a, b, c)) return false;

        foreach (int i in remainingIndices)
        {
            if (i == prev || i == curr || i == next) continue;
            if (PointInTriangle(vertices[i], a, b, c)) return false;
        }

        return true;
    }

    private bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return Vector2.SignedAngle(b - a, c - b) > 0;
    }

    private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        return s >= 0 && t >= 0 && (s + t) <= 1;
    }

 private void GenerateMesh()
{
    List<int> triangles = Triangulate(drawnPoints);

    if (triangles.Count == 0) return;

    List<Vector3> vertices = new List<Vector3>();
    foreach (var point in drawnPoints)
    {
        vertices.Add(new Vector3(point.x, point.y, 0)); // Ensure vertices are valid and in 3D space
    }

    // Check if any triangle indices are out of bounds
    for (int i = 0; i < triangles.Count; i++)
    {
        if (triangles[i] >= vertices.Count) 
        {
            Debug.LogWarning($"Triangle index {triangles[i]} is out of bounds! Capping the index.");
            triangles[i] = Mathf.Min(triangles[i], vertices.Count - 1); // Cap the index to the last valid vertex
        }
    }

    Mesh mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    // Create a new GameObject to hold the mesh
    GameObject newMeshObject = new GameObject("GeneratedMesh");
    if (!newMeshObject)
    {
        Debug.LogError("No mesh generated");
        return;
    }

    // Add components for visual representation
    MeshFilter meshFilter = newMeshObject.AddComponent<MeshFilter>();
    MeshRenderer meshRenderer = newMeshObject.AddComponent<MeshRenderer>();
    
    // Assign the generated mesh for visual representation
    meshFilter.mesh = mesh;

    // Set material 
    meshRenderer.material = new Material(Shader.Find("Standard"));
    
    // Add a Rigidbody to make compound colliders work
    Rigidbody rb = newMeshObject.AddComponent<Rigidbody>();
    rb.isKinematic = true; // Make it kinematic so it doesn't fall with gravity
    
    // Generate compound colliders
    GenerateCompoundColliders(newMeshObject, vertices.ToArray(), triangles.ToArray());
}

private void GenerateCompoundColliders(GameObject parent, Vector3[] vertices, int[] triangles)
{
    int maxTrianglesPerCollider = 1; // Adjust this number based on your needs
    
    // Group triangles into chunks for separate colliders
    for (int startIdx = 0; startIdx < triangles.Length; startIdx += maxTrianglesPerCollider * 3)
    {
        // Calculate how many triangles will be in this chunk
        int triangleCount = Mathf.Min(maxTrianglesPerCollider, (triangles.Length - startIdx) / 3);
        if (triangleCount <= 0) continue;
        
        // Create a new sub-mesh for this chunk
        Mesh subMesh = new Mesh();
        
        // Create a mapping to reindex vertices
        Dictionary<int, int> vertexMapping = new Dictionary<int, int>();
        List<Vector3> subVertices = new List<Vector3>();
        List<int> subTriangles = new List<int>();
        
        // Process each triangle in this chunk
        for (int i = 0; i < triangleCount * 3; i += 3)
        {
            int baseIdx = startIdx + i;
            if (baseIdx + 2 >= triangles.Length) break;
            
            // Get the three vertex indices for this triangle
            int idx1 = triangles[baseIdx];
            int idx2 = triangles[baseIdx + 1];
            int idx3 = triangles[baseIdx + 2];
            
            // Map original vertex indices to new indices in the sub-mesh
            int newIdx1 = MapVertexIndex(vertexMapping, subVertices, vertices, idx1);
            int newIdx2 = MapVertexIndex(vertexMapping, subVertices, vertices, idx2);
            int newIdx3 = MapVertexIndex(vertexMapping, subVertices, vertices, idx3);
            
            // Add the triangle with the new indices
            subTriangles.Add(newIdx1);
            subTriangles.Add(newIdx2);
            subTriangles.Add(newIdx3);
        }
        
        // Skip if we didn't collect any triangles
        if (subTriangles.Count == 0 || subVertices.Count == 0) continue;
        
        // Create the sub-mesh
        subMesh.vertices = subVertices.ToArray();
        subMesh.triangles = subTriangles.ToArray();
        subMesh.RecalculateNormals();
        
        // Create a child GameObject for this collider
        GameObject colliderObj = new GameObject($"Collider_{startIdx/3}");
        colliderObj.transform.SetParent(parent.transform, false);
        
        // Add a mesh collider
        MeshCollider meshCollider = colliderObj.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = subMesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
    }
}

private int MapVertexIndex(Dictionary<int, int> mapping, List<Vector3> newVertices, Vector3[] originalVertices, int originalIndex)
{
    // If we've already mapped this vertex, return its new index
    if (mapping.TryGetValue(originalIndex, out int newIndex))
    {
        return newIndex;
    }
    
    // Otherwise, add it to our new vertex list and create a mapping
    newIndex = newVertices.Count;
    newVertices.Add(originalVertices[originalIndex]);
    mapping[originalIndex] = newIndex;
    
    return newIndex;
}
 
    private void ConfirmSelection()
    {
        
    }
    
}
