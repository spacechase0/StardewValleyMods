# GJKEPA
Pure C# 3D implementation of GJK and EPA algoritms. Fast and stable, with a minimum of external links. 

The GJK_EPA_BСP class implements the Gilbert-Johnson-Keerthi (GJK) algorithm and the Expanding Polytope Algorithm (EPA) for collision detection between convex polyhedra.
It provides methods to check for intersection between two polyhedra and to compute the contact point, penetration depth, and contact normal if an intersection occurs.
The class also includes support for debugging and visualization of the algorithms' processes.
The implementation of GJK was taken and adapted from the article https://winter.dev/articles/gjk-algorithm.
The implementation of EPA was taken and adapted from the article https://winter.dev/articles/epa-algorithm.
To determine the contact point, the barycentric coordinates of the projection of the origin were used.

You can use it very simply! Just call the `CheckIntersection` function. It returns true if the polyhedra intersect, otherwise false. If true, the contactPoint, penetrationDepth, and contactNormal out parameters are set.

Parameters:
```
polyhedron1 - An array of Vector3 representing the vertices of the first polyhedron

polyhedron2 - An array of Vector3 representing the vertices of the second polyhedron

contactPoint - The point of contact between the two polyhedra if an intersection occurs

penetrationDepth - The depth of penetration between the two polyhedra if an intersection occurs

contactNormal - The normal of the contact point between the two polyhedra if an intersection occurs
```

If you need to understand how these algorithms work, you can define "GJKEPA_DEBUG" and define a function for logging, drawing points, vectors and lines.

You can use System.Numeric vectors or OpenTK.Mathematics vectors. For OpenTK use the "OpenTK" definition.

These algorithms are a good choice for physics solvers. It can be used for simulation or games.

![Example of using](https://github.com/exatb/GJKEPA/blob/main/screen.jpg)
