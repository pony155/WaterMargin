using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using Spelljammer.Interop;
using Spelljammer.Localization;

namespace Spelljammer.Presentation;

internal sealed class CharacterCreationCompletedEventArgs(CharacterCreationSelection selection) : EventArgs
{
    internal CharacterCreationSelection Selection { get; } = selection;
}

internal sealed class SpriteForgeCharacterCreationView : FrameworkElement, IDisposable
{
    internal const double LogicalWidth = 1100;
    internal const double LogicalHeight = 680;
    private const uint ExpectedUiInteropVersion = 1;
    private const uint ElementCapacity = 32;
    private const uint ActionCapacity = 16;

    private static readonly ulong RootKey = StableKey("spelljammer.creation.root");
    private static readonly ulong ModalKey = StableKey("spelljammer.creation.modal");
    private static readonly ulong TitleKey = StableKey("spelljammer.creation.title");
    private static readonly ulong IntroductionKey = StableKey("spelljammer.creation.introduction");
    private static readonly ulong PreviousKey = StableKey("spelljammer.creation.previous");
    private static readonly ulong PortraitKey = StableKey("spelljammer.creation.portrait");
    private static readonly ulong CaptainKey = StableKey("spelljammer.creation.captain");
    private static readonly ulong NextKey = StableKey("spelljammer.creation.next");
    private static readonly ulong DetailPanelKey = StableKey("spelljammer.creation.details");
    private static readonly ulong RaceLabelKey = StableKey("spelljammer.creation.race-label");
    private static readonly ulong RaceValueKey = StableKey("spelljammer.creation.race-value");
    private static readonly ulong HeritageLabelKey = StableKey("spelljammer.creation.heritage-label");
    private static readonly ulong HeritageValueKey = StableKey("spelljammer.creation.heritage-value");
    private static readonly ulong BackgroundLabelKey = StableKey("spelljammer.creation.background-label");
    private static readonly ulong BackgroundValueKey = StableKey("spelljammer.creation.background-value");
    private static readonly ulong SummaryKey = StableKey("spelljammer.creation.summary");
    private static readonly ulong SeedLabelKey = StableKey("spelljammer.creation.seed-label");
    private static readonly ulong SeedValueKey = StableKey("spelljammer.creation.seed-value");
    private static readonly ulong StatusKey = StableKey("spelljammer.creation.status");
    private static readonly ulong BackKey = StableKey("spelljammer.creation.back");
    private static readonly ulong RerollKey = StableKey("spelljammer.creation.reroll");
    private static readonly ulong ConfirmKey = StableKey("spelljammer.creation.confirm");
    private static readonly ulong CancelAction = StableKey("spelljammer.creation.action.cancel");

    private readonly ulong[] elementKeys =
    [
        ModalKey, TitleKey, IntroductionKey, PreviousKey, PortraitKey, CaptainKey, NextKey,
        DetailPanelKey, RaceLabelKey, RaceValueKey, HeritageLabelKey, HeritageValueKey,
        BackgroundLabelKey, BackgroundValueKey, SummaryKey, SeedLabelKey, SeedValueKey,
        StatusKey, BackKey, RerollKey, ConfirmKey,
    ];
    private readonly Dictionary<ulong, EngineUiElementSnapshot> snapshots = [];
    private readonly EngineUiPresentationCommand[] presentation = new EngineUiPresentationCommand[ElementCapacity];
    private readonly EngineUiAction[] actions = new EngineUiAction[ActionCapacity];
    private readonly GameText strings;
    private nint context;
    private ulong document;
    private ulong inputSequence;
    private int choiceIndex;
    private ulong seed;
    private bool disposed;

    internal SpriteForgeCharacterCreationView(GameText strings, CharacterCreationSelection? initial)
    {
        this.strings = strings;
        choiceIndex = initial is null ? 0 : FindChoiceIndex(initial.Choice.CharacterId);
        seed = initial?.Seed ?? NewSeed();
        Focusable = true;
        SnapsToDevicePixels = true;
        CreateNativeDocument();
        Loaded += View_Loaded;
        Unloaded += View_Unloaded;
    }

    internal event EventHandler<CharacterCreationCompletedEventArgs>? Completed;
    internal event EventHandler? CancelRequested;

    protected override Size MeasureOverride(Size availableSize) => new(LogicalWidth, LogicalHeight);

    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    protected override AutomationPeer OnCreateAutomationPeer() => new FrameworkElementAutomationPeer(this);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        drawingContext.DrawRectangle(Brush("#090D18"), null, new Rect(RenderSize));
        if (context == nint.Zero)
        {
            return;
        }

        RefreshSnapshots();
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIBuildPresentation(
            context, document, presentation, (uint)presentation.Length, out uint commandCount),
            "build the character-creation presentation");
        for (int index = 0; index < commandCount; ++index)
        {
            EngineUiPresentationCommand command = presentation[index];
            drawingContext.DrawRectangle(
                ToBrush(command.Color),
                null,
                Scale(command.X, command.Y, command.Width, command.Height));
        }

        CharacterCreationChoice choice = CurrentChoice;
        string captain = strings.Get($"creation.captain.{choice.TextId}.name");
        string race = strings.Get($"creation.race.{choice.TextId}.name");
        string heritage = strings.Get($"creation.heritage.{choice.TextId}.name");
        string background = strings.Get("creation.background.expedition-veteran.name");

        DrawPortrait(drawingContext, StringInfo.GetNextTextElement(captain));
        strings.BeginFrame();
        DrawText(drawingContext, TitleKey, strings.Get("creation.title"), 32, "#F2E9D8", false);
        DrawText(drawingContext, IntroductionKey, strings.Get("creation.introduction"), 14, "#93A1BE", false);
        DrawText(drawingContext, PreviousKey, strings.Get("creation.button.previous"), 15, "#F2E9D8", true);
        DrawText(drawingContext, CaptainKey, captain, 24, "#F2E9D8", true);
        DrawText(drawingContext, NextKey, strings.Get("creation.button.next"), 15, "#F2E9D8", true);
        DrawText(drawingContext, RaceLabelKey, strings.Get("creation.label.race"), 13, "#D7AF70", false);
        DrawText(drawingContext, RaceValueKey, race, 18, "#F2E9D8", false);
        DrawText(drawingContext, HeritageLabelKey, strings.Get("creation.label.heritage"), 13, "#D7AF70", false);
        DrawText(drawingContext, HeritageValueKey, heritage, 18, "#F2E9D8", false);
        DrawText(drawingContext, BackgroundLabelKey, strings.Get("creation.label.background"), 13, "#D7AF70", false);
        DrawText(drawingContext, BackgroundValueKey, background, 18, "#F2E9D8", false);
        DrawText(drawingContext, SummaryKey, strings.Format(
            "creation.summary",
            LocalizationArgument.Text("race", race),
            LocalizationArgument.Text("heritage", heritage),
            LocalizationArgument.Text("background", background)), 14, "#B8C7DF", false);
        DrawText(drawingContext, SeedLabelKey, strings.Get("creation.label.seed"), 13, "#D7AF70", false);
        DrawText(drawingContext, SeedValueKey, strings.Format(
            "creation.value.seed", LocalizationArgument.Unsigned("seed", seed)), 15, "#80DED9", false);
        DrawText(drawingContext, StatusKey, strings.Get("creation.status.ready"), 13, "#93A1BE", false);
        DrawText(drawingContext, BackKey, strings.Get("creation.button.back"), 14, "#F2E9D8", true);
        DrawText(drawingContext, RerollKey, strings.Get("creation.button.reroll"), 14, "#F2E9D8", true);
        DrawText(drawingContext, ConfirmKey, strings.Get("creation.button.confirm"), 14, "#F2E9D8", true);

        foreach (EngineUiElementSnapshot snapshot in snapshots.Values.Where(value => value.Focused != 0))
        {
            drawingContext.DrawRectangle(null, new Pen(Brush("#80DED9"), 2),
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
            Key.Escape => EngineUiNavigation.Cancel,
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

    private CharacterCreationChoice CurrentChoice => CharacterCreationChoices.All[choiceIndex];

    private static int FindChoiceIndex(Spelljammer.Simulation.Content.CharacterId characterId)
    {
        for (int index = 0; index < CharacterCreationChoices.All.Count; ++index)
        {
            if (CharacterCreationChoices.All[index].CharacterId == characterId)
            {
                return index;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(characterId));
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
                Panel = Color(0.067f, 0.094f, 0.153f),
                Button = Color(0.157f, 0.216f, 0.333f),
                ButtonHovered = Color(0.239f, 0.333f, 0.471f),
                ButtonPressed = Color(0.090f, 0.125f, 0.196f),
                ButtonFocused = Color(0.251f, 0.392f, 0.490f),
                ButtonDisabled = Color(0.075f, 0.090f, 0.125f),
            },
        };
        ThrowIfFailed(SpriteForgeNative.SpriteForge_CreateUIContext(
            in description, out context, out document), "create the character-creation UI document");

        List<nint> allocatedNames = [];
        try
        {
            EngineUiElementDescription[] elements = BuildElements(allocatedNames);
            ThrowIfFailed(SpriteForgeNative.SpriteForge_UIAddElements(
                context, document, elements, (uint)elements.Length),
                "commit the character-creation UI document");
        }
        catch
        {
            DestroyNativeDocument();
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

    private EngineUiElementDescription[] BuildElements(List<nint> names)
    {
        CharacterCreationChoice choice = CurrentChoice;
        string captain = strings.Get($"creation.captain.{choice.TextId}.name");
        string race = strings.Get($"creation.race.{choice.TextId}.name");
        string heritage = strings.Get($"creation.heritage.{choice.TextId}.name");
        string background = strings.Get("creation.background.expedition-veteran.name");
        string summary = strings.Format(
            "creation.summary",
            LocalizationArgument.Text("race", race),
            LocalizationArgument.Text("heritage", heritage),
            LocalizationArgument.Text("background", background));
        return
        [
            Element(ModalKey, RootKey, 20, 20, 1060, 640, EngineUiBehavior.None,
                strings.Get("creation.accessibility.screen"), names, modal: true, dismissAction: CancelAction),
            Text(TitleKey, 56, 44, 988, 42, strings.Get("creation.title"), names),
            Text(IntroductionKey, 56, 90, 988, 42, strings.Get("creation.introduction"), names),
            Button(PreviousKey, 58, 176, 128, 54, 0, strings.Get("creation.button.previous"), names),
            Element(PortraitKey, ModalKey, 210, 148, 286, 252, EngineUiBehavior.None,
                captain, names, customColor: true, color: Color(0.045f, 0.071f, 0.122f)),
            Text(CaptainKey, 210, 410, 286, 48, captain, names),
            Button(NextKey, 518, 176, 128, 54, 1, strings.Get("creation.button.next"), names),
            Element(DetailPanelKey, ModalKey, 674, 148, 368, 310, EngineUiBehavior.None,
                strings.Get("creation.accessibility.details"), names,
                customColor: true, color: Color(0.045f, 0.071f, 0.122f)),
            Text(RaceLabelKey, 704, 174, 300, 22, strings.Get("creation.label.race"), names),
            Text(RaceValueKey, 704, 196, 300, 34, race, names),
            Text(HeritageLabelKey, 704, 238, 300, 22, strings.Get("creation.label.heritage"), names),
            Text(HeritageValueKey, 704, 260, 300, 34, heritage, names),
            Text(BackgroundLabelKey, 704, 302, 300, 22, strings.Get("creation.label.background"), names),
            Text(BackgroundValueKey, 704, 324, 300, 34, background, names),
            Text(SummaryKey, 704, 372, 300, 68, summary, names),
            Text(SeedLabelKey, 58, 486, 150, 26, strings.Get("creation.label.seed"), names),
            Text(SeedValueKey, 210, 486, 430, 26,
                strings.Format("creation.value.seed", LocalizationArgument.Unsigned("seed", seed)), names),
            Text(StatusKey, 58, 530, 984, 34, strings.Get("creation.status.ready"), names),
            Button(BackKey, 566, 584, 136, 48, 3, strings.Get("creation.button.back"), names),
            Button(RerollKey, 720, 584, 136, 48, 2, strings.Get("creation.button.reroll"), names),
            Button(ConfirmKey, 874, 584, 168, 48, 4, strings.Get("creation.button.confirm"), names),
        ];
    }

    private static EngineUiElementDescription Text(
        ulong key, float x, float y, float width, float height, string name, List<nint> names) =>
        Element(key, ModalKey, x, y, width, height, EngineUiBehavior.None, name, names,
            kind: EngineUiElementKind.Text, customColor: true, color: Color(0, 0, 0, 0));

    private static EngineUiElementDescription Button(
        ulong key, float x, float y, float width, float height, int tabOrder, string name, List<nint> names) =>
        Element(key, ModalKey, x, y, width, height, EngineUiBehavior.Button, name, names, tabOrder: tabOrder);

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
        ulong dismissAction = 0,
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
            DismissAction = dismissAction,
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

    private void SendPointer(EngineUiInputType type, Point physical)
    {
        if (context == nint.Zero || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        Process([new EngineUiInput
        {
            Type = type,
            X = (float)(physical.X * LogicalWidth / ActualWidth),
            Y = (float)(physical.Y * LogicalHeight / ActualHeight),
            Sequence = ++inputSequence,
            PointerId = 1,
            InsideViewport = physical.X >= 0 && physical.Y >= 0 &&
                physical.X < ActualWidth && physical.Y < ActualHeight ? 1u : 0u,
        }]);
    }

    private void Process(EngineUiInput[] input)
    {
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIProcessInput(
            context, document, input, (uint)input.Length), "process character-creation input");
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIConsumeActions(
            context, document, actions, (uint)actions.Length, out uint actionCount),
            "consume character-creation actions");
        for (int index = 0; index < actionCount; ++index)
        {
            EngineUiAction action = actions[index];
            if (action.Source == PreviousKey)
            {
                choiceIndex = (choiceIndex + CharacterCreationChoices.All.Count - 1) %
                    CharacterCreationChoices.All.Count;
                Recreate();
            }
            else if (action.Source == NextKey)
            {
                choiceIndex = (choiceIndex + 1) % CharacterCreationChoices.All.Count;
                Recreate();
            }
            else if (action.Source == RerollKey)
            {
                seed = NewSeed();
                Recreate();
            }
            else if (action.Source == ConfirmKey)
            {
                Completed?.Invoke(this, new CharacterCreationCompletedEventArgs(
                    new CharacterCreationSelection(CurrentChoice, seed)));
            }
            else if (action.Source == BackKey || action.Type == CancelAction)
            {
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        InvalidateVisual();
    }

    private void Recreate()
    {
        DestroyNativeDocument();
        CreateNativeDocument();
        InvalidateVisual();
    }

    private void RefreshSnapshots()
    {
        EngineUiElementSnapshot[] values = new EngineUiElementSnapshot[elementKeys.Length];
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIGetElementSnapshots(
            context, document, elementKeys, (uint)elementKeys.Length, values, (uint)values.Length, out uint count),
            "copy character-creation element snapshots");
        snapshots.Clear();
        for (int index = 0; index < count; ++index)
        {
            snapshots.Add(values[index].Key, values[index]);
        }
    }

    private void DrawPortrait(DrawingContext drawingContext, string monogram)
    {
        if (!snapshots.TryGetValue(PortraitKey, out EngineUiElementSnapshot snapshot))
        {
            return;
        }

        Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
        double radius = Math.Min(bounds.Width, bounds.Height) * 0.32;
        Point center = new(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        drawingContext.DrawEllipse(Brush("#203451"), new Pen(Brush("#D7AF70"), 3), center, radius, radius);
        FormattedText formatted = new(
            monogram,
            strings.Culture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            72 * ActualHeight / LogicalHeight,
            Brush("#80DED9"),
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        drawingContext.DrawText(formatted, new Point(
            center.X - formatted.Width / 2,
            center.Y - formatted.Height / 2));
    }

    private void DrawText(
        DrawingContext drawingContext,
        ulong key,
        string text,
        double fontSize,
        string color,
        bool centered)
    {
        if (!snapshots.TryGetValue(key, out EngineUiElementSnapshot snapshot))
        {
            return;
        }

        FormattedText formatted = new(
            text,
            strings.Culture,
            FlowDirection.LeftToRight,
            new Typeface(centered ? "Segoe UI Semibold" : "Segoe UI"),
            fontSize * ActualHeight / LogicalHeight,
            Brush(color),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1, snapshot.Width * ActualWidth / LogicalWidth),
            MaxTextHeight = Math.Max(1, snapshot.Height * ActualHeight / LogicalHeight),
            TextAlignment = centered ? TextAlignment.Center : TextAlignment.Left,
            Trimming = TextTrimming.CharacterEllipsis,
        };
        Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
        double y = bounds.Y + Math.Max(0, (bounds.Height - formatted.Height) / 2);
        drawingContext.DrawText(formatted, new Point(bounds.X, y));
    }

    private Rect Scale(float x, float y, float width, float height) => new(
        x * ActualWidth / LogicalWidth,
        y * ActualHeight / LogicalHeight,
        width * ActualWidth / LogicalWidth,
        height * ActualHeight / LogicalHeight);

    private static ulong NewSeed()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(bytes);
        return value == 0 ? 1 : value;
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
