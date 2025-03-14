using System.Collections.Generic;
using UnityEngine;

public class BayazitDecomposition
{
    /// <summary>
    /// Decomposes a concave polygon into a set of convex polygons using Mark Bayazit's algorithm.
    /// </summary>
    /// <param name="vertices">List of vertices defining the concave polygon (in counterclockwise order)</param>
    /// <returns>List of convex polygons (each a list of Vector2 points)</returns>
    public static List<List<Vector2>> DecomposePolygon(List<Vector2> vertices)
    {
        // Ensure we have at least 3 vertices to form a polygon
        if (vertices == null || vertices.Count < 3)
            return new List<List<Vector2>>();

        // Ensure the polygon is in counterclockwise order
        if (!IsCounterClockwise(vertices))
        {
            vertices = new List<Vector2>(vertices);
            vertices.Reverse();
        }

        return DecomposeRecursive(vertices);
    }

    /// <summary>
    /// Recursively decomposes a polygon into convex parts.
    /// </summary>
    private static List<List<Vector2>> DecomposeRecursive(List<Vector2> polygon)
    {
        List<List<Vector2>> result = new List<List<Vector2>>();

        // Check if the polygon is already convex
        if (IsPolygonConvex(polygon))
        {
            result.Add(polygon);
            return result;
        }

        // Find a reflex vertex
        int reflexIndex = -1;
        for (int i = 0; i < polygon.Count; i++)
        {
            if (IsReflex(polygon, i))
            {
                reflexIndex = i;
                break;
            }
        }

        // If no reflex vertices found, the polygon is convex
        if (reflexIndex == -1)
        {
            result.Add(polygon);
            return result;
        }

        // Find the best position to clip the polygon
        int leftVertexIndex, rightVertexIndex;
        Vector2 reflexVertex = polygon[reflexIndex];
        Vector2 leftVertex, rightVertex;
        
        // Get vertices before and after the reflex vertex
        int prevIndex = (reflexIndex == 0) ? polygon.Count - 1 : reflexIndex - 1;
        int nextIndex = (reflexIndex == polygon.Count - 1) ? 0 : reflexIndex + 1;
        
        // Get steiner point slightly inside the reflex vertex
        Vector2 reflexNormal = GetBisectorNormal(polygon, reflexIndex);
        Vector2 steinerPoint = reflexVertex + reflexNormal * 0.01f;

        // Try to find the best place to clip
        if (!FindBestClippingVertex(polygon, reflexIndex, steinerPoint, out leftVertexIndex, out rightVertexIndex))
        {
            // If no good clipping vertex found, use closest visible vertex as fallback
            FindClosestVisibleVertex(polygon, reflexIndex, out int closestIndex);
            
            // Create two new polygons by splitting at this diagonal
            SplitPolygon(polygon, reflexIndex, closestIndex, out List<Vector2> poly1, out List<Vector2> poly2);
            
            // Recursively decompose the two new polygons
            result.AddRange(DecomposeRecursive(poly1));
            result.AddRange(DecomposeRecursive(poly2));
            
            return result;
        }
        
        leftVertex = polygon[leftVertexIndex];
        rightVertex = polygon[rightVertexIndex];

        // Create two new polygons
        List<Vector2> firstPoly = new List<Vector2>();
        List<Vector2> secondPoly = new List<Vector2>();
        
        // Build first polygon
        firstPoly.Add(reflexVertex);
        int index = reflexIndex;
        
        do
        {
            index = (index + 1) % polygon.Count;
            firstPoly.Add(polygon[index]);
        }
        while (index != rightVertexIndex);
        
        if (leftVertexIndex != rightVertexIndex)
            firstPoly.Add(leftVertex);
            
        // Build second polygon
        secondPoly.Add(reflexVertex);
        index = reflexIndex;
        
        do
        {
            index = (index - 1 + polygon.Count) % polygon.Count;
            secondPoly.Add(polygon[index]);
        }
        while (index != leftVertexIndex);
        
        if (leftVertexIndex != rightVertexIndex)
            secondPoly.Add(rightVertex);

        // Recursively decompose the two new polygons
        result.AddRange(DecomposeRecursive(firstPoly));
        result.AddRange(DecomposeRecursive(secondPoly));
        
        return result;
    }

    /// <summary>
    /// Finds the best vertex to create a diagonal from a reflex vertex.
    /// </summary>
    private static bool FindBestClippingVertex(List<Vector2> polygon, int reflexIndex, Vector2 steinerPoint, out int leftIndex, out int rightIndex)
    {
        leftIndex = rightIndex = -1;
        
        // Variables to track the best vertices
        float minLeftAngle = float.MaxValue;
        float minRightAngle = float.MaxValue;
        
        Vector2 reflexVertex = polygon[reflexIndex];
        int prevIndex = (reflexIndex == 0) ? polygon.Count - 1 : reflexIndex - 1;
        int nextIndex = (reflexIndex == polygon.Count - 1) ? 0 : reflexIndex + 1;
        
        Vector2 prevVector = (polygon[prevIndex] - reflexVertex).normalized;
        Vector2 nextVector = (polygon[nextIndex] - reflexVertex).normalized;

        // Try each vertex as a potential diagonal endpoint
        for (int i = 0; i < polygon.Count; i++)
        {
            // Skip adjacent vertices and self
            if (i == reflexIndex || i == prevIndex || i == nextIndex)
                continue;
                
            Vector2 testVertex = polygon[i];
            Vector2 testVector = (testVertex - reflexVertex).normalized;
            
            // Check if this vertex is visible from the reflex vertex
            if (!IsVisible(polygon, reflexIndex, i))
                continue;
                
            // Calculate angle to determine if this vertex is on the "left" or "right" side
            float crossProductWithPrev = CrossProduct(prevVector, testVector);
            float crossProductWithNext = CrossProduct(testVector, nextVector);
            
            // Left side check (counterclockwise from prevVertex)
            if (crossProductWithPrev >= 0 && IsPointInCone(polygon, reflexIndex, testVertex))
            {
                float angle = Vector2.Angle(prevVector, testVector);
                if (angle < minLeftAngle)
                {
                    minLeftAngle = angle;
                    leftIndex = i;
                }
            }
            
            // Right side check (counterclockwise to nextVertex)
            if (crossProductWithNext >= 0 && IsPointInCone(polygon, reflexIndex, testVertex))
            {
                float angle = Vector2.Angle(testVector, nextVector);
                if (angle < minRightAngle)
                {
                    minRightAngle = angle;
                    rightIndex = i;
                }
            }
        }
        
        return (leftIndex != -1 && rightIndex != -1);
    }

    /// <summary>
    /// Checks if a point is within the cone formed at the reflex vertex.
    /// </summary>
    private static bool IsPointInCone(List<Vector2> polygon, int reflexIndex, Vector2 point)
    {
        int n = polygon.Count;
        int prevIndex = (reflexIndex == 0) ? n - 1 : reflexIndex - 1;
        int nextIndex = (reflexIndex == n - 1) ? 0 : reflexIndex + 1;
        
        Vector2 reflexVertex = polygon[reflexIndex];
        Vector2 prevVertex = polygon[prevIndex];
        Vector2 nextVertex = polygon[nextIndex];
        
        // For a reflex vertex, the valid cone is outside the polygon
        Vector2 toPrev = (prevVertex - reflexVertex).normalized;
        Vector2 toNext = (nextVertex - reflexVertex).normalized;
        Vector2 toPoint = (point - reflexVertex).normalized;
        
        // Check if point is in the correct direction
        float crossPrevToPoint = CrossProduct(toPrev, toPoint);
        float crossPointToNext = CrossProduct(toPoint, toNext);
        
        // For reflex vertices, we're looking outside the polygon
        return crossPrevToPoint <= 0 || crossPointToNext <= 0;
    }

    /// <summary>
    /// Finds the closest visible vertex from a reflex vertex.
    /// </summary>
    private static void FindClosestVisibleVertex(List<Vector2> polygon, int reflexIndex, out int closestIndex)
    {
        Vector2 reflexVertex = polygon[reflexIndex];
        float closestDistance = float.MaxValue;
        closestIndex = -1;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            // Skip adjacent vertices and self
            if (i == reflexIndex || 
                i == (reflexIndex - 1 + polygon.Count) % polygon.Count || 
                i == (reflexIndex + 1) % polygon.Count)
                continue;
                
            // Check if this vertex is visible
            if (IsVisible(polygon, reflexIndex, i))
            {
                float distance = Vector2.Distance(reflexVertex, polygon[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }
        }
        
        // If no visible vertex found, use next non-adjacent vertex as fallback
        if (closestIndex == -1)
        {
            closestIndex = (reflexIndex + 2) % polygon.Count;
        }
    }

    /// <summary>
    /// Splits a polygon along a diagonal.
    /// </summary>
    private static void SplitPolygon(List<Vector2> polygon, int indexA, int indexB, out List<Vector2> poly1, out List<Vector2> poly2)
    {
        poly1 = new List<Vector2>();
        poly2 = new List<Vector2>();
        
        // Ensure indexA comes before indexB when traversing counterclockwise
        if (indexA > indexB)
        {
            int temp = indexA;
            indexA = indexB;
            indexB = temp;
        }
        
        // Build first polygon from indexA to indexB
        for (int i = indexA; i <= indexB; i++)
        {
            poly1.Add(polygon[i]);
        }
        
        // Build second polygon from indexB to indexA (wrapping around)
        poly2.Add(polygon[indexB]);
        for (int i = indexB + 1; i < polygon.Count; i++)
        {
            poly2.Add(polygon[i]);
        }
        for (int i = 0; i <= indexA; i++)
        {
            poly2.Add(polygon[i]);
        }
    }

    /// <summary>
    /// Gets the normal vector at the bisector of the angle at a vertex.
    /// </summary>
    private static Vector2 GetBisectorNormal(List<Vector2> polygon, int vertexIndex)
    {
        int prevIndex = (vertexIndex == 0) ? polygon.Count - 1 : vertexIndex - 1;
        int nextIndex = (vertexIndex == polygon.Count - 1) ? 0 : vertexIndex + 1;
        
        Vector2 vertex = polygon[vertexIndex];
        Vector2 prev = polygon[prevIndex];
        Vector2 next = polygon[nextIndex];
        
        Vector2 toPrev = (prev - vertex).normalized;
        Vector2 toNext = (next - vertex).normalized;
        
        // Bisector is the average of the two vectors
        Vector2 bisector = (toPrev + toNext).normalized;
        
        // If reflex, the normal should point inside
        if (IsReflex(polygon, vertexIndex))
        {
            return new Vector2(-bisector.y, bisector.x);
        }
        else
        {
            return new Vector2(bisector.y, -bisector.x);
        }
    }

    /// <summary>
    /// Checks if a diagonal between two vertices is visible (doesn't intersect any edges).
    /// </summary>
    private static bool IsVisible(List<Vector2> polygon, int fromIndex, int toIndex)
    {
        Vector2 from = polygon[fromIndex];
        Vector2 to = polygon[toIndex];
        
        // Check against all edges
        for (int i = 0; i < polygon.Count; i++)
        {
            int nextI = (i + 1) % polygon.Count;
            
            // Skip edges connected to the vertices we're checking
            if ((i == fromIndex && nextI == toIndex) || (i == toIndex && nextI == fromIndex))
                continue;
                
            // Skip edges that share a vertex with the diagonal
            if (i == fromIndex || i == toIndex || nextI == fromIndex || nextI == toIndex)
                continue;
                
            // Check if the diagonal intersects this edge
            if (LineSegmentIntersection(from, to, polygon[i], polygon[nextI]))
                return false;
        }
        
        // For non-simple polygons, also check if the diagonal is outside the polygon
        Vector2 midpoint = (from + to) * 0.5f;
        return IsPointInPolygon(polygon, midpoint);
    }

    /// <summary>
    /// Checks if a vertex is reflex (interior angle > 180 degrees).
    /// </summary>
    private static bool IsReflex(List<Vector2> polygon, int vertexIndex)
    {
        int prevIndex = (vertexIndex == 0) ? polygon.Count - 1 : vertexIndex - 1;
        int nextIndex = (vertexIndex == polygon.Count - 1) ? 0 : vertexIndex + 1;
        
        Vector2 vertex = polygon[vertexIndex];
        Vector2 prev = polygon[prevIndex];
        Vector2 next = polygon[nextIndex];
        
        // In counterclockwise order, reflex vertices have negative cross product
        return CrossProduct(prev - vertex, next - vertex) < 0;
    }

    /// <summary>
    /// Checks if two line segments intersect.
    /// </summary>
    private static bool LineSegmentIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float denominator = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);
        
        // Lines are parallel or coincident
        if (Mathf.Approximately(denominator, 0))
            return false;
            
        float ua = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / denominator;
        float ub = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / denominator;
        
        // Check if intersection is within both line segments
        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }

    /// <summary>
    /// Checks if a point is inside a polygon using the ray casting algorithm.
    /// </summary>
    private static bool IsPointInPolygon(List<Vector2> polygon, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    /// <summary>
    /// Checks if a polygon is in counterclockwise order.
    /// </summary>
    private static bool IsCounterClockwise(List<Vector2> polygon)
    {
        float area = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            int j = (i + 1) % polygon.Count;
            area += polygon[i].x * polygon[j].y;
            area -= polygon[i].y * polygon[j].x;
        }
        return area < 0;
    }

    /// <summary>
    /// Checks if a polygon is convex.
    /// </summary>
    private static bool IsPolygonConvex(List<Vector2> polygon)
    {
        if (polygon.Count < 3)
            return false;
            
        bool sign = false;
        bool signSet = false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            int prevIndex = (i == 0) ? polygon.Count - 1 : i - 1;
            int nextIndex = (i + 1) % polygon.Count;
            
            Vector2 prev = polygon[prevIndex];
            Vector2 current = polygon[i];
            Vector2 next = polygon[nextIndex];
            
            float cross = CrossProduct(prev - current, next - current);
            
            if (!signSet)
            {
                if (!Mathf.Approximately(cross, 0))
                {
                    sign = cross > 0;
                    signSet = true;
                }
            }
            else if ((cross > 0) != sign && !Mathf.Approximately(cross, 0))
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Calculates the 2D cross product of two vectors.
    /// </summary>
    private static float CrossProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    /// <summary>
    /// Triangulates a convex polygon.
    /// </summary>
    public static List<int> TriangulateConvex(List<Vector2> vertices)
    {
        List<int> indices = new List<int>();
        
        // Fan triangulation works for convex polygons
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            indices.Add(0);
            indices.Add(i);
            indices.Add(i + 1);
        }
        
        return indices;
    }

    /// <summary>
    /// Convert 2D decomposition results to 3D for Unity mesh generation.
    /// </summary>
    public static List<List<Vector3>> ConvertTo3D(List<List<Vector2>> convexParts, float zValue = 0)
    {
        List<List<Vector3>> result = new List<List<Vector3>>();
        
        foreach (var part in convexParts)
        {
            List<Vector3> convexPart3D = new List<Vector3>();
            foreach (var point in part)
            {
                convexPart3D.Add(new Vector3(point.x, point.y, zValue));
            }
            result.Add(convexPart3D);
        }
        
        return result;
    }
}