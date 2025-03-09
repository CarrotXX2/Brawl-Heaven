using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [Header("")]
    private bool usingUltimate = false;
    
    protected override void Update()
    {
        if (!usingUltimate)
        {
            base.Update();
        }
        else
        {
            MoveCursor();
            
            if (startDrawing)
            {
                DrawShape();
            }
        }
    }
    
    public override void OnUltimateCast() // Draw a shape and give it a property
    {   
        rb.isKinematic = true; // Player cant move or fall while drawing 
        
        cursorInstance = Instantiate(cursor, transform);
        
        StartCoroutine(UltimateCoroutine());
    }
    private IEnumerator UltimateCoroutine()
    {
        usingUltimate = true;
        yield return new WaitForSeconds(5); // Wait for an animation to finish playing before player is allowed to draw 
        startDrawing = true;
        
        yield return new WaitForSeconds(ultimateDuration);
        
        rb.isKinematic = false;
        usingUltimate = false;
        startDrawing = false;
    }
    
    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        cursorMoveInput = context.ReadValue<Vector2>();
    }

    private void DrawShape()
    {
        if (drawnPoints.Count == 0 || Vector3.Distance(cursorPosition, drawnPoints[drawnPoints.Count - 1]) > 0.1f)
        {
            drawnPoints.Add(cursorPosition);
            
            if (Vector2.Distance(drawnPoints.First(), drawnPoints.Last()) < 0.2f && drawnPoints.Count >= 3)
            {
                drawnPoints.Add(drawnPoints.First()); // Close the loop
                
               GenerateMesh();
            }
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
                    indices.Add(curr);
                    indices.Add(next);
                    
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

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[drawnPoints.Count];
        
        for (int i = 0; i < drawnPoints.Count; i++) vertices[i] = new Vector3(drawnPoints[i].x, drawnPoints[i].y, 0);

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Create a GameObject 
        GameObject newMeshObject = new GameObject("GeneratedMesh"); 

        // Add a MeshFilter and MeshRenderer
        MeshFilter meshFilter = newMeshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newMeshObject.AddComponent<MeshRenderer>();

        // Assign the generated mesh
        meshFilter.mesh = mesh;

        // Set material 
        meshRenderer.material = new Material(Shader.Find("Standard"));
        
        // Set Collider
        newMeshObject.AddComponent<MeshCollider>();
    }
    
    private void MoveCursor()
    {
        cursorPosition += cursorMoveInput * cursorSpeed * Time.deltaTime;

        // Clamp cursor position to stay within screen bounds
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);

        // Move the cursor in UI
        cursorInstance.GetComponent<Transform>().position = cursorPosition;
    }   

    private void ConfirmSelection()
    {
        
    }
    
}
