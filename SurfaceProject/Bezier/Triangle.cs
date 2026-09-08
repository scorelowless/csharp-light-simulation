using System;

namespace SurfaceProject.Bezier;

public class Triangle(Vertex a, Vertex b, Vertex c)
{
    public readonly Vertex A = a, B = b, C = c;
    public Vertex this[int index] => index switch
        {
            0 => A,
            1 => B,
            2 => C,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
}