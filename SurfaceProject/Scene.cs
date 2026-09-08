using System;
using System.IO;
using System.Numerics;
using Avalonia.Media.Imaging;
using SurfaceProject.Bezier;
using SurfaceProject.DataStructures;

namespace SurfaceProject;

public class Scene
{
    private readonly BezierSurface _surface1;
    private readonly BezierSurface _surface2;
    private Triangulation _triangulation1;
    private Triangulation _triangulation2;
    private readonly Rasterizer _rasterizer;
    public readonly Lighting Lighting;
    private readonly BitMap _bmp;
    private static readonly Vector3[,] SecondSurfaceControlPoints = new[,]
    {
        { new Vector3(-150, -100, -200), new Vector3(-50, -100, -200), new Vector3(50, -100, -200), new Vector3(150, -100, -200) },
        { new Vector3(-250, -100,  -50), new Vector3(-50, -100,  -50), new Vector3(50, -100,  -50), new Vector3(250, -100,  -50) },
        { new Vector3(-250,  -50,   50), new Vector3(-50,  -50,   50), new Vector3(50,  -50,   50), new Vector3(250,  -50,   50) },
        { new Vector3(-150,    0,  150), new Vector3(-50,    0,  150), new Vector3(50,    0,  150), new Vector3(150,    0,  150) }
    };

    public int Subdivisions;
    public bool Fill, Triangles, Edges;

    public Scene()
    {
        Vector3[,] controlPoints = ReadControlPoints();
        var objectTexture1 = ReadTexture();
        var normalMap1 = ReadNormal();
        _surface1 = new BezierSurface(controlPoints);
        _surface2 = new BezierSurface(SecondSurfaceControlPoints);
        Subdivisions = 30;
        Lighting = new Lighting(objectTexture1, normalMap1);
        _rasterizer = new Rasterizer(Lighting)
        {
            Alpha = float.DegreesToRadians(20),
            Beta = float.DegreesToRadians(50),
        };
        Fill = true;
        Triangles = false;
        Edges = false;
        _bmp = new BitMap(800, 800);
    }

    public WriteableBitmap Render()
    {
        _bmp.Reset();
        _rasterizer.ResetZBuffor();
        _triangulation1 = new Triangulation(_surface1, Subdivisions);
        _triangulation2 = new Triangulation(_surface2, Subdivisions);
        Rotate(_triangulation1, _triangulation2);
        // rasterizer.DrawLightSource(Math3D.ProjectTo2D(Lighting.LightPosition), _bmp);
        if (Fill)
        {
            _rasterizer.RasterizeMesh(_triangulation1, _bmp, false);
            _rasterizer.RasterizeMesh(_triangulation2, _bmp, true);
        }

        if (Triangles)
        {
            Rasterizer.RasterizeWireframe(_triangulation1, _bmp);
            Rasterizer.RasterizeWireframe(_triangulation2, _bmp);
        }

        if (Edges)
        {
            _rasterizer.RasterizeEdges(_surface1, _bmp, Alpha1, Beta1);
            _rasterizer.RasterizeEdges(_surface2, _bmp, Alpha2, Beta2);
        }
        
        return _bmp.GetBitmap();
    }

    public float Alpha1
    {
        get => _rasterizer.Alpha;
        set => _rasterizer.Alpha = value;
    }

    public float Beta1
    {
        get => _rasterizer.Beta;
        set => _rasterizer.Beta = value;
    }
    public float Alpha2;
    public float Beta2;
    private void Rotate(Triangulation tr1, Triangulation tr2)
    {
        Matrix3 rotationZ = Math3D.RotationZ(Alpha1);
        Matrix3 rotationX = Math3D.RotationX(Beta1);
        Matrix3 rotation1 = rotationX * rotationZ;
        rotationZ = Math3D.RotationZ(Alpha2);
        rotationX = Math3D.RotationX(Beta2);
        Matrix3 rotation2 = rotationX * rotationZ;
        foreach (Vertex vertex in tr1.Vertices)
        {
            vertex.ApplyRotation(rotation1);
        }
        foreach (Vertex vertex in tr2.Vertices)
        {
            vertex.ApplyRotation(rotation2);
        }
    }

    private static Vector3[,] ReadControlPoints()
    {
        // punkty kontrolne zapisane w pliku CONTROLPOINTS.txt
        // separatorem dziesiętnym jest kropka
        // projekt powinien być już skonfigurowany, żeby plik był kopiowany do katalogu wyjściowego
        const string path = "Assets/CONTROLPOINTS.txt";
        string[] lines = File.ReadAllLines(path);
        Vector3[,] controlPoints = new Vector3[4, 4];
        if(lines.Length != 16) throw new Exception("Plik CONTROLPOINTS.txt musi zawierać dokładnie 16 linii.");
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                string[] parts = lines[i * 4 + j].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                    throw new Exception("Każda linia w pliku CONTROLPOINTS.txt musi zawierać dokładnie 3 wartości.");
                float x = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                float z = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                controlPoints[i, j] = new Vector3(x, y, z);
            }
        }
        return controlPoints;
    }

    private static BitMap ReadTexture()
    {
        const string path = "Assets/TEXTURE.png";
        using var stream = File.OpenRead(path);
        Bitmap bitmap = new Bitmap(stream);
        BitMap bmp = new BitMap(bitmap);
        return bmp;
    }

    private static BitMap ReadNormal()
    {
        const string path = "Assets/NORMAL.png";
        using var stream = File.OpenRead(path);
        Bitmap bitmap = new Bitmap(stream);
        BitMap bmp = new BitMap(bitmap);
        return bmp;
    }
}