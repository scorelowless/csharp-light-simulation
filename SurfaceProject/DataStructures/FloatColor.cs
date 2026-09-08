using Avalonia.Media;

namespace SurfaceProject.DataStructures;

public class FloatColor
{
    public float R;
    public float G;
    public float B;

    public FloatColor()
    {
        R = 1;
        G = 1;
        B = 1;
    }
    public FloatColor(float r, float g, float b)
    {
        R = float.Clamp(r, 0, 1);
        G = float.Clamp(g, 0, 1);
        B = float.Clamp(b, 0, 1);
    }

    public void FromRgb(float r, float g, float b)
    {
        // clamp niepotrzebny, bo użycia tej funkcji mają po sobie operator+ który je sclampuje
        R = r;
        G = g;
        B = b;
    }
    
    public void FromRgb(byte r, byte g, byte b)
    {
        // clamp jest niepotrzebny
        R = r / 255f;
        G = g / 255f;
        B = b / 255f;
    }
    
    public void FromColor(FloatColor color)
    {
        R = color.R;
        G = color.G;
        B = color.B;
    }
    
    public static FloatColor operator +(FloatColor c1, FloatColor c2)
    {
        //return new FloatColor(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
        c1.R = float.Clamp(c1.R + c2.R, 0, 1);
        c1.G = float.Clamp(c1.G + c2.G, 0, 1);
        c1.B = float.Clamp(c1.B + c2.B, 0, 1);
        return c1;
    }

    public Color Color => Color.FromRgb((byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
}