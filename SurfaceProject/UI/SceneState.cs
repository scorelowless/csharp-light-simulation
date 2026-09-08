using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SurfaceProject.DataStructures;

namespace SurfaceProject.UI;

public class SceneState
{
    public Scene Scene { get; } = new();

    public WriteableBitmap Render() => Scene.Render();

    public float Kd
    {
        get => Scene.Lighting.kd;
        set => Scene.Lighting.kd = value;
    }
    
    public float Ks
    {
        get => Scene.Lighting.ks;
        set => Scene.Lighting.ks = value;
    }
    
    public int M
    {
        get => Scene.Lighting.m;
        set => Scene.Lighting.m = value;
    }
    
    public float LightZ
    {
        get => Scene.Lighting.LightPosition.Z;
        set => Scene.Lighting.LightPosition = Scene.Lighting.LightPosition with { Z = value };
    }
    
    public int Subdivisions
    {
        get => Scene.Subdivisions;
        set => Scene.Subdivisions = value;
    }
    
    public float Alpha
    {
        get => float.RadiansToDegrees(Scene.Alpha1);
        set => Scene.Alpha1 = float.DegreesToRadians(value);
    }
    
    public float Beta
    {
        get => float.RadiansToDegrees(Scene.Beta1);
        set => Scene.Beta1 = float.DegreesToRadians(value);
    }
    
    public bool Fill
    {
        get => Scene.Fill;
        set => Scene.Fill = value;
    }
    
    public bool Triangles
    {
        get => Scene.Triangles;
        set => Scene.Triangles = value;
    }
    
    public bool Edges
    {
        get => Scene.Edges;
        set => Scene.Edges = value;
    }
    
    public bool UseNormalMap
    {
        get => Scene.Lighting.UseNormalMap;
        set => Scene.Lighting.UseNormalMap = value;
    }

    public Color LightColor
    {
        get => Scene.Lighting.LightColor.Color;
        set => Scene.Lighting.LightColor = new FloatColor(value.R / 255f, value.G / 255f, value.B / 255f);
    }
    
    public Color ObjectColor
    {
        get => Scene.Lighting.ObjectColor.Color;
        set => Scene.Lighting.ObjectColor = new FloatColor(value.R / 255f, value.G / 255f, value.B / 255f);
    }

    public bool IsObjectTexture 
    {
        get => Scene.Lighting.UseTexture;
        set => Scene.Lighting.UseTexture = value;
    }
    
    public bool IsObjectColor
    {
        get => !Scene.Lighting.UseTexture;
        set => Scene.Lighting.UseTexture = !value;
    }

    public BitMap Texture
    {
        set => Scene.Lighting.Texture = value;
    }
    
    public BitMap NormalMap
    {
        set => Scene.Lighting.NormalMap = value;
    }

    public bool StopAnimationLight { get; set; }
    public bool StopAnimationSpin { get; set; }
}