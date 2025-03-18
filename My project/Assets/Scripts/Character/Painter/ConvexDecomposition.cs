using System.Collections.Generic;
using UnityEngine;

public class ConvexDecomposition
{
    // Main method to divide the shape into convex pieces
    public static List<List<Vector3>> DecomposeToConvexPolygons(List<Vector3> points)
    {
        // Convert to 2D for simplicity (since we're working in a plane)
        List<Vector2> points2D = new List<Vector2>();
        foreach (var point in points)
        {
            points2D.Add(new Vector2(point.x, point.y));
        }

        // Get convex partitions
        List<List<Vector2>> convexParts = ConvexPartition(points2D);

        // Convert back to 3D
        List<List<Vector3>> result = new List<List<Vector3>>();
        foreach (var part in convexParts)
        {
            List<Vector3> convexPart3D = new List<Vector3>();
            foreach (var point in part)
            {
                convexPart3D.Add(new Vector3(point.x, point.y, 0));
            }
            result.Add(convexPart3D);
        }

        return result;
    }

    // Implementation of convex partitioning using Hertel-Mehlhorn algorithm
    private static List<List<Vector2>> ConvexPartition(List<Vector2> points)
    {
        // Ensure we have a closed polygon
        if (points.Count < 3)
            return new List<List<Vector2>>();

        // Make sure the polygon is in counter-clockwise order
        if (!IsCounterClockwise(points))
            points.Reverse();

        // Start with the original polygon
        List<List<Vector2>> partitions = new List<List<Vector2>>();
        List<Vector2> currentPolygon = new List<Vector2>(points);

        // Find and remove reflex vertices until the polygon is convex
        while (true)
        {
            int reflexIndex = FindReflexVertex(currentPolygon);
            if (reflexIndex == -1)
            {
                // Current polygon is convex, add it to the result
                partitions.Add(new List<Vector2>(currentPolygon));
                break;
            }

            // Find best diagonal to split at this reflex vertex
            int bestVertex = FindBestDiagonalVertex(currentPolygon, reflexIndex);
            
            if (bestVertex == -1)
            {
                // Fallback: just use the next vertex (this shouldn't happen with valid polygons)
                bestVertex = (reflexIndex + 2) % currentPolygon.Count;
            }

            // Split the polygon
            List<Vector2> newPolygon = new List<Vector2>();
            int start = reflexIndex;
            int end = bestVertex;

            // Build the new polygon
            int current = start;
            do
            {
                newPolygon.Add(currentPolygon[current]);
                current = (current + 1) % currentPolygon.Count;
            }
            while (current != (end + 1) % currentPolygon.Count);

            // If the new polygon is convex, add it to partitions
            if (IsConvexPolygon(newPolygon))
            {
                partitions.Add(newPolygon);
            }
            else
            {
                // If not convex, recursively partition it
                List<List<Vector2>> subPartitions = ConvexPartition(newPolygon);
                partitions.AddRange(subPartitions);
            }

            // Modify the current polygon by removing vertices between start and end (exclusive)
            List<Vector2> updatedPolygon = new List<Vector2>();
            current = end;
            do
            {
                updatedPolygon.Add(currentPolygon[current]);
                current = (current + 1) % currentPolygon.Count;
            }
            while (current != start);
            updatedPolygon.Add(currentPolygon[start]);

            // If the updated polygon is convex, add it and we're done
            if (IsConvexPolygon(updatedPolygon))
            {
                partitions.Add(updatedPolygon);
                break;
            }
            
            // Otherwise, continue with this updated polygon
            currentPolygon = updatedPolygon;
        }

        return partitions;
    }

    // Find a reflex vertex in the polygon (where interior angle > 180 degrees)
    private static int FindReflexVertex(List<Vector2> polygon)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            int prev = (i - 1 + polygon.Count) % polygon.Count;
            int next = (i + 1) % polygon.Count;

            Vector2 v1 = polygon[prev] - polygon[i];
            Vector2 v2 = polygon[next] - polygon[i];

            // Check if this is a reflex vertex (interior angle > 180°)
            if (CrossProduct(v1, v2) < 0)
            {
                return i;
            }
        }
        return -1; // No reflex vertices found, polygon is convex
    }

    // Find the best vertex to create a diagonal from the reflex vertex
    private static int FindBestDiagonalVertex(List<Vector2> polygon, int reflexIndex)
    {
        Vector2 reflexVertex = polygon[reflexIndex];
        int bestVertex = -1;
        float bestScore = float.MaxValue;

        // Try each vertex as a potential diagonal endpoint
        for (int i = 0; i < polygon.Count; i++)
        {
            // Skip adjacent vertices and self
            if (i == reflexIndex || 
                i == (reflexIndex - 1 + polygon.Count) % polygon.Count || 
                i == (reflexIndex + 1) % polygon.Count)
                continue;

            // Check if diagonal is valid (doesn't intersect any edges)
            if (IsValidDiagonal(polygon, reflexIndex, i))
            {
                // Score based on distance (closer is better)
                float dist = Vector2.Distance(reflexVertex, polygon[i]);
                if (dist < bestScore)
                {
                    bestScore = dist;
                    bestVertex = i;
                }
            }
        }

        return bestVertex;
    }

    // Check if a diagonal between two vertices doesn't intersect any polygon edges
    private static bool IsValidDiagonal(List<Vector2> polygon, int from, int to)
    {
        Vector2 p1 = polygon[from];
        Vector2 p2 = polygon[to];

        // Check against all edges
        for (int i = 0; i < polygon.Count; i++)
        {
            int next = (i + 1) % polygon.Count;
            
            // Skip edges connected to the vertices we're checking
            if (i == from || next == from || i == to || next == to)
                continue;

            Vector2 q1 = polygon[i];
            Vector2 q2 = polygon[next];

            // Check if line segments intersect
            if (LineSegmentsIntersect(p1, p2, q1, q2))
                return false;
        }

        // Also check if the diagonal is inside the polygon
        Vector2 midpoint = (p1 + p2) * 0.5f;
        return IsPointInPolygon(polygon, midpoint);
    }

    // Check if a point is inside a polygon using the ray casting algorithm
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

    // Check if two line segments intersect
    private static bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float d1 = Direction(q1, q2, p1);
        float d2 = Direction(q1, q2, p2);
        float d3 = Direction(p1, p2, q1);
        float d4 = Direction(p1, p2, q2);

        // Check if the lines intersect
        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && 
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        // Check if a point lies on a line
        if (d1 == 0 && IsPointOnSegment(q1, q2, p1)) return true;
        if (d2 == 0 && IsPointOnSegment(q1, q2, p2)) return true;
        if (d3 == 0 && IsPointOnSegment(p1, p2, q1)) return true;
        if (d4 == 0 && IsPointOnSegment(p1, p2, q2)) return true;

        return false;
    }

    // Helper method for line segment intersection
    private static float Direction(Vector2 a, Vector2 b, Vector2 c)
    {
        return CrossProduct(c - a, b - a);
    }

    // Check if point c is on line segment ab
    private static bool IsPointOnSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return c.x <= Mathf.Max(a.x, b.x) && c.x >= Mathf.Min(a.x, b.x) &&
               c.y <= Mathf.Max(a.y, b.y) && c.y >= Mathf.Min(a.y, b.y);
    }

    // Calculate cross product of two 2D vectors
    private static float CrossProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    // Check if a polygon is in counter-clockwise order
    private static bool IsCounterClockwise(List<Vector2> polygon)
    {
        float sum = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count];
            sum += (next.x - current.x) * (next.y + current.y);
        }
        return sum < 0;
    }

    // Check if a polygon is convex
    private static bool IsConvexPolygon(List<Vector2> polygon)
    {
        if (polygon.Count < 3) return false;

        bool sign = false;
        bool signSet = false;

        for (int i = 0; i < polygon.Count; i++)
        {
            int prev = (i - 1 + polygon.Count) % polygon.Count;
            int next = (i + 1) % polygon.Count;

            Vector2 v1 = polygon[i] - polygon[prev];
            Vector2 v2 = polygon[next] - polygon[i];

            float cross = CrossProduct(v1, v2);

            // Set sign on first valid cross product
            if (!signSet)
            {
                if (cross != 0)
                {
                    sign = cross > 0;
                    signSet = true;
                }
            }
            else if ((cross > 0) != sign && cross != 0)
            {
                return false; // Found both positive and negative cross products
            }
        }

        return true;
    }

    // Triangulate a convex polygon
    public static List<int> TriangulateConvex(List<Vector3> vertices)
    {
        List<int> indices = new List<int>();
        
        // Simple fan triangulation - works for convex polygons
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            indices.Add(0);
            indices.Add(i);
            indices.Add(i + 1);
        }
        
        return indices;
    }
}