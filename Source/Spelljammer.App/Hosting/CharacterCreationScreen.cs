using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Spelljammer.Presentation;

namespace Spelljammer;

internal sealed class CharacterCreationScreen : Grid, IDisposable
{
    private readonly SpriteForgeCharacterCreationView creationView;
    private bool disposed;

    internal CharacterCreationScreen(GameText strings, CharacterCreationSelection? initial)
    {
        Background = new SolidColorBrush(Color.FromArgb(224, 3, 6, 14));
        creationView = new SpriteForgeCharacterCreationView(strings, initial);
        creationView.Completed += CreationView_Completed;
        creationView.CancelRequested += CreationView_CancelRequested;
        Children.Add(new Viewbox
        {
            Child = creationView,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20),
        });
    }

    internal event EventHandler<CharacterCreationCompletedEventArgs>? Completed;
    internal event EventHandler? Cancelled;

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        creationView.Completed -= CreationView_Completed;
        creationView.CancelRequested -= CreationView_CancelRequested;
        creationView.Dispose();
        Children.Clear();
        GC.SuppressFinalize(this);
    }

    private void CreationView_Completed(object? sender, CharacterCreationCompletedEventArgs e) =>
        Completed?.Invoke(this, e);

    private void CreationView_CancelRequested(object? sender, EventArgs e) =>
        Cancelled?.Invoke(this, EventArgs.Empty);
}
