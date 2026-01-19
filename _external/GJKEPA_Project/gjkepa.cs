//#define OpenTK //Define using OpenTK here

using System;
using System.Collections.Generic;
/*
#if  OpenTK
using OpenTK.Mathematics;
#else
using System.Numerics;
#endif
*/
using Microsoft.Xna.Framework;
using System.Linq;

/// <summary>
/// The GJK_EPA_BCP class implements the Gilbert-Johnson-Keerthi (GJK) algorithm and the Expanding Polytope Algorithm (EPA) for collision detection between convex polyhedra.
/// It provides methods to check for intersection between two polyhedra and to compute the contact point, penetration depth, and contact normal if an intersection occurs.
/// The class also includes support for debugging and visualization of the algorithms' processes.
/// The implementation of GJK was taken and adapted from the article https://winter.dev/articles/gjk-algorithm.
/// The implementation of EPA was taken and adapted from the article https://winter.dev/articles/epa-algorithm.
/// To determine the contact point, the barycentric coordinates of the projection of the origin were used.
/// </summary>
public class GJK_EPA_BCP
{
    static public int gjk_max = 32; //Maximum gjk iterations
    static public int epa_max = 32; //Maximum epa iterations

    private const float Epsilon = 1e-3f; // Ìàëîå çíà÷åíèå äëÿ ÷èñëåííîé ñòàáèëüíîñòè EPA

#if GJKEPA_DEBUG
    public delegate void DrawPointDelegate(Vector3 v, UInt32 color);
    public delegate void DrawVectorDelegate(Vector3 begin, Vector3 end, UInt32 color);
    public delegate void DrawLineDelegate(Vector3 begin, Vector3 end, UInt32 color);
    public delegate void LogDelegate(string s);

    //Debugging functions will run without checking for null!
    static public DrawPointDelegate DrawPoint = null;
    static public DrawVectorDelegate DrawVector = null;
    static public DrawLineDelegate DrawLine = null;
    static public LogDelegate Log = null;
#endif

    /// <summary>
    /// Determines if two convex polyhedra intersect using the Gilbert-Johnson-Keerthi (GJK) algorithm and,
    /// if they do, computes the contact point, penetration depth, and contact normal using the Expanding Polytope Algorithm (EPA).
    /// </summary>
    /// <param name="polyhedron1">An array of Vector3 representing the vertices of the first polyhedron.</param>
    /// <param name="polyhedron2">An array of Vector3 representing the vertices of the second polyhedron.</param>
    /// <param name="contactPoint">The point of contact between the two polyhedra if an intersection occurs.</param>
    /// <param name="penetrationDepth">The depth of penetration between the two polyhedra if an intersection occurs.</param>
    /// <param name="contactNormal">The normal of the contact point between the two polyhedra if an intersection occurs.</param>
    /// <returns>
    /// Returns true if the polyhedra intersect, otherwise false. If true, the contactPoint, penetrationDepth, and contactNormal out parameters are set.
    /// </returns>
    public static bool CheckIntersection(Vector3[] polyhedron1, Vector3[] polyhedron2, out Vector3 contactPoint, out float penetrationDepth, out Vector3 contactNormal)
    {
        contactPoint = new Vector3(0, 0, 0);
        penetrationDepth = 0;
        contactNormal = new Vector3(0, 0, 0);

        // GJK initialization
        Simplex simplex = new Simplex();
        Vector3 direction = Vector3.Subtract(polyhedron1[0], polyhedron2[0]);

        SupportPoint support = CalculateSupportPoint(polyhedron1, polyhedron2, direction);

        simplex.AddSupportPoint(support);

        direction = -support.Point;

        int gjk_cnt = 0;

        while (gjk_cnt < gjk_max)
        {
            support = CalculateSupportPoint(polyhedron1, polyhedron2, direction);

            if (Vector3.Dot(support.Point, direction) <= 0)
            {
                return false; // No intersection
            }

            simplex.AddSupportPoint(support);

            if (simplex.ContainsOrigin(ref direction))
            {
                // Intersection detected, proceed with EPA
#if GJKEPA_DEBUG
                //simplex.DebugDraw();
#endif

                int epa_cnt = EPA(polyhedron1, polyhedron2, simplex, out contactNormal, out penetrationDepth, out contactPoint);

#if GJKEPA_DEBUG
                DrawVector(contactPoint, contactNormal * penetrationDepth, 4278222848u); //Green out vector
                DrawPoint(contactPoint, 4294901760u); //Red point
                Log(" gjk=" + gjk_cnt.ToString() + "  , epa=" + epa_cnt.ToString());
#endif
                return true;
            }

            gjk_cnt++;
        }
#if GJKEPA_DEBUG
        Log(" gjk=" + gjk_cnt.ToString());
#endif
        return false;
    }

    public struct SupportPoint
    {
        public Vector3 Point; // Support point
        public int Index1;    // Index in the first polyhedron
        public int Index2;    // Index in the second polyhedron

        public SupportPoint(Vector3 point, int index1, int index2)
        {
            Point = point;
            Index1 = index1;
            Index2 = index2;
        }
    }

    private static SupportPoint CalculateSupportPoint(Vector3[] polyhedron1, Vector3[] polyhedron2, Vector3 direction)
    {
        int index1, index2;
        Vector3 support1 = GetSupportPoint(polyhedron1, direction, out index1);
        Vector3 support2 = GetSupportPoint(polyhedron2, -direction, out index2);

        return new SupportPoint(Vector3.Subtract(support1, support2), index1, index2);
    }

    private static Vector3 GetSupportPoint(Vector3[] polyhedron, Vector3 direction, out int index)
    {
        double maxDot = double.MinValue;
        Vector3 support = new Vector3(0, 0, 0);
        index = -1;

        for (int i = 0; i < polyhedron.Length; i++)
        {
            double dot = Vector3.Dot(polyhedron[i], direction);
            if (dot > maxDot)
            {
                maxDot = dot;
                support = polyhedron[i];
                index = i;
            }
        }

        return support;
    }

    private class Simplex
    {
        private List<SupportPoint> points;

        public List<SupportPoint> Vertices
        {
            get { return points; }
        }

        public Simplex()
        {
            points = new List<SupportPoint>();
        }

#if GJKEPA_DEBUG                
        public void DebugDraw()
        {
            UInt32 clr = 4278222848u; //Green color
            float mul = 1.05f;
            switch (points.Count)
            {
                case 1:
                    DrawPoint(points[0].Point, clr);
                    break;
                case 2:
                    DrawLine(points[0].Point * mul, points[1].Point * mul, clr);
                    break;
                case 3:
                    DrawLine(points[0].Point * mul, points[1].Point * mul, clr);
                    DrawLine(points[1].Point * mul, points[2].Point * mul, clr);
                    DrawLine(points[2].Point * mul, points[0].Point * mul, clr);
                    break;
                case 4:
                    DrawLine(points[0].Point * mul, points[1].Point * mul, clr);
                    DrawLine(points[0].Point * mul, points[2].Point * mul, clr);
                    DrawLine(points[0].Point * mul, points[3].Point * mul, clr);
                    DrawLine(points[1].Point * mul, points[3].Point * mul, clr);
                    DrawLine(points[1].Point * mul, points[3].Point * mul, clr);
                    DrawLine(points[2].Point * mul, points[3].Point * mul, clr);
                    DrawLine(points[1].Point * mul, points[2].Point * mul, clr);
                    break;
                default:
                    return;
            }
        }
#endif

        public void AddSupportPoint(SupportPoint supportPoint)
        {
            points.Insert(0, supportPoint);
        }

        public bool ContainsOrigin(ref Vector3 direction)
        {
            if (points.Count == 4)
            {
                return ContainsOriginTetrahedron(ref direction);
            }
            else if (points.Count == 3)
            {
                return ContainsOriginTriangle(ref direction);
            }
            else if (points.Count == 2)
            {
                return ContainsOriginLine(ref direction);
            }

            return false;
        }

        private bool ContainsOriginTetrahedron(ref Vector3 direction)
        {
            SupportPoint a = points[0];
            SupportPoint b = points[1];
            SupportPoint c = points[2];
            SupportPoint d = points[3];

            Vector3 ab = b.Point - a.Point;
            Vector3 ac = c.Point - a.Point;
            Vector3 ad = d.Point - a.Point;
            Vector3 ao = -a.Point;

            Vector3 abc = Vector3.Cross(ab, ac);
            Vector3 acd = Vector3.Cross(ac, ad);
            Vector3 adb = Vector3.Cross(ad, ab);

            if (Vector3.Dot(abc, ao) > 0)
            {
                points.Clear();
                points.Add(a);
                points.Add(b);
                points.Add(c);
                //return Triangle(points = { a, b, c }, direction);
                return ContainsOriginTriangle(ref direction);
            }

            if (Vector3.Dot(acd, ao) > 0)
            {
                points.Clear();
                points.Add(a);
                points.Add(c);
                points.Add(d);
                //return Triangle(points = { a, c, d }, direction);
                return ContainsOriginTriangle(ref direction);
            }

            if (Vector3.Dot(adb, ao) > 0)
            {
                points.Clear();
                points.Add(a);
                points.Add(d);
                points.Add(b);
                //return Triangle(points = { a, d, b }, direction);
                return ContainsOriginTriangle(ref direction);
            }

            return true;
        }

        private bool ContainsOriginTriangle(ref Vector3 direction)
        {
            //The order of the points is such that a is always the last point added.
            SupportPoint a = points[0];
            SupportPoint b = points[1];
            SupportPoint c = points[2];

            Vector3 ab = b.Point - a.Point;
            Vector3 ac = c.Point - a.Point;
            Vector3 ao = -a.Point;

            Vector3 abc = Vector3.Cross(ab, ac);

            if (Vector3.Dot(Vector3.Cross(abc, ac), ao) > 0)
            {
                if (Vector3.Dot(ac, ao) > 0)
                {
                    points.Clear();
                    points.Add(a);
                    points.Add(c);
                    //points = { a, c };
                    direction = Vector3.Cross(Vector3.Cross(ac, ao), ac);
                }
                else
                {
                    points.Clear();
                    points.Add(a);
                    points.Add(b);
                    //points = { a, b };
                    return ContainsOriginLine(ref direction);
                }
            }

            else
            {
                if (Vector3.Dot(Vector3.Cross(ab, abc), ao) > 0)
                {
                    points.Clear();
                    points.Add(a);
                    points.Add(b);
                    //points = { a, b }
                    return ContainsOriginLine(ref direction);
                }

                else
                {
                    if (Vector3.Dot(abc, ao) > 0)
                    {
                        direction = abc;
                    }

                    else
                    {
                        points.Clear();
                        points.Add(a);
                        points.Add(c);
                        points.Add(b);
                        //points = { a, c, b };
                        direction = -abc;
                    }
                }
            }

            return false;
        }

        private bool ContainsOriginLine(ref Vector3 direction)
        {
            // The order of points is such that 'a' is always the newly added point
            SupportPoint a = points[0];
            SupportPoint b = points[1];

            Vector3 ab = b.Point - a.Point;

            Vector3 ao = -a.Point;

            if (Vector3.Dot(ab, ao) > 0)
                // Construct a perpendicular to the line in the direction of the origin
                direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
            else
            {
                points.Clear();
                points.Add(a);

                direction = ao;
            }

            return false;
        }

    }

#if GJKEPA_DEBUG                
    private static void DrawPolytope(List<SupportPoint> poly, List<int> faces)
    {
        for (int i = 0; i < faces.Count; i += 3)
        {
            Vector3 a = poly[faces[i]].Point;
            Vector3 b = poly[faces[i + 1]].Point;
            Vector3 c = poly[faces[i + 2]].Point;

            UInt32 clr = 4294967040u; //Yellow
            DrawLine(a, b, clr);
            DrawLine(a, c, clr);
            DrawLine(c, b, clr);
        }
    }
#endif

    private static (List<Vector4>, int) GetFaceNormals(List<SupportPoint> polytope, List<int> faces)
    {
        List<Vector4> normals = new List<Vector4>();
        int minTriangle = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < faces.Count; i += 3)
        {
            Vector3 a = polytope[faces[i]].Point;
            Vector3 b = polytope[faces[i + 1]].Point;
            Vector3 c = polytope[faces[i + 2]].Point;

            Vector3 normal = Vector3.Cross(b - a, c - a);
            
            #if OpenTK
            float l = normal.Length; //For OpenTK.Mathematics
            #else
            float l = normal.Length(); //For System.Numeric
            #endif

            float distance = float.MaxValue;

            //If vectors is colliniar we have degenerate face
            if (l < Epsilon)
            {
                normal = Vector3.Zero;
                distance = float.MaxValue;
            }
            else
            {
                normal = normal / l;
                distance = Vector3.Dot(normal, a);
            }

            //If origin outside the polytope
            if (distance < 0)
            {
                normal *= -1;
                distance *= -1;
            }

            normals.Add(new Vector4(normal, distance));

            if (distance < minDistance)
            {
                minTriangle = i / 3;
                minDistance = distance;
            }
        }

        return (normals, minTriangle);
    }

    private static void AddIfUniqueEdge(ref List<(int, int)> edges, List<int> faces, int a, int b)
    {
        //      0--<--3
        //     / \ B /   A: 2-0
        //    / A \ /    B: 0-2
        //   1-->--2
        (int, int) reverse = (0, 0);
        bool found = false;
        foreach ((int, int) f in edges)
        {
            if ((f.Item1 == faces[b]) && (f.Item2 == faces[a]))
            {
                reverse = f;
                found = true;
                break;
            }
        }

        if (found)
        {
            edges.Remove(reverse);
        }
        else
        {
            edges.Add((faces[a], faces[b]));
        }
    }

    private static Vector3 V4to3(Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    private static int EPA(Vector3[] polyhedron1, Vector3[] polyhedron2, Simplex simplex, out Vector3 contactNormal, out float PenetrationDepth, out Vector3 contactPoint)
    {
        int epa_cnt = 0;

        List<SupportPoint> polytope = simplex.Vertices;

        //3 vertex indexes for each face
        List<int> faces = new List<int>{
            0, 1, 2,
            0, 3, 1,
            0, 2, 3,
            1, 3, 2
        };

        //we do not know the signs of the projections of the origin onto the normals, but they must be the same
        var (normals, minFace) = GetFaceNormals(polytope, faces);

        Vector3 minNormal = V4to3(normals[minFace]);
        float minDistance = float.MaxValue;

        while ((minDistance == float.MaxValue) && (epa_cnt < epa_max))
        {
            minNormal = V4to3(normals[minFace]);
            minDistance = normals[minFace].W;

            SupportPoint support = CalculateSupportPoint(polyhedron1, polyhedron2, minNormal);
            float sDistance = Vector3.Dot(minNormal, support.Point);

            if (Math.Abs(sDistance - minDistance) > Epsilon)
            {
                minDistance = float.MaxValue;

                List<(int, int)> uniqueEdges = new List<(int, int)>();

                for (int i = 0; i < normals.Count; i++)
                {
                    if ((Vector3.Dot(V4to3(normals[i]), support.Point) - normals[i].W) > 0)
                    {
                        int f = i * 3;
                        AddIfUniqueEdge(ref uniqueEdges, faces, f, f + 1);
                        AddIfUniqueEdge(ref uniqueEdges, faces, f + 1, f + 2);
                        AddIfUniqueEdge(ref uniqueEdges, faces, f + 2, f);
                        faces[f + 2] = faces.Last<int>(); faces.RemoveAt(faces.Count - 1);
                        faces[f + 1] = faces.Last<int>(); faces.RemoveAt(faces.Count - 1);
                        faces[f] = faces.Last<int>(); faces.RemoveAt(faces.Count - 1);

                        normals[i] = normals.Last<Vector4>(); normals.RemoveAt(normals.Count - 1);

                        i--;
                    }
                }

                //It is possible to encounter a situation where, due to invalid points(zeros, NaN), uniqueEdges does not contain any edges, which needs to be addressed.                

                List<int> newFaces = new List<int>();
                for (int i = 0; i < uniqueEdges.Count; i++)
                {
                    (int, int) f = uniqueEdges[i];
                    newFaces.Add(f.Item1);
                    newFaces.Add(f.Item2);
                    newFaces.Add(polytope.Count);

                    //The direction is maintained counterclockwise.
                }

                polytope.Add(support);

                var (newNormals, newMinFace) = GetFaceNormals(polytope, newFaces);

                float oldMinDistance = float.MaxValue;
                for (int i = 0; i < normals.Count; i++)
                {
                    if (normals[i].W < oldMinDistance)
                    {
                        oldMinDistance = normals[i].W;
                        minFace = i;
                    }
                }

                if (newNormals[newMinFace].W < oldMinDistance)
                {
                    minFace = newMinFace + normals.Count;
                }

                faces.AddRange(newFaces);
                normals.AddRange(newNormals);
            }
            epa_cnt++;
        }


#if GJKEPA_DEBUG
        /*
        DrawPolytope(polytope, faces);
        UInt32 clr = 4294901760u; // Red
        Vector3[] minTri = new Vector3[3];
        minTri[0] = polytope[faces[minFace * 3]].Point;
        minTri[1] = polytope[faces[minFace * 3 + 1]].Point;
        minTri[2] = polytope[faces[minFace * 3 + 2]].Point;
        DrawLine(minTri[0] * 1.1f, minTri[1] * 1.1f, clr);
        DrawLine(minTri[1] * 1.1f, minTri[2] * 1.1f, clr);
        DrawLine(minTri[0] * 1.1f, minTri[2] * 1.1f, clr);
        */
#endif

        SupportPoint a = polytope[faces[minFace * 3]];
        SupportPoint b = polytope[faces[minFace * 3 + 1]];
        SupportPoint c = polytope[faces[minFace * 3 + 2]];

        // Finding the projection of the origin onto the plane of the triangle
        float distance = Vector3.Dot(a.Point, minNormal);
        Vector3 projectedPoint = -distance * minNormal;

        // Getting the barycentric coordinates of this projection within the triangle belonging to the simplex
        (float u, float v, float w) = GetBarycentricCoordinates(projectedPoint, a.Point, b.Point, c.Point);

        // Taking the corresponding triangle from the first polyhedron
        Vector3 a1 = polyhedron1[a.Index1];
        Vector3 b1 = polyhedron1[b.Index1];
        Vector3 c1 = polyhedron1[c.Index1];

        // Contact point on the first polyhedron
        Vector3 contactPoint1 = u * a1 + v * b1 + w * c1;

        // Taking the corresponding triangle from the second polyhedron
        Vector3 a2 = polyhedron2[a.Index2];
        Vector3 b2 = polyhedron2[b.Index2];
        Vector3 c2 = polyhedron2[c.Index2];

        // Contact point on the second polyhedron
        Vector3 contactPoint2 = u * a2 + v * b2 + w * c2;

        contactPoint = (contactPoint1 + contactPoint2) / 2; // Returning the midpoint
        contactNormal = minNormal;
        PenetrationDepth = minDistance + 0.001f;

        return epa_cnt;
    }

    public static (float u, float v, float w) GetBarycentricCoordinates(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Vectors from vertex A to vertices B and C
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;

        // Compute dot products
        float d00 = Vector3.Dot(v0, v0); // Same as length squared V0
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1); // Same as length squared V1
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;

        float u = 0;
        float v = 0;
        float w = 0;

        // Check for a zero denominator before division = check for degenerate triangle (area is zero)
        if (Math.Abs(denom) <= Epsilon)
        {
            // The triangle is degenerate

            // Check if all vertices coincide (triangle collapses to a point)
            if (d00 <= Epsilon && d11 <= Epsilon)
            {
                // All edges are degenerate (vertices coincide at a point)
                // Return barycentric coordinates corresponding to vertex a
                u = 1;
                v = 0;
                w = 0;
            }
            else
            {
                // Seems triangle collapses to a line (vertices are colinear)
                // We can check it:
                // Vector3 cross = Vector3.Cross(v0, v1);
                // if (Vector3.Dot(cross, cross) <= Epsilon).... 
                // But if the triangle area is close to zero and the triangle has not colapsed to a point then it has colapsed to a line
                // Use edge AB if it's not degenerate
                if (d00 > Epsilon)
                {
                    // Compute parameter t for projection of point p onto line AB
                    float t = Vector3.Dot(v2, v0) / d00;
                    // if |t|>1 then p lies in AC but we can use u,v,w calculated to AB with a small error    
                    // Barycentric coordinates for edge AB
                    u = 1.0f - t;   // weight for vertex a
                    v = t;          // weight for vertex b
                    w = 0.0f;       // vertex c does not contribute
                }
                // Else, use edge AC 
                else if (d11 > Epsilon)
                {
                    // Compute parameter t for projection of point p onto line AC
                    float t = Vector3.Dot(v2, v1) / d11;
                    // Barycentric coordinates for edge AC
                    u = 1.0f - t; // weight for vertex a
                    v = 0.0f;     // vertex b does not contribute
                    w = t;        // weight for vertex c
                }
                else
                {
                    // The triangle is degenerate in an unexpected way
                    // Return barycentric coordinates corresponding to vertex a                    
                    u = 1;
                    v = 0;
                    w = 0;
                }
            }

        } else
        { 
            // Compute barycentric coordinates
            v = (d11 * d20 - d01 * d21) / denom;
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1.0f - v - w;
        }

        return (u, v, w);
    }

}
