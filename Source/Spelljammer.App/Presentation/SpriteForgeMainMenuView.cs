using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Spelljammer.Interop;

namespace Spelljammer.Presentation;

internal sealed class SpriteForgeMainMenuView : FrameworkElement, IDisposable
{
    internal const double LogicalWidth = 1280;
    internal const double LogicalHeight = 720;
    private const uint ExpectedUiInteropVersion = 1;
    private const uint ElementCapacity = 12;
    private const uint ActionCapacity = 8;

    private static readonly ulong RootKey = StableKey("spelljammer.menu.root");
    private static readonly ulong PanelKey = StableKey("spelljammer.menu.panel");
    private static readonly ulong TitleKey = StableKey("spelljammer.menu.title");
    private static readonly ulong SubtitleKey = StableKey("spelljammer.menu.subtitle");
    private static readonly ulong SettingsButtonKey = StableKey("spelljammer.menu.settings");
    private static readonly ulong QuitButtonKey = StableKey("spelljammer.menu.quit");
    private static readonly ulong StatusKey = StableKey("spelljammer.menu.status");
    private static readonly ulong VersionKey = StableKey("spelljammer.menu.version");

    private readonly GameText strings;
    private readonly string version;
    private readonly BitmapSource background;
    private readonly ulong[] elementKeys =
    [
        PanelKey,
        TitleKey,
        SubtitleKey,
        SettingsButtonKey,
        QuitButtonKey,
        StatusKey,
        VersionKey,
    ];
    private readonly Dictionary<ulong, EngineUiElementSnapshot> snapshots = [];
    private readonly EngineUiPresentationCommand[] presentation = new EngineUiPresentationCommand[ElementCapacity];
    private readonly EngineUiAction[] actions = new EngineUiAction[ActionCapacity];
    private nint context;
    private ulong document;
    private ulong inputSequence;
    private string status = string.Empty;
    private bool statusIsError;
    private bool disposed;

    internal SpriteForgeMainMenuView(GameText strings, string version)
    {
        this.strings = strings;
        this.version = version;
        background = LoadBackground();
        Focusable = true;
        SnapsToDevicePixels = true;
        CreateNativeDocument();
        Loaded += View_Loaded;
        Unloaded += View_Unloaded;
    }

    internal event EventHandler? SettingsRequested;
    internal event EventHandler? QuitRequested;

    internal void SetStatus(string text, bool isError)
    {
        status = text;
        statusIsError = isError;
        InvalidateVisual();
    }

    internal void RefreshLanguage()
    {
        DestroyNativeDocument();
        CreateNativeDocument();
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize) => new(LogicalWidth, LogicalHeight);

    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    protected override AutomationPeer OnCreateAutomationPeer() => new FrameworkElementAutomationPeer(this);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        DrawBackground(drawingContext);
        if (context == nint.Zero)
        {
            return;
        }

        RefreshSnapshots();
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIBuildPresentation(
            context,
            document,
            presentation,
            (uint)presentation.Length,
            out uint commandCount), "build the main-menu presentation");
        for (int index = 0; index < commandCount; ++index)
        {
            EngineUiPresentationCommand command = presentation[index];
            drawingContext.DrawRectangle(
                ToBrush(command.Color),
                null,
                Scale(command.X, command.Y, command.Width, command.Height));
        }

        strings.BeginFrame();
        DrawText(drawingContext, TitleKey, strings.Get("menu.title"), 48, "#F4E7CE",
            TextAlignment.Center, FontWeights.SemiBold);
        DrawText(drawingContext, SubtitleKey, strings.Get("menu.subtitle"), 16, "#C4BBD1",
            TextAlignment.Center, FontWeights.Normal);
        DrawText(drawingContext, SettingsButtonKey, strings.Get("menu.button.settings"), 20, "#F4E7CE",
            TextAlignment.Center, FontWeights.SemiBold);
        DrawText(drawingContext, QuitButtonKey, strings.Get("menu.button.quit"), 20, "#F4E7CE",
            TextAlignment.Center, FontWeights.SemiBold);
        if (!string.IsNullOrEmpty(status))
        {
            DrawText(drawingContext, StatusKey, status, 13,
                statusIsError ? "#F39A8D" : "#B8C7DF", TextAlignment.Center, FontWeights.Normal);
        }
        DrawText(drawingContext, VersionKey, strings.Version(version), 13,
            "#C4BBD1", TextAlignment.Right, FontWeights.Normal);

        foreach (EngineUiElementSnapshot snapshot in snapshots.Values.Where(value => value.Focused != 0))
        {
            drawingContext.DrawRectangle(null, new Pen(Brush("#D7AF70"), 3),
                Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height));
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        SendPointer(EngineUiInputType.PointerMoved, e.GetPosition(this));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        CaptureMouse();
        SendPointer(EngineUiInputType.PointerDown, e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        SendPointer(EngineUiInputType.PointerUp, e.GetPosition(this));
        ReleaseMouseCapture();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        EngineUiNavigation navigation = e.Key switch
        {
            Key.Tab when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => EngineUiNavigation.Previous,
            Key.Tab => EngineUiNavigation.Next,
            Key.Left => EngineUiNavigation.Left,
            Key.Right => EngineUiNavigation.Right,
            Key.Up => EngineUiNavigation.Up,
            Key.Down => EngineUiNavigation.Down,
            Key.Enter or Key.Space => EngineUiNavigation.Accept,
            _ => EngineUiNavigation.None,
        };
        if (navigation == EngineUiNavigation.None)
        {
            base.OnKeyDown(e);
            return;
        }

        Process([new EngineUiInput
        {
            Type = EngineUiInputType.Navigation,
            Navigation = navigation,
            Sequence = ++inputSequence,
            InsideViewport = 1,
        }]);
        e.Handled = true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        DestroyNativeDocument();

        Loaded -= View_Loaded;
        Unloaded -= View_Unloaded;
        GC.SuppressFinalize(this);
    }

    private static BitmapSource LoadBackground()
    {
        BitmapImage image = new();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(
            "pack://application:,,,/Assets/UI/MainMenu/Background.png",
            UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private void CreateNativeDocument()
    {
        uint version = SpriteForgeNative.SpriteForge_GetUIInteropVersion();
        if (version != ExpectedUiInteropVersion)
        {
            throw new InvalidOperationException(
                $"SpriteForge.dll exposes UI interop version {version}; Spelljammer expects {ExpectedUiInteropVersion}.");
        }

        EngineUiDocumentDescription description = new()
        {
            RootKey = RootKey,
            LogicalWidth = (uint)LogicalWidth,
            LogicalHeight = (uint)LogicalHeight,
            MaximumElements = ElementCapacity,
            MaximumActions = ActionCapacity,
            Theme = new EngineUiTheme
            {
                Panel = Color(0, 0, 0, 0),
                Button = Color(0, 0, 0, 0),
                ButtonHovered = Color(0, 0, 0, 0),
                ButtonPressed = Color(0, 0, 0, 0),
                ButtonFocused = Color(0, 0, 0, 0),
                ButtonDisabled = Color(0, 0, 0, 0),
            },
        };
        ThrowIfFailed(SpriteForgeNative.SpriteForge_CreateUIContext(
            in description,
            out context,
            out document), "create the main-menu UI document");

        List<nint> allocatedNames = [];
        try
        {
            EngineUiElementDescription[] elements = BuildElements(allocatedNames);
            ThrowIfFailed(SpriteForgeNative.SpriteForge_UIAddElements(
                context,
                document,
                elements,
                (uint)elements.Length), "commit the main-menu UI document");
        }
        catch
        {
            SpriteForgeNative.SpriteForge_DestroyUIContext(context);
            context = nint.Zero;
            document = 0;
            throw;
        }
        finally
        {
            foreach (nint name in allocatedNames)
            {
                Marshal.FreeCoTaskMem(name);
            }
        }
    }

    private EngineUiElementDescription[] BuildElements(List<nint> allocatedNames) =>
    [
        Element(PanelKey, RootKey, 800, 70, 420, 580, EngineUiBehavior.None,
            strings.Get("menu.accessibility.main"), allocatedNames, modal: true,
            customColor: true, color: Color(0, 0, 0, 0)),
        TextElement(TitleKey, 30, 55, 360, 72, strings.Get("menu.title"), allocatedNames),
        TextElement(SubtitleKey, 30, 135, 360, 50, strings.Get("menu.subtitle"), allocatedNames),
        Button(SettingsButtonKey, 60, 290, 300, 68, 0,
            strings.Get("menu.button.settings"), allocatedNames),
        Button(QuitButtonKey, 60, 382, 300, 68, 1,
            strings.Get("menu.button.quit"), allocatedNames),
        TextElement(StatusKey, 35, 485, 350, 58,
            strings.Get("menu.accessibility.status"), allocatedNames),
        Element(VersionKey, RootKey, 1010, 674, 240, 26, EngineUiBehavior.None,
            strings.Version(version), allocatedNames, kind: EngineUiElementKind.Text,
            customColor: true, color: Color(0, 0, 0, 0)),
    ];

    private static EngineUiElementDescription TextElement(
        ulong key,
        float x,
        float y,
        float width,
        float height,
        string name,
        List<nint> names) =>
        Element(key, PanelKey, x, y, width, height, EngineUiBehavior.None, name, names,
            kind: EngineUiElementKind.Text, customColor: true, color: Color(0, 0, 0, 0));

    private static EngineUiElementDescription Button(
        ulong key,
        float x,
        float y,
        float width,
        float height,
        int tabOrder,
        string name,
        List<nint> names) =>
        Element(key, PanelKey, x, y, width, height, EngineUiBehavior.Button, name, names, tabOrder: tabOrder);

    private static EngineUiElementDescription Element(
        ulong key,
        ulong parent,
        float x,
        float y,
        float width,
        float height,
        EngineUiBehavior behavior,
        string accessibleName,
        List<nint> allocatedNames,
        int tabOrder = int.MaxValue,
        bool modal = false,
        EngineUiElementKind kind = EngineUiElementKind.Container,
        bool customColor = false,
        EngineUiColor color = default)
    {
        byte[] encoded = Encoding.UTF8.GetBytes(accessibleName);
        nint name = Marshal.StringToCoTaskMemUTF8(accessibleName);
        allocatedNames.Add(name);
        bool interactive = behavior != EngineUiBehavior.None;
        return new EngineUiElementDescription
        {
            Key = key,
            ParentKey = parent,
            Action = interactive ? key : 0,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            SliderMinimum = 0,
            SliderMaximum = 1,
            SliderValue = 0,
            SliderStep = 0.1f,
            TabOrder = tabOrder,
            Kind = kind,
            Behavior = behavior,
            Visible = 1,
            Enabled = 1,
            HitTestable = interactive ? 1u : 0u,
            Modal = modal ? 1u : 0u,
            Focusable = interactive ? 1u : 0u,
            CustomColor = customColor ? 1u : 0u,
            Color = color,
            AccessibleNameUtf8 = name,
            AccessibleNameBytes = (uint)encoded.Length,
        };
    }

    private void DrawBackground(DrawingContext drawingContext)
    {
        double scale = Math.Max(ActualWidth / background.PixelWidth, ActualHeight / background.PixelHeight);
        double width = background.PixelWidth * scale;
        double height = background.PixelHeight * scale;
        drawingContext.DrawImage(background, new Rect(
            (ActualWidth - width) / 2,
            (ActualHeight - height) / 2,
            width,
            height));
    }

    private void DrawText(
        DrawingContext drawingContext,
        ulong key,
        string text,
        double fontSize,
        string color,
        TextAlignment alignment,
        FontWeight weight)
    {
        if (!snapshots.TryGetValue(key, out EngineUiElementSnapshot snapshot))
        {
            return;
        }

        double scale = CalculateLayoutTransform().Scale;
        FormattedText formatted = new(
            text,
            strings.Culture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, weight, FontStretches.Normal),
            fontSize * scale,
            Brush(color),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1, snapshot.Width * scale),
            TextAlignment = alignment,
            Trimming = TextTrimming.CharacterEllipsis,
        };
        Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
        drawingContext.DrawText(formatted, new Point(bounds.X, bounds.Y + Math.Max(0, (bounds.Height - formatted.Height) / 2)));
    }

    private void SendPointer(EngineUiInputType type, Point physical)
    {
        if (context == nint.Zero || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        (double scale, double offsetX, double offsetY) = CalculateLayoutTransform();
        float x = (float)((physical.X - offsetX) / scale);
        float y = (float)((physical.Y - offsetY) / scale);
        Process([new EngineUiInput
        {
            Type = type,
            X = x,
            Y = y,
            Sequence = ++inputSequence,
            PointerId = 1,
            InsideViewport = x >= 0 && y >= 0 && x < LogicalWidth && y < LogicalHeight ? 1u : 0u,
        }]);
    }

    private void Process(EngineUiInput[] input)
    {
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIProcessInput(
            context,
            document,
            input,
            (uint)input.Length), "process main-menu input");
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIConsumeActions(
            context,
            document,
            actions,
            (uint)actions.Length,
            out uint actionCount), "consume main-menu actions");
        for (int index = 0; index < actionCount; ++index)
        {
            EngineUiAction action = actions[index];
            if (action.Source == SettingsButtonKey)
            {
                SettingsRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (action.Source == QuitButtonKey)
            {
                QuitRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        InvalidateVisual();
    }

    private void RefreshSnapshots()
    {
        EngineUiElementSnapshot[] values = new EngineUiElementSnapshot[elementKeys.Length];
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIGetElementSnapshots(
            context,
            document,
            elementKeys,
            (uint)elementKeys.Length,
            values,
            (uint)values.Length,
            out uint count), "copy main-menu element snapshots");
        snapshots.Clear();
        for (int index = 0; index < count; ++index)
        {
            snapshots.Add(values[index].Key, values[index]);
        }
    }

    private Rect Scale(float x, float y, float width, float height)
    {
        (double scale, double offsetX, double offsetY) = CalculateLayoutTransform();
        return new Rect(
            offsetX + x * scale,
            offsetY + y * scale,
            width * scale,
            height * scale);
    }

    private (double Scale, double OffsetX, double OffsetY) CalculateLayoutTransform()
    {
        double scale = Math.Min(ActualWidth / LogicalWidth, ActualHeight / LogicalHeight);
        return (scale, (ActualWidth - LogicalWidth * scale) / 2, (ActualHeight - LogicalHeight * scale) / 2);
    }

    private static EngineUiColor Color(float red, float green, float blue, float alpha = 1) =>
        new() { Red = red, Green = green, Blue = blue, Alpha = alpha };

    private static SolidColorBrush ToBrush(EngineUiColor color)
    {
        Color value = System.Windows.Media.Color.FromArgb(
            Channel(color.Alpha), Channel(color.Red), Channel(color.Green), Channel(color.Blue));
        SolidColorBrush brush = new(value);
        brush.Freeze();
        return brush;
    }

    private static SolidColorBrush Brush(string value)
    {
        SolidColorBrush brush = new((Color)ColorConverter.ConvertFromString(value));
        brush.Freeze();
        return brush;
    }

    private static byte Channel(float value) => (byte)Math.Round(Math.Clamp(value, 0, 1) * byte.MaxValue);

    private static ulong StableKey(string value)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offset;
        foreach (byte item in Encoding.UTF8.GetBytes(value))
        {
            hash ^= item;
            hash *= prime;
        }

        return hash == 0 ? 1 : hash;
    }

    private void View_Loaded(object sender, RoutedEventArgs e) => Focus();

    private void View_Unloaded(object sender, RoutedEventArgs e) => Dispose();

    private void DestroyNativeDocument()
    {
        if (context == nint.Zero)
        {
            return;
        }

        SpriteForgeNative.SpriteForge_DestroyUIContext(context);
        context = nint.Zero;
        document = 0;
        snapshots.Clear();
    }

    private static void ThrowIfFailed(EngineStatus status, string operation)
    {
        if (status != EngineStatus.Success)
        {
            throw new InvalidOperationException(
                $"SpriteForge.dll could not {operation} ({status}, {(int)status}).");
        }
    }
}
