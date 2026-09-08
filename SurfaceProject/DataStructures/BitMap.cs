using System;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SurfaceProject.DataStructures;

public class BitMap
{
    private readonly WriteableBitmap _bitmap;
    private readonly int _width;
    private readonly int _height;
    private readonly uint[] _pixels;

    public BitMap(int width, int height)
    {
        _width = width;
        _height = height;
        _pixels = new uint[width * height];
        Reset();
        _bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);
    }

    public BitMap(Bitmap bitmap) : this(bitmap.PixelSize.Width, bitmap.PixelSize.Height)
    {
        using var fb = _bitmap.Lock();
        bitmap.CopyPixels(fb, AlphaFormat.Unpremul);
        IntPtr ptr = fb.Address;
        Marshal.Copy(ptr, (_pixels as object as int[])!, 0, _pixels.Length);
    }

    public void SetPixel(int x, int y, Color color)
    {
        TransformPoint(ref x, ref y);
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return;
        Volatile.Write(ref _pixels[y * _width + x], color.ToUInt32());
    }
    
    public Color GetPixel(float u, float v)
    {
        int x = (int)(u * _width);
        int y = (int)(v * _height);
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return Colors.Transparent;
        }
        uint pixel = Volatile.Read(ref _pixels[y * _width + x]);
        return Color.FromUInt32(pixel);
    }

    private void TransformPoint(ref int x, ref int y)
    {
        x += _width / 2;
        y *= -1;
        y += _height / 2;
    }

    private void UpdateBitmap()
    {
        using var fb = _bitmap.Lock();
        IntPtr ptr = fb.Address;
        Marshal.Copy((_pixels as object as int[])!, 0, ptr, _pixels.Length);
    }

    public void Reset()
    {
        for (int i = 0; i < _height; i++)
        {
            for(int j = 0; j < _width; j++)
            {
                _pixels[i * _width + j] = 0xFFFFFFFF; // white background
            }
        }
    }

    public WriteableBitmap GetBitmap()
    {
        UpdateBitmap();
        return _bitmap;
    }
}