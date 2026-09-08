using System;
using System.Numerics;

namespace SurfaceProject.Bezier;

public class BezierSurface
{
    public Vector3[,] V { get; }

    private const int N = 3;
    private const int M = 3;
    private static float B(int i, int degree, float t) => (i, degree) switch
    {
        (0, 2) => (1 - t) * (1 - t),
        (1, 2) => 2 * (1 - t) * t,
        (2, 2) => t * t,
        (0, 3) => (1 - t) * (1 - t) * (1 - t),
        (1, 3) => 3 * (1 - t) * (1 - t) * t,
        (2, 3) => 3 * (1 - t) * t * t,
        (3, 3) => t * t * t,
        _ => throw new ArgumentOutOfRangeException()
    };

    public BezierSurface(Vector3[,] vertices)
    {
        V = vertices;
    }

    public Vector3 EvaluateP(float u, float v)
    {
        Vector3 val = Vector3.Zero;
        for (int i = 0; i <= N; ++i)
        {
            for (int j = 0; j <= M; ++j)
            {
                val += V[i, j] * B(i, N, u) * B(j, M, v);
            }
        }

        return val;
    }

    public Vector3 EvaluatePu(float u, float v)
    {
        Vector3 val = Vector3.Zero;
        for (int i = 0; i <= N - 1; ++i)
        {
            for (int j = 0; j <= M; ++j)
            {
                val += (V[i+1, j] - V[i, j]) * B(i, N - 1, u) * B(j, M, v);
            }
        }
        return N * val;
    }

    public Vector3 EvaluatePv(float u, float v)
    {
        Vector3 val = Vector3.Zero;
        for (int i = 0; i <= N; ++i)
        {
            for (int j = 0; j <= M - 1; ++j)
            {
                val += (V[i, j+1] - V[i, j]) * B(i, N, u) * B(j, M - 1, v);
            }
        }
        return M * val;
    }

    public static Vector3 EvaluateNormal(Vector3 pu, Vector3 pv)
    {
        // wymaga minusa, żeby normalne się zgadzały
        return -Vector3.Cross(pu, pv);
    }

}