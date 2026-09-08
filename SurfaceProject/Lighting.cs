// ReSharper disable InconsistentNaming

using System.Numerics;
using Avalonia.Media;
using SurfaceProject.Bezier;
using SurfaceProject.DataStructures;

namespace SurfaceProject;

public class Lighting
{
    public float kd { get; set; }
    public float ks { get; set; }
    public FloatColor LightColor { get; set; } = new();
    public FloatColor ObjectColor { get; set; } = new();
    public BitMap Texture { get; set; }
    public BitMap NormalMap { get; set; }
    public bool UseNormalMap { get; set; }
    public bool UseTexture { get; set; }
    public int m { get; set; }
    public Vector3 LightPosition { get; set; }
    private readonly Vector3 V = new(0, 0, 1);
    
    public Lighting(BitMap texture, BitMap normalMap)
    {
        kd = 0.5f;
        ks = 0.5f;
        m = 40;
        LightPosition = new Vector3(0, 0, 400);
        Texture = texture;
        NormalMap = normalMap;
    }

    public void ComputeColor(Vertex v, Rasterizer.ThreadState local, bool isConst)
    {
        Vector3 L = Vector3.Normalize(LightPosition - v.PRot);
        Vector3 N = Vector3.Normalize(v.NRot);

        if (isConst)
        {
            local.Color.FromRgb(0.6f, 0.8f, 0.1f);
        }
        else
        {
            if (UseTexture)
            {
                Color? textureColor = Texture.GetPixel(v.u, v.v);
                local.Color.FromRgb(textureColor.Value.R, textureColor.Value.G, textureColor.Value.B);
                // color = new FloatColor(textureColor.Value.R / 255f, textureColor.Value.G / 255f,
                //    textureColor.Value.B / 255f);
            }
            else
            {
                //color = ObjectColor;
                local.Color.FromColor(ObjectColor);
            }

            if (UseNormalMap)
            {
                Color normalColor = NormalMap.GetPixel(v.u, v.v);
                Vector3 normal = new Vector3(normalColor.R / 255f * 2 - 1,
                    normalColor.G / 255f * 2 - 1,
                    normalColor.B / 255f * 2 - 1);
                Vector3 Pu = Vector3.Normalize(v.PuRot);
                Vector3 Pv = Vector3.Normalize(v.PvRot);
                // Matrix3 M = new Matrix3(Pu, Pv, N);
                local.TempMatrix.FromColumns(Pu, Pv, N);
                N = Vector3.Normalize(local.TempMatrix*normal);
            }
        }
        local.ResultColor.FromRgb(0f, 0f, 0f);
        ComputeDiffuse(L, N, local); // zapisuje do zmiennej w local
        ComputeSpecular(L, N, local); // dodaje do tej samej zmiennej w local
    }

    private void ComputeDiffuse(Vector3 L, Vector3 N, Rasterizer.ThreadState local)
    {
        float cos = Vector3.Dot(N, L);
        // if (cos < 0) cos = 0;
        cos = float.Abs(cos); // aby obie strony były oświetlone
        float r = kd * LightColor.R * local.Color.R * cos;
        float g = kd * LightColor.G * local.Color.G * cos;
        float b = kd * LightColor.B * local.Color.B * cos;
        local.TempColor.FromRgb(r, g, b);
        local.ResultColor += local.TempColor;
    } 
    private void ComputeSpecular(Vector3 L, Vector3 N, Rasterizer.ThreadState local)
    {
        Vector3 R = Vector3.Normalize(2 * Vector3.Dot(N, L) * N - L);
        float cos = Vector3.Dot(V, R);
        // if (cos < 0) cos = 0;
        cos = float.Abs(cos); // aby obie strony były oświetlone
        cos = float.Pow(cos, m);
        float r = ks * LightColor.R * local.Color.R * cos;
        float g = ks * LightColor.G * local.Color.G * cos;
        float b = ks * LightColor.B * local.Color.B * cos;
        local.TempColor.FromRgb(r, g, b);
        local.ResultColor += local.TempColor;
    }
}