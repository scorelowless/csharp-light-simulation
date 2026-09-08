using System.Collections.Generic;

namespace SurfaceProject.Bezier;

public class Triangulation
{
    public List<Triangle> Triangles { get; }
    public Vertex[,] Vertices { get; }
    
    public Triangulation(BezierSurface surface, int subdivisions)
    {
        Triangles = [];
        Vertices = new Vertex[subdivisions + 1, subdivisions + 1];
        // tworzymy wierzchołki
        for (int i = 0; i <= subdivisions; ++i)
        {
            for (int j = 0; j <= subdivisions; ++j)
            {
                float u = 1f * i / subdivisions;
                float v = 1f * j / subdivisions;
                Vertices[i, j] = new Vertex(u, v, surface);
            }
        }
        // tworzymy trójkąty
        for (int i = 0; i < subdivisions; ++i)
        {
            for (int j = 0; j < subdivisions; ++j)
            {
                Vertex a = Vertices[i, j];
                Vertex b = Vertices[i + 1, j];
                Vertex c = Vertices[i, j + 1];
                Vertex d = Vertices[i + 1, j + 1];
                Triangles.Add(new Triangle(a, b, d));
                Triangles.Add(new Triangle(a, c, d));
            }
        }
    }
}