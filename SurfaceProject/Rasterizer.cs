using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Media;
using SurfaceProject.Bezier;
using SurfaceProject.DataStructures;

namespace SurfaceProject;

public class Rasterizer
{
    private readonly Lighting _lighting;
    public float Alpha;
    public float Beta;
    private readonly PixelList _points;
    private float[,] _zBuffor;

    public Rasterizer(Lighting lighting)
    {
        _lighting = lighting;
        _points = new PixelList(1000000);
        _zBuffor = new float[800, 800];
        ResetZBuffor();
    }
    
    public void ResetZBuffor()
    {
        for (int i = 0; i < 800; i++)
        {
            for (int j = 0; j < 800; j++)
            {
                _zBuffor[i, j] = float.NegativeInfinity;
            }
        }
    }
    
    public class ThreadState
    {
        public readonly Vertex TempVertex = new();
        public readonly FloatColor Color = new();
        public readonly FloatColor TempColor = new();
        public FloatColor ResultColor = new();
        public readonly Matrix3 TempMatrix = Matrix3.Identity;
    }
    public void RasterizeMesh(Triangulation tr, BitMap bmp, bool isConst)
    {
        foreach (Triangle t in tr.Triangles)
        {
            VertexSortFill(t);
        }
        Parallel.ForEach(_points,
            () => new ThreadState(),
            (point, _, local) => {
            (int x, int y, Triangle t) = point;
            
            local.TempVertex.FromBarycentric(x, y, t);
            float z = local.TempVertex.PRot.Z;
            // teoretycznie może dojść do wyścigu na _zBuffor i w tablicy będzie niepoprawna wartość
            // można temu zapobiec nie robiąc tego równolegle, co spowolni działanie
            // lub stosując blokady, co też spowolni działanie
            // w praktyce nie zauważyłem większych problemów z tym związanych, czasem zdarzają się małe artefakty
            if (z <= _zBuffor[x + 400, y + 400]) return local;
            _lighting.ComputeColor(local.TempVertex, local, isConst);
            _zBuffor[x + 400, y + 400] = z;
            bmp.SetPixel(x, y, local.ResultColor.Color);
            return local;
        }, _ => { });
        
        _points.Reset();
    }
    
    private struct Edge(int yMax, float x, float invSlope)
    {
        public readonly int YMax = yMax;
        public float X = x;

        public void Update()
        {
            X += invSlope;
        }
    }
    private void VertexSortFill(Triangle t)
    {
        int[] ind = [0, 1, 2];
        Array.Sort(ind, (i1, i2) => t[i1].PRot.Y.CompareTo(t[i2].PRot.Y));
        int[] ys = [ (int)t[0].PRot.Y, (int)t[1].PRot.Y, (int)t[2].PRot.Y ];
        
        List<Edge> aet = [];
        int k = 0;
        for (int y = ys[ind[0]]; y <= ys[ind[2]]; ++y) // od ymin do ymax
        {
            // czy zaszły zmiany w AET
            while (k < 3 && ys[ind[k]] == y - 1)
            {
                // wierzchołek po ind[k] to (ind[k]+1)%3, a wierzchołek przed ind[k] to (ind[k]+2)%3
                // dziwnie wygląda pętla na dwie iteracje, ale ładniej niż pisać dwa razy to samo
                // oprócz tego, jak chcę ją zamienić na dwa osobne przypadki to program przestaje działać
                for (int i = 1; i <= 2; ++i)
                {
                    int next = (ind[k] + i) % 3;
                    if (ys[next] >= y - 1)
                    {
                        // dodajemy krawędź do AET
                        if(ys[next] == ys[ind[k]]) continue; // pozioma krawędź
                        float invSlope = (t[next].PRot.X - t[ind[k]].PRot.X) /
                                         (t[next].PRot.Y - t[ind[k]].PRot.Y);
                        Edge edge = new Edge(ys[next], t[ind[k]].PRot.X, invSlope);
                        aet.Add(edge);
                    }
                    else
                    {
                        // usuwamy krawędź z AET
                        for(int j = 0; j < aet.Count; ++j)
                        {
                            if (aet[j].YMax != y - 1) continue;
                            aet.RemoveAt(j);
                            break;
                        }
                    }
                }
                k++;
            }
            aet.Sort((e1, e2) => e1.X.CompareTo(e2.X));
            for (int i = 0; i < aet.Count; i += 2)
            {
                int xStart = (int)(aet[i].X);
                int xEnd = (int)(aet[i + 1].X);
                for (int x = xStart; x <= xEnd; ++x)
                {
                    _points.Add(x, y, t);
                }
            }
            // aktualizujemy x w AET
            for (int i = 0; i < aet.Count; ++i)
            {
                Edge e = aet[i];
                e.Update();
                aet[i] = e;
            }
        }
    }

    public static void RasterizeWireframe(Triangulation triangulation, BitMap bmp)
    {
        var edges = new HashSet<(Vertex, Vertex)>();
        foreach (Triangle t in triangulation.Triangles)
        {
            edges.Add((t.A, t.B));
            edges.Add((t.B, t.C));
            edges.Add((t.C, t.A));
        }
        foreach (var (v1, v2) in edges)
        {
            DrawLineProjected(Math3D.ProjectTo2D(v1.PRot), Math3D.ProjectTo2D(v2.PRot), bmp);
        }
    }

    private static void DrawLineProjected(Vector2 v1, Vector2 v2, BitMap bmp)
    {
        int x0 = (int)v1.X;
        int y0 = (int)v1.Y;
        int x1 = (int)v2.X;
        int y1 = (int)v2.Y;
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            bmp.SetPixel(x0, y0, Colors.Black);
            if (x0 == x1 && y0 == y1) break;
            int e2 = err << 1;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public void RasterizeEdges(BezierSurface surface, BitMap bmp, float alpha, float beta)
    {
        int uCount = surface.V.GetLength(0);
        int vCount = surface.V.GetLength(1);
        Vector2[,] rotV = new Vector2[uCount, vCount];
        Matrix3 rotationZ = Math3D.RotationZ(alpha);
        Matrix3 rotationX = Math3D.RotationX(beta);
        Matrix3 rotation = rotationX * rotationZ;
        for (int i = 0; i < uCount; i++)
        {
            for (int j = 0; j < vCount; j++)
            {
                Vector3 v = surface.V[i, j];
                v = rotation * v;
                rotV[i, j] = Math3D.ProjectTo2D(v);
            }
        }
        for (int i = 0; i < uCount; i++)
        {
            for (int j = 0; j < vCount; j++)
            {
                if(i < uCount - 1) DrawLineProjected(rotV[i, j], rotV[i + 1, j], bmp);
                if (j < vCount - 1) DrawLineProjected(rotV[i, j], rotV[i, j+1], bmp);
                DrawPointProjected(rotV[i, j], bmp, Colors.Black);
            }
        }
    }

    private static void DrawPointProjected(Vector2 v, BitMap bmp, Color c)
    {
        int x = (int)v.X;
        int y = (int)v.Y;
        const int size = 5;
        for (int i = x - size; i <= x + size; i++)
        {
            for (int j = y - size; j <= y + size; j++)
            {
                bmp.SetPixel(i, j, c);
            }
        }
    }

    // public void DrawLightSource(Vector2 v, BitMap bmp)
    // {
    //     int x = (int)v.X;
    //     int y = (int)v.Y;
    //     const int size = 6;
    //     for (int i = x - size; i <= x + size; i++)
    //     {
    //         bmp.SetPixel(i, y - size, Colors.Black);
    //         bmp.SetPixel(i, y + size, Colors.Black);
    //     }
    //     for (int i = y - size; i <= y + size; i++)
    //     {
    //         bmp.SetPixel(x - size, i, Colors.Black);
    //         bmp.SetPixel(x + size, i, Colors.Black);
    //     }
    //     DrawPointProjected(v, bmp, _lighting.LightColor.Color);
    // }
}