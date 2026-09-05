using System.Windows;

namespace WaterMargin;

public partial class MainWindow : Window
{
    private const int FrameCount = 4;

    public MainWindow()
    {
        InitializeComponent();
        Viewport.FrameChanged += Viewport_FrameChanged;
        Closed += MainWindow_Closed;
        ShowFrame(0);
    }

    private void Viewport_FrameChanged(object? sender, int frame)
    {
        ShowFrame(frame);
    }

    private void ShowFrame(int frame)
    {
        FrameLabel.Text = $"Frame {frame + 1} / {FrameCount} · 8 FPS";
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        Viewport.TogglePlayback();
        PlayPauseButton.Content = Viewport.IsPlaying ? "Pause" : "Play";
    }

    private void StepButton_Click(object sender, RoutedEventArgs e)
    {
        Viewport.StepFrame();
        PlayPauseButton.Content = "Play";
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        Viewport.Restart();
        PlayPauseButton.Content = "Pause";
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Viewport.FrameChanged -= Viewport_FrameChanged;
        Closed -= MainWindow_Closed;
    }
}
