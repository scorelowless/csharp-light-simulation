using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SurfaceProject.DataStructures;
using SurfaceProject.UI;

namespace SurfaceProject;

public partial class MainWindow : Window
{
    private SceneState _state;

    public MainWindow()
    {
        InitializeComponent();
        Opened += OnOpened;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            _state = new SceneState();
            DataContext = _state;
            SceneView.State = _state;
        }
        catch (Exception ex)
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(
                "Błąd",
                $"Błąd przy wczytywaniu zasobów:\n{ex.Message}\n\nAplikacja się zamknie.",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error);

            await msg.ShowWindowAsync();
            Environment.Exit(1);
        }
        _ = RenderLoop();
    }

    private async Task RenderLoop()
    {
        var sw = new Stopwatch();
        while (true)
        {
            sw.Restart();
            LightingAnimation();
            SpinAnimation();
            await Task.Run(() => SceneView.Update());
            await Dispatcher.UIThread.InvokeAsync(() => SceneView.Invalidate());
            var delay = int.Max(0, 33 - (int)sw.ElapsedMilliseconds);
            await Task.Delay(delay);
        }
    }
    
    private int _tickCount;
    private void LightingAnimation()
    {
        if(_state.StopAnimationLight) return;
        _tickCount++;
        _state.Scene.Lighting.LightPosition = _state.Scene.Lighting.LightPosition with
        {
            X = _tickCount / 3f * (float.Cos(_tickCount / 10f)),
            Y = _tickCount / 3f * (float.Sin(_tickCount / 10f))
        };
    }
    
    private void SpinAnimation()
    {
        if(_state.StopAnimationSpin) return;
        _state.Scene.Alpha1 += 0.03f;
        _state.Scene.Alpha2 += 0.003f;
        _state.Scene.Beta1 -= 0.003f;
        _state.Scene.Beta2 += 0.03f;
    }

    public void Reset(object sender, RoutedEventArgs args)
    {
        _tickCount = 0;
        _state.Scene.Lighting.LightPosition = _state.Scene.Lighting.LightPosition with
        {
            X = 0,
            Y = 0,
        };
    }
    
    public void StartStop(object sender, RoutedEventArgs args)
    {
        _state.StopAnimationLight = !_state.StopAnimationLight;
        StartStopButton.Content = _state.StopAnimationLight ? "Puść animację światła" : "Zatrzymaj animację światła";
    }

    public void SpinAnimation(object sender, RoutedEventArgs args)
    {
        _state.StopAnimationSpin = !_state.StopAnimationSpin;
        SpinButton.Content = _state.StopAnimationSpin ? "Puść obracanie powierzchni" : "Zatrzymaj obracanie powierzchni";
    }
    
    public async void ChooseFile(object sender, RoutedEventArgs args)
    {
        try
        {
            var top = GetTopLevel(this);
            if (top == null)
                return;
            string title;
            bool isTexture = false;
        
            if (Equals(sender, ChooseTextureButton))
            {
                title = "Wybierz plik tekstury";
                isTexture = true;
            }
            else if (Equals(sender, ChooseNormalButton))
            {
                title = "Wybierz plik mapy normalnych";
            }
            else
            {
                return;
            }
            var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = 
                [
                    new FilePickerFileType("Images")
                    {
                        Patterns = ["*.png", "*.jpg", "*.jpeg"]
                    }
                ]
            });

            if (files.Count <= 0) return;
            var file = files[0];
            await using var stream = await file.OpenReadAsync();
            if (isTexture)
            {
                _state.Texture = new BitMap(new Bitmap(stream));
            }
            else
            {
                _state.NormalMap = new BitMap(new Bitmap(stream));
            }
        }
        catch (Exception e)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Błąd",
                $"Błąd przy wczytywaniu wybieraniu pliku:\n{e.Message}\n\nSpróbuj ponownie.",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error);
            await messageBox.ShowWindowAsync();
        }
    }
}