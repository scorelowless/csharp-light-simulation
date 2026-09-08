using System.Numerics;
using SurfaceProject.DataStructures;

namespace SurfaceProject;

public static class Math3D
{
    public static Matrix3 RotationX(float angle)
    {
        float cos = float.Cos(angle);
        float sin = float.Sin(angle);

        // return 3x3 rotation matrix around X axis
        return new Matrix3(
            1, 0, 0,
            0, cos, -sin,
            0, sin, cos);
    }

    public static Matrix3 RotationZ(float angle)
    {
        float cos = float.Cos(angle);
        float sin = float.Sin(angle);

        return new Matrix3(
            cos, -sin, 0,
            sin, cos, 0,
            0, 0, 1);
    }

    public static Vector2 ProjectTo2D(Vector3 p)
    {
        return new Vector2(p.X, p.Y);
    }
}