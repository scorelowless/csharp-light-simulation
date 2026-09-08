using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SurfaceProject.UI;

public class SceneView : Control
{
    public required SceneState State { get; set; }
    private WriteableBitmap? _bitmap;

    public void Update()
    {
        _bitmap = State.Render();
    }

    public void Invalidate()
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_bitmap == null) return;
        context.DrawImage(_bitmap, new Rect(0, 0, Bounds.Width, Bounds.Height));
    }
}