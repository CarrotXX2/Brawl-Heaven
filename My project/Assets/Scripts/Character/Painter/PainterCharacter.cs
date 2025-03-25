using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PainterCharacter : PlayerController
{
    [Header("Ultimate Stats")] [SerializeField]
    private float ultimateDuration = 5f;

    [Header("Ultimate Properties")] [SerializeField]
    private DrawingProperties[] drawingProperties;

    private int lastDirectionIndex;
    [SerializeField] private int propertyIndex;
    
    [Header("SmokeScreen Data")]
    [SerializeField] private ParticleSystem smokeScreenParticle;

    [Header("Drawing Logic")] private List<GameObject> generatedObjects = new List<GameObject>();

    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject cursorInstance;

    [SerializeField] private float cursorSpeed;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private float minPointDistance;
    [SerializeField] private float closeLoopThreshold;

    private Coroutine ultimateCast;
    private Vector3 cursorPosition;
    private Vector2 cursorMoveInput;

    private bool startDrawing;

    private List<Vector3> drawnPoints = new List<Vector3>();

    [Space] private bool usingUltimate = false;

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

    public override void OnUltimateCast(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        // If we're already using the ultimate and drawing, end the ultimate
        if (usingUltimate && startDrawing)
        {
            StopCoroutine(ultimateCast);
            UltimateEnd();
            return;
        }

        // If we're in ultimate mode but not drawing yet, start drawing
        if (usingUltimate && !startDrawing)
        {
            startDrawing = true;
            ultimateCast = StartCoroutine(UltimateCoroutine());
            return;
        }

        // If we're not in ultimate mode yet, start ultimate mode
        drawnPoints.Clear();
        rb.isKinematic = true; // Player can't move or fall while drawing

        Debug.Log("Cast Ultimate");

        // Instantiate cursor at player position (including Z coordinate)
        if (!cursorInstance)
        {
            cursorPosition = playerTransform.position;
            cursorInstance = Instantiate(cursor, playerTransform.position, Quaternion.identity);
        }

        usingUltimate = true;
    }

    private IEnumerator UltimateCoroutine()
    {
        // Start drawing immediately
        startDrawing = true;

        yield return new WaitForSeconds(ultimateDuration);

        UltimateEnd();
    }

    private void UltimateEnd()
    {
        // Check if shape is closed and has enough points
        if (drawnPoints.Count >= 3 && Vector3.Distance(drawnPoints[0], drawnPoints[drawnPoints.Count - 1]) < closeLoopThreshold)
        {
            // Close the loop by adding the first point again
            drawnPoints.Add(drawnPoints[0]);

            // Generate the mesh with proper triangulation
            GenerateMesh();
        }
        else
        {
            Debug.LogWarning("Shape is not closed or has too few points");
        }

        Destroy(cursorInstance);
        cursorInstance = null;
        rb.isKinematic = false;
        usingUltimate = false;
        startDrawing = false;
    }

    private void MoveUltimateCursor()
    {
        cursorMoveInput = moveInput;

        // Update X and Y position, but preserve Z position
        cursorPosition.x += cursorMoveInput.x * cursorSpeed * Time.deltaTime;
        cursorPosition.y += cursorMoveInput.y * cursorSpeed * Time.deltaTime;

        // Update cursor position
        if (cursorInstance != null)
        {
            cursorInstance.transform.position = cursorPosition;
        }
    }

    #region Property Logic

    public override void OnRightAnalogStickMove(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Vector2 pointerDirection = ctx.ReadValue<Vector2>();

        // Only update the direction if the stick is pushed beyond the deadzone
        if (pointerDirection.magnitude > deadZoneThreshold)
        {
            propertyIndex = GetDirectionIndex(pointerDirection);
            lastDirectionIndex = propertyIndex;

            // Handle the new direction (e.g., change a property based on direction)
            HandleDirectionChange(lastDirectionIndex);
        }

        // Note: We don't update lastDirectionIndex when within deadzone,
        // so the previous selection remains active
    }

    private int GetDirectionIndex(Vector2 direction)
    {
        // Normalize the direction to get consistent results regardless of stick pressure
        direction = direction.normalized;

        // Calculate the angle in degrees (0-360)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // Map the angle to cardinal directions (0-3)
        // 0 = Right (0°), 1 = Up (90°), 2 = Left (180°), 3 = Down (270°)
        int index = Mathf.RoundToInt(angle / 90f) % 4;

        return index;
    }

    private void HandleDirectionChange(int directionIndex)
    {
        // Implement your logic for each direction here
        switch (directionIndex)
        {
            case 0: // Right
                Debug.Log("Right property selected");
                // Set your property for Right
                break;
            case 1: // Up
                Debug.Log("Up property selected");
                // Set your property for Up
                break;
            case 2: // Left
                Debug.Log("Left property selected");
                // Set your property for Left
                break;
            case 3: // Down
                Debug.Log("Down property selected");
                // Set your property for Down
                break;
        }
    }

    #endregion

    #region Drawing Logic

    private void DrawShape()
    {
        // Add point if it's the first point or if it's far enough from the last point
        if (drawnPoints.Count == 0 ||
            Vector3.Distance(cursorPosition, drawnPoints[drawnPoints.Count - 1]) > minPointDistance)
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

        // Create gameObjects to hold the mesh data 
        // DrawingBackground is for generating an outline of the mesh 
        GameObject drawingObject = new GameObject("DrawnShape");
        GameObject drawingBackground = new GameObject("Drawing Background");

        drawingBackground.transform.localScale *= drawingProperties[propertyIndex].outlineWidth;

        generatedObjects.Add(drawingObject);
        generatedObjects.Add(drawingBackground);

        // Convert to 2D points for triangulation (assume all points are on the same Z plane)
        Vector2[] points2D = drawnPoints.Select(p => new Vector2(p.x, p.y)).ToArray();

        // Use the triangulator to generate triangles - always use clockwise winding
        Triangulator triangulator = new Triangulator(points2D);
        int[] triangles = triangulator.Triangulate(true); // Force clockwise winding

        // Convert back to 3D vertices for the mesh
        Vector3[] vertices = new Vector3[points2D.Length];
        for (int i = 0; i < points2D.Length; i++)
        {
            // Preserve the original Z position from drawn points
            vertices[i] = new Vector3(points2D[i].x, points2D[i].y, drawnPoints[i].z);
        }

        // Calculate the center of the vertices
        Vector3 center = Vector3.zero;
        for (int i = 0; i < vertices.Length; i++)
        {
            center += vertices[i];
        }

        center /= vertices.Length;

        // Offset all vertices by the center position to center the pivot
        Vector3[] centeredVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            centeredVertices[i] = vertices[i] - center;
        }

        // Create the mesh with centered vertices
        Mesh mesh = new Mesh();
        mesh.vertices = centeredVertices;
        mesh.triangles = triangles;

        // Generate proper UVs
        GenerateUVs(mesh);

        // Force normals to be consistent (all facing the same direction)
        Vector3[] normals = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.forward; // Use Vector3.back if your camera looks along positive Z
        }

        mesh.normals = normals;

        // Don't recalculate normals since we've set them manually
        mesh.RecalculateBounds();

        // Position the game objects at the center position
        drawingObject.transform.position = center;
        drawingBackground.transform.position = center;

        // Move background a little bit to prevent mesh flickering
        drawingBackground.transform.position = new Vector3(
            drawingBackground.transform.position.x,
            drawingBackground.transform.position.y,
            drawingBackground.transform.position.z + 0.1f);

        // Add mesh components
        MeshFilter meshFilter = drawingObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = drawingObject.AddComponent<MeshRenderer>();

        MeshFilter meshFilter2 = drawingBackground.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer2 = drawingBackground.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = mesh;
        meshFilter2.mesh = mesh;
        
        // Add SmokeScreen particles system to gameObject
        
        
        // Add collider 
        PolygonCollider2D polygonCollider = drawingObject.AddComponent<PolygonCollider2D>();

        // Create centered collider points
        Vector2[] centeredColliderPoints = new Vector2[centeredVertices.Length];
        for (int i = 0; i < centeredVertices.Length; i++)
        {
            centeredColliderPoints[i] = new Vector2(centeredVertices[i].x, centeredVertices[i].y);
        }

        // Set the centered collider path
        polygonCollider.SetPath(0, centeredColliderPoints);

        // Add drawing property
        drawingObject.AddComponent<DrawnMesh>();
        drawingObject.GetComponent<DrawnMesh>().drawingProperties = drawingProperties[propertyIndex];
        drawingObject.GetComponent<DrawnMesh>().smokeParticleSystem = smokeScreenParticle;
        
        // Set material
        meshRenderer.material = drawingProperties[propertyIndex].mainMaterial;
        meshRenderer2.material = drawingProperties[propertyIndex].backGroundMaterial;

        Debug.Log($"Generated mesh with {vertices.Length} vertices and {triangles.Length / 3} triangles");
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

    #endregion

    #region Triangulations

    // Triangulator that can handle both clockwise and counter-clockwise winding
    public class Triangulator
    {
        private List<Vector2> m_points = new List<Vector2>();

        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public int[] Triangulate(bool clockwise)
        {
            List<int> indices = new List<int>();

            // Ensure we have enough points
            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            // Create a list of indices
            List<int> remainingIndices = Enumerable.Range(0, n).ToList();

            // Check the current winding order
            float area = CalculateArea();
            bool isClockwise = area < 0;

            // If the requested winding order is different from the current one, reverse the points
            if (clockwise != isClockwise)
            {
                m_points.Reverse();
                remainingIndices.Reverse();
            }

            // Triangulate the polygon using ear clipping algorithm
            while (remainingIndices.Count > 3)
            {
                bool earFound = false;

                for (int i = 0; i < remainingIndices.Count; i++)
                {
                    int prev = (i - 1 + remainingIndices.Count) % remainingIndices.Count;
                    int curr = i;
                    int next = (i + 1) % remainingIndices.Count;

                    int prevIndex = remainingIndices[prev];
                    int currIndex = remainingIndices[curr];
                    int nextIndex = remainingIndices[next];

                    Vector2 v1 = m_points[prevIndex];
                    Vector2 v2 = m_points[currIndex];
                    Vector2 v3 = m_points[nextIndex];

                    // Check if this vertex is an ear
                    if (IsEar(prevIndex, currIndex, nextIndex, remainingIndices))
                    {
                        // Add the ear triangle with correct winding
                        indices.Add(prevIndex);
                        indices.Add(currIndex);
                        indices.Add(nextIndex);

                        // Remove the ear tip
                        remainingIndices.RemoveAt(curr);
                        earFound = true;
                        break;
                    }
                }

                // If no ear is found, use a simple fan triangulation as fallback
                if (!earFound)
                {
                    Debug.LogWarning("No ear found. Using simple fan triangulation.");
                    indices.Clear();

                    // Simple fan triangulation from the first vertex
                    for (int i = 1; i < n - 1; i++)
                    {
                        indices.Add(0);
                        indices.Add(i);
                        indices.Add(i + 1);
                    }

                    break;
                }
            }

            // Add the final triangle
            if (remainingIndices.Count == 3)
            {
                indices.Add(remainingIndices[0]);
                indices.Add(remainingIndices[1]);
                indices.Add(remainingIndices[2]);
            }

            return indices.ToArray();
        }

        // The rest of your methods remain the same
        private float CalculateArea()
        {
            float area = 0f;
            for (int i = 0; i < m_points.Count; i++)
            {
                int j = (i + 1) % m_points.Count;
                area += m_points[i].x * m_points[j].y;
                area -= m_points[j].x * m_points[i].y;
            }

            return area / 2.0f;
        }

        private bool IsEar(int p1, int p2, int p3, List<int> indices)
        {
            Vector2 a = m_points[p1];
            Vector2 b = m_points[p2];
            Vector2 c = m_points[p3];

            // Check if the triangle is convex
            if (!IsConvex(a, b, c))
                return false;

            // Check if any other point is inside this triangle
            for (int i = 0; i < indices.Count; i++)
            {
                int otherIndex = indices[i];
                if (otherIndex == p1 || otherIndex == p2 || otherIndex == p3)
                    continue;

                Vector2 p = m_points[otherIndex];
                if (IsPointInTriangle(p, a, b, c))
                    return false;
            }

            return true;
        }

        private bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            return (CrossProduct(b - a, c - b) > 0);
        }

        private float CrossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            // Compute barycentric coordinates
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            // If all same sign, point is in triangle
            return !(hasNeg && hasPos);
        }

        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
    #endregion
}