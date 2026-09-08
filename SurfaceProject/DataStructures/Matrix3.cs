using System.Numerics;

namespace SurfaceProject.DataStructures;

public class Matrix3(
    float m11, float m12, float m13,
    float m21, float m22, float m23,
    float m31, float m32, float m33)
{
    private float _m11 = m11, _m12 = m12, _m13 = m13;
    private float _m21 = m21, _m22 = m22, _m23 = m23;
    private float _m31 = m31, _m32 = m32, _m33 = m33;

    public void FromColumns(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        _m11 = v1.X; _m12 = v2.X; _m13 = v3.X;
        _m21 = v1.Y; _m22 = v2.Y; _m23 = v3.Y;
        _m31 = v1.Z; _m32 = v2.Z; _m33 = v3.Z;
    }

    private Vector3 Multiply(Vector3 v)
    {
        return new Vector3(
            _m11 * v.X + _m12 * v.Y + _m13 * v.Z,
            _m21 * v.X + _m22 * v.Y + _m23 * v.Z,
            _m31 * v.X + _m32 * v.Y + _m33 * v.Z
        );
    }

    public static Vector3 operator *(Matrix3 m, Vector3 v)
        => m.Multiply(v);

    private Matrix3 Multiply(Matrix3 o)
    {
        return new Matrix3(
            _m11 * o._m11 + _m12 * o._m21 + _m13 * o._m31,
            _m11 * o._m12 + _m12 * o._m22 + _m13 * o._m32,
            _m11 * o._m13 + _m12 * o._m23 + _m13 * o._m33,

            _m21 * o._m11 + _m22 * o._m21 + _m23 * o._m31,
            _m21 * o._m12 + _m22 * o._m22 + _m23 * o._m32,
            _m21 * o._m13 + _m22 * o._m23 + _m23 * o._m33,

            _m31 * o._m11 + _m32 * o._m21 + _m33 * o._m31,
            _m31 * o._m12 + _m32 * o._m22 + _m33 * o._m32,
            _m31 * o._m13 + _m32 * o._m23 + _m33 * o._m33
        );
    }

    public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        => a.Multiply(b);

    public static Matrix3 Identity => new(1, 0, 0,
                                          0, 1, 0,
                                          0, 0, 1);
}