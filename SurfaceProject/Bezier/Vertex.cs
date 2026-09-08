using System;
using System.Numerics;
using SurfaceProject.DataStructures;

namespace SurfaceProject.Bezier;

public class Vertex
{
   private Vector3 _p;
   private Vector3 _pu;
   private Vector3 _pv;
   private Vector3 _n;
   public Vector3 PRot;
   public Vector3 PuRot;
   public Vector3 PvRot;
   public Vector3 NRot;
   public float u, v;

   public Vertex(float u, float v, BezierSurface bs)
   {
      this.u = u;
      this.v = v;
      ApplyBezier(bs);
      PRot = Vector3.Zero;
      PuRot = Vector3.Zero;
      PvRot = Vector3.Zero;
      NRot = Vector3.Zero;
   }

   public Vertex()
   {
   }
   
   public void FromBarycentric(int x, int y, Triangle t)
   {
      Vector3 barycentric = ComputeBarycentric(x, y, t);
      u = barycentric.X * t.A.u +
          barycentric.Y * t.B.u +
          barycentric.Z * t.C.u;
      v = barycentric.X * t.A.v +
          barycentric.Y * t.B.v +
          barycentric.Z * t.C.v;
      // poniższe obliczenia nie są potrzebne, bo ta metoda jest używana tylko po obróceniu wierzchołków
      // _p = barycentric.X * t.A._p +
      //      barycentric.Y * t.B._p +
      //      barycentric.Z * t.C._p;
      // _pu = barycentric.X * t.A._pu +
      //       barycentric.Y * t.B._pu +
      //       barycentric.Z * t.C._pu;
      // _pv = barycentric.X * t.A._pv +
      //       barycentric.Y * t.B._pv +
      //       barycentric.Z * t.C._pv;
      // _n = barycentric.X * t.A._n +
      //      barycentric.Y * t.B._n +
      //      barycentric.Z * t.C._n;
      PRot = barycentric.X * t.A.PRot +
             barycentric.Y * t.B.PRot +
             barycentric.Z * t.C.PRot;
      PuRot = barycentric.X * t.A.PuRot +
              barycentric.Y * t.B.PuRot +
              barycentric.Z * t.C.PuRot;
      PvRot = barycentric.X * t.A.PvRot +
              barycentric.Y * t.B.PvRot +
              barycentric.Z * t.C.PvRot;
      NRot = barycentric.X * t.A.NRot +
             barycentric.Y * t.B.NRot +
             barycentric.Z * t.C.NRot;
   }

   private void ApplyBezier(BezierSurface bs)
   {
      // wartości nienormalizowane
      _p = bs.EvaluateP(u, v);
      _pu = bs.EvaluatePu(u, v);
      _pv = bs.EvaluatePv(u, v);
      _n = BezierSurface.EvaluateNormal(_pu, _pv);
   }

   public void ApplyRotation(Matrix3 rotation)
   {
      PRot = rotation * _p;
      PuRot = rotation * _pu;
      PvRot = rotation * _pv;
      NRot = rotation * _n;
   }
   
   private static Vector3 ComputeBarycentric(int x, int y, Triangle t)
   {
      (float cx, float cy) = (t.C.PRot.X, t.C.PRot.Y);
      (float ax, float ay) = (t.A.PRot.X - cx, t.A.PRot.Y - cy);
      (float bx, float by) = (t.B.PRot.X - cx, t.B.PRot.Y - cy);
      (float px, float py) = (x - cx, y - cy);
      float denominator = ax * by - ay * bx;
      const float eps = 1e-6f;
      if (MathF.Abs(denominator) < eps) // ABC są współliniowe
      {
         if (ax - bx < eps) return new Vector3(1, 0, 0);
         float q = (ax - bx) / (px - bx);
         return new Vector3(q, 1 - q, 0);
      }
      float l1 = (px * by - py * bx) / denominator;
      float l2 = (ax * py - ay * px) / denominator;
      return new Vector3(l1, l2, 1 - l1 - l2);
   }
}