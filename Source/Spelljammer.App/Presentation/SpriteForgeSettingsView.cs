using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using Spelljammer.Interop;
using Spelljammer.Settings;

namespace Spelljammer.Presentation;

internal sealed class GameSettingsApplyRequestedEventArgs(GameSettingsProfile profile) : EventArgs
{
    internal GameSettingsProfile Profile { get; } = profile;
}

internal sealed class SpriteForgeSettingsView : FrameworkElement, IDisposable
{
    internal const double LogicalWidth = 800;
    internal const double LogicalHeight = 640;
    private const uint ExpectedUiInteropVersion = 1;
    private const uint ElementCapacity = 48;
    private const uint ActionCapacity = 32;

    private static readonly ulong RootKey = Key("spelljammer.settings.root");
    private static readonly ulong ModalKey = Key("spelljammer.settings.modal");
    private static readonly ulong TitleKey = Key("spelljammer.settings.title");
    private static readonly ulong IntroductionKey = Key("spelljammer.settings.introduction");
    private static readonly ulong AudioHeadingKey = Key("spelljammer.settings.audio-heading");
    private static readonly ulong AccessibilityHeadingKey = Key("spelljammer.settings.accessibility-heading");
    private static readonly ulong MasterLabelKey = Key("spelljammer.settings.master-label");
    private static readonly ulong MasterSliderKey = Key("spelljammer.settings.master-slider");
    private static readonly ulong MasterValueKey = Key("spelljammer.settings.master-value");
    private static readonly ulong MusicLabelKey = Key("spelljammer.settings.music-label");
    private static readonly ulong MusicSliderKey = Key("spelljammer.settings.music-slider");
    private static readonly ulong MusicValueKey = Key("spelljammer.settings.music-value");
    private static readonly ulong EffectsLabelKey = Key("spelljammer.settings.effects-label");
    private static readonly ulong EffectsSliderKey = Key("spelljammer.settings.effects-slider");
    private static readonly ulong EffectsValueKey = Key("spelljammer.settings.effects-value");
    private static readonly ulong SubtitlesLabelKey = Key("spelljammer.settings.subtitles-label");
    private static readonly ulong SubtitlesToggleKey = Key("spelljammer.settings.subtitles-toggle");
    private static readonly ulong MotionLabelKey = Key("spelljammer.settings.motion-label");
    private static readonly ulong MotionToggleKey = Key("spelljammer.settings.motion-toggle");
    private static readonly ulong ShakeLabelKey = Key("spelljammer.settings.shake-label");
    private static readonly ulong ShakeToggleKey = Key("spelljammer.settings.shake-toggle");
    private static readonly ulong ScaleLabelKey = Key("spelljammer.settings.scale-label");
    private static readonly ulong ScaleSliderKey = Key("spelljammer.settings.scale-slider");
    private static readonly ulong ScaleValueKey = Key("spelljammer.settings.scale-value");
    private static readonly ulong StatusKey = Key("spelljammer.settings.status");
    private static readonly ulong ResetButtonKey = Key("spelljammer.settings.reset");
    private static readonly ulong CancelButtonKey = Key("spelljammer.settings.cancel");
    private static readonly ulong ApplyButtonKey = Key("spelljammer.settings.apply");
    private static readonly ulong CancelAction = Key("spelljammer.settings.action.cancel");

    private readonly ulong[] elementKeys;
    private readonly Dictionary<ulong, EngineUiElementSnapshot> snapshots = [];
    private readonly EngineUiPresentationCommand[] presentation = new EngineUiPresentationCommand[ElementCapacity];
    private readonly EngineUiAction[] actions = new EngineUiAction[ActionCapacity];
    private readonly GameSettingsStrings strings;
    private nint context;
    private ulong document;
    private ulong inputSequence;
    private GameSettingsProfile draft;
    private string status;
    private bool statusIsError;
    private bool disposed;

    internal SpriteForgeSettingsView(GameSettingsProfile initial, GameSettingsStrings strings)
    {
        draft = initial;
        this.strings = strings;
        status = strings.Get("settings.status.ready");
        elementKeys =
        [
            ModalKey, TitleKey, IntroductionKey, AudioHeadingKey, AccessibilityHeadingKey,
            MasterLabelKey, MasterSliderKey, MasterValueKey,
            MusicLabelKey, MusicSliderKey, MusicValueKey,
            EffectsLabelKey, EffectsSliderKey, EffectsValueKey,
            SubtitlesLabelKey, SubtitlesToggleKey,
            MotionLabelKey, MotionToggleKey,
            ShakeLabelKey, ShakeToggleKey,
            ScaleLabelKey, ScaleSliderKey, ScaleValueKey,
            StatusKey, ResetButtonKey, CancelButtonKey, ApplyButtonKey,
        ];
        Focusable = true;
        SnapsToDevicePixels = true;
        CreateNativeDocument();
        Loaded += View_Loaded;
        Unloaded += View_Unloaded;
    }

    internal event EventHandler<GameSettingsApplyRequestedEventArgs>? ApplyRequested;
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
        EngineStatus presentationStatus = SpriteForgeNative.SpriteForge_UIBuildPresentation(
            context,
            document,
            presentation,
            (uint)presentation.Length,
            out uint commandCount);
        ThrowIfFailed(presentationStatus, "build the settings presentation");
        for (int index = 0; index < commandCount; index++)
        {
            EngineUiPresentationCommand command = presentation[index];
            drawingContext.DrawRectangle(ToBrush(command.Color), null, Scale(command.X, command.Y, command.Width, command.Height));
        }

        DrawSlider(drawingContext, MasterSliderKey, draft.MasterVolume, GameSettingsProfile.MinimumVolume, GameSettingsProfile.MaximumVolume);
        DrawSlider(drawingContext, MusicSliderKey, draft.MusicVolume, GameSettingsProfile.MinimumVolume, GameSettingsProfile.MaximumVolume);
        DrawSlider(drawingContext, EffectsSliderKey, draft.EffectsVolume, GameSettingsProfile.MinimumVolume, GameSettingsProfile.MaximumVolume);
        DrawSlider(drawingContext, ScaleSliderKey, draft.UiScalePercent,
            GameSettingsProfile.MinimumUiScalePercent, GameSettingsProfile.MaximumUiScalePercent);
        DrawToggle(drawingContext, SubtitlesToggleKey, draft.Subtitles);
        DrawToggle(drawingContext, MotionToggleKey, draft.ReducedMotion);
        DrawToggle(drawingContext, ShakeToggleKey, draft.ScreenShake);

        strings.BeginFrame();
        DrawText(drawingContext, TitleKey, strings.Get("settings.title"), 28, "#F2E9D8", false);
        DrawText(drawingContext, IntroductionKey, strings.Get("settings.introduction"), 13, "#93A1BE", false);
        DrawText(drawingContext, AudioHeadingKey, strings.Get("settings.heading.audio"), 14, "#D7AF70", false);
        DrawText(drawingContext, AccessibilityHeadingKey, strings.Get("settings.heading.accessibility"), 14, "#D7AF70", false);
        DrawText(drawingContext, MasterLabelKey, strings.Get("settings.label.master-volume"), 15, "#F2E9D8", false);
        DrawText(drawingContext, MasterValueKey, strings.Percent(draft.MasterVolume), 14, "#80DED9", true);
        DrawText(drawingContext, MusicLabelKey, strings.Get("settings.label.music-volume"), 15, "#F2E9D8", false);
        DrawText(drawingContext, MusicValueKey, strings.Percent(draft.MusicVolume), 14, "#80DED9", true);
        DrawText(drawingContext, EffectsLabelKey, strings.Get("settings.label.effects-volume"), 15, "#F2E9D8", false);
        DrawText(drawingContext, EffectsValueKey, strings.Percent(draft.EffectsVolume), 14, "#80DED9", true);
        DrawText(drawingContext, SubtitlesLabelKey, strings.Get("settings.label.subtitles"), 15, "#F2E9D8", false);
        DrawText(drawingContext, SubtitlesToggleKey, ToggleText(draft.Subtitles), 13, "#F2E9D8", true);
        DrawText(drawingContext, MotionLabelKey, strings.Get("settings.label.reduced-motion"), 15, "#F2E9D8", false);
        DrawText(drawingContext, MotionToggleKey, ToggleText(draft.ReducedMotion), 13, "#F2E9D8", true);
        DrawText(drawingContext, ShakeLabelKey, strings.Get("settings.label.screen-shake"), 15, "#F2E9D8", false);
        DrawText(drawingContext, ShakeToggleKey, ToggleText(draft.ScreenShake), 13, "#F2E9D8", true);
        DrawText(drawingContext, ScaleLabelKey, strings.Get("settings.label.interface-scale"), 15, "#F2E9D8", false);
        DrawText(drawingContext, ScaleValueKey, strings.Percent(draft.UiScalePercent), 14, "#80DED9", true);
        DrawText(drawingContext, StatusKey, status, 12, statusIsError ? "#F39A8D" : "#93A1BE", false);
        DrawText(drawingContext, ResetButtonKey, strings.Get("settings.button.reset"), 13, "#F2E9D8", true);
        DrawText(drawingContext, CancelButtonKey, strings.Get("settings.button.cancel"), 13, "#F2E9D8", true);
        DrawText(drawingContext, ApplyButtonKey, strings.Get("settings.button.apply"), 13, "#F2E9D8", true);

        foreach (EngineUiElementSnapshot snapshot in snapshots.Values.Where(value => value.Focused != 0))
        {
            Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
            drawingContext.DrawRectangle(null, new Pen(Brush("#80DED9"), 2), bounds);
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
            System.Windows.Input.Key.Tab when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => EngineUiNavigation.Previous,
            System.Windows.Input.Key.Tab => EngineUiNavigation.Next,
            System.Windows.Input.Key.Left => EngineUiNavigation.Left,
            System.Windows.Input.Key.Right => EngineUiNavigation.Right,
            System.Windows.Input.Key.Up => EngineUiNavigation.Up,
            System.Windows.Input.Key.Down => EngineUiNavigation.Down,
            System.Windows.Input.Key.Enter or System.Windows.Input.Key.Space => EngineUiNavigation.Accept,
            System.Windows.Input.Key.Escape => EngineUiNavigation.Cancel,
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

    internal void SetApplyFailure(GameSettingsDiagnostic diagnostic)
    {
        status = strings.Diagnostic(
            "settings.status.save-failed",
            GameSettingsDiagnostics.Stable(diagnostic));
        statusIsError = true;
        IsEnabled = true;
        Focus();
        InvalidateVisual();
    }

    internal void SetBusy()
    {
        status = strings.Get("settings.status.saving");
        statusIsError = false;
        IsEnabled = false;
        InvalidateVisual();
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
        ThrowIfFailed(SpriteForgeNative.SpriteForge_CreateUIContext(in description, out context, out document),
            "create the settings UI document");

        List<nint> allocatedNames = [];
        try
        {
            EngineUiElementDescription[] elements = BuildElements(allocatedNames);
            ThrowIfFailed(SpriteForgeNative.SpriteForge_UIAddElements(context, document, elements, (uint)elements.Length),
                "commit the settings UI document");
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

    private EngineUiElementDescription[] BuildElements(List<nint> allocatedNames) =>
    [
        Element(ModalKey, RootKey, 20, 20, 760, 600, EngineUiBehavior.None,
            strings.Get("settings.accessibility.dialog"), allocatedNames, modal: true, dismissAction: CancelAction),
        TextElement(TitleKey, 52, 42, 680, 40, strings.Get("settings.title"), allocatedNames),
        TextElement(IntroductionKey, 52, 80, 680, 30,
            strings.Get("settings.accessibility.description"), allocatedNames),
        TextElement(AudioHeadingKey, 52, 115, 680, 24, strings.Get("settings.heading.audio"), allocatedNames),
        TextElement(MasterLabelKey, 58, 148, 260, 28,
            strings.Get("settings.label.master-volume"), allocatedNames),
        Slider(MasterSliderKey, 350, 150, draft.MasterVolume, 0, 100, 5, 0,
            strings.Get("settings.label.master-volume"), allocatedNames),
        TextElement(MasterValueKey, 662, 146, 72, 28,
            AccessibleValue("settings.label.master-volume"), allocatedNames),
        TextElement(MusicLabelKey, 58, 193, 260, 28,
            strings.Get("settings.label.music-volume"), allocatedNames),
        Slider(MusicSliderKey, 350, 195, draft.MusicVolume, 0, 100, 5, 1,
            strings.Get("settings.label.music-volume"), allocatedNames),
        TextElement(MusicValueKey, 662, 191, 72, 28,
            AccessibleValue("settings.label.music-volume"), allocatedNames),
        TextElement(EffectsLabelKey, 58, 238, 260, 28,
            strings.Get("settings.label.effects-volume"), allocatedNames),
        Slider(EffectsSliderKey, 350, 240, draft.EffectsVolume, 0, 100, 5, 2,
            strings.Get("settings.label.effects-volume"), allocatedNames),
        TextElement(EffectsValueKey, 662, 236, 72, 28,
            AccessibleValue("settings.label.effects-volume"), allocatedNames),
        TextElement(AccessibilityHeadingKey, 52, 285, 680, 24,
            strings.Get("settings.heading.accessibility"), allocatedNames),
        TextElement(SubtitlesLabelKey, 58, 320, 350, 32,
            strings.Get("settings.label.subtitles"), allocatedNames),
        Toggle(SubtitlesToggleKey, 610, 316, draft.Subtitles, 3,
            strings.Get("settings.label.subtitles"), allocatedNames),
        TextElement(MotionLabelKey, 58, 365, 350, 32,
            strings.Get("settings.label.reduced-motion"), allocatedNames),
        Toggle(MotionToggleKey, 610, 361, draft.ReducedMotion, 4,
            strings.Get("settings.label.reduced-motion"), allocatedNames),
        TextElement(ShakeLabelKey, 58, 410, 350, 32,
            strings.Get("settings.label.screen-shake"), allocatedNames),
        Toggle(ShakeToggleKey, 610, 406, draft.ScreenShake, 5,
            strings.Get("settings.label.screen-shake"), allocatedNames),
        TextElement(ScaleLabelKey, 58, 458, 260, 28,
            strings.Get("settings.label.interface-scale"), allocatedNames),
        Slider(ScaleSliderKey, 350, 460, draft.UiScalePercent, 75, 150, 5, 6,
            strings.Get("settings.label.interface-scale"), allocatedNames),
        TextElement(ScaleValueKey, 662, 456, 72, 28,
            AccessibleValue("settings.label.interface-scale"), allocatedNames),
        TextElement(StatusKey, 52, 505, 680, 28,
            strings.Get("settings.accessibility.status"), allocatedNames),
        Button(ResetButtonKey, 342, 550, 112, 42, 7,
            strings.Get("settings.button.reset"), allocatedNames),
        Button(CancelButtonKey, 470, 550, 112, 42, 8,
            strings.Get("settings.button.cancel"), allocatedNames),
        Button(ApplyButtonKey, 598, 550, 136, 42, 9,
            strings.Get("settings.button.apply"), allocatedNames),
    ];

    private string AccessibleValue(string settingKey) => strings.Format(
        "settings.accessibility.value",
        Spelljammer.Localization.LocalizationArgument.Text("setting", strings.Get(settingKey)));

    private static EngineUiElementDescription TextElement(
        ulong key, float x, float y, float width, float height, string name, List<nint> names) =>
        Element(key, ModalKey, x, y, width, height, EngineUiBehavior.None, name, names,
            kind: EngineUiElementKind.Text, customColor: true, color: Color(0, 0, 0, 0));

    private static EngineUiElementDescription Button(
        ulong key, float x, float y, float width, float height, int tabOrder, string name, List<nint> names) =>
        Element(key, ModalKey, x, y, width, height, EngineUiBehavior.Button, name, names, tabOrder: tabOrder);

    private static EngineUiElementDescription Toggle(
        ulong key, float x, float y, bool value, int tabOrder, string name, List<nint> names) =>
        Element(key, ModalKey, x, y, 124, 36, EngineUiBehavior.Toggle, name, names,
            tabOrder: tabOrder, toggle: value);

    private static EngineUiElementDescription Slider(
        ulong key, float x, float y, float value, float minimum, float maximum, float step,
        int tabOrder, string name, List<nint> names) =>
        Element(key, ModalKey, x, y, 290, 24, EngineUiBehavior.Slider, name, names,
            tabOrder: tabOrder, sliderMinimum: minimum, sliderMaximum: maximum, sliderValue: value, sliderStep: step);

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
        bool toggle = false,
        float sliderMinimum = 0,
        float sliderMaximum = 1,
        float sliderValue = 0,
        float sliderStep = 0.1f,
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
            SliderMinimum = sliderMinimum,
            SliderMaximum = sliderMaximum,
            SliderValue = sliderValue,
            SliderStep = sliderStep,
            TabOrder = tabOrder,
            Kind = kind,
            Behavior = behavior,
            ToggleChecked = toggle ? 1u : 0u,
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
            InsideViewport = physical.X >= 0 && physical.Y >= 0 && physical.X < ActualWidth && physical.Y < ActualHeight ? 1u : 0u,
        }]);
    }

    private void Process(EngineUiInput[] input)
    {
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIProcessInput(context, document, input, (uint)input.Length),
            "process settings input");
        ConsumeActions();
        InvalidateVisual();
    }

    private void ConsumeActions()
    {
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIConsumeActions(
            context, document, actions, (uint)actions.Length, out uint actionCount), "consume settings actions");
        for (int index = 0; index < actionCount; index++)
        {
            EngineUiAction action = actions[index];
            if (action.Source == MasterSliderKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Scalar);
                draft = draft with { MasterVolume = (int)MathF.Round(action.ScalarValue) };
            }
            else if (action.Source == MusicSliderKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Scalar);
                draft = draft with { MusicVolume = (int)MathF.Round(action.ScalarValue) };
            }
            else if (action.Source == EffectsSliderKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Scalar);
                draft = draft with { EffectsVolume = (int)MathF.Round(action.ScalarValue) };
            }
            else if (action.Source == ScaleSliderKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Scalar);
                draft = draft with { UiScalePercent = (int)MathF.Round(action.ScalarValue) };
            }
            else if (action.Source == SubtitlesToggleKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Boolean);
                draft = draft with { Subtitles = action.BooleanValue != 0 };
            }
            else if (action.Source == MotionToggleKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Boolean);
                draft = draft with { ReducedMotion = action.BooleanValue != 0 };
            }
            else if (action.Source == ShakeToggleKey)
            {
                RequireActionValue(action, EngineUiActionValueType.Boolean);
                draft = draft with { ScreenShake = action.BooleanValue != 0 };
            }
            else if (action.Source == ResetButtonKey)
            {
                Recreate(GameSettingsProfile.Default);
            }
            else if (action.Source == ApplyButtonKey)
            {
                ApplyRequested?.Invoke(this, new GameSettingsApplyRequestedEventArgs(draft));
            }
            else if (action.Source == CancelButtonKey || action.Type == CancelAction)
            {
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Recreate(GameSettingsProfile profile)
    {
        draft = profile;
        status = strings.Get("settings.status.reset");
        statusIsError = false;
        DestroyNativeDocument();
        CreateNativeDocument();
    }

    private void RefreshSnapshots()
    {
        EngineUiElementSnapshot[] values = new EngineUiElementSnapshot[elementKeys.Length];
        ThrowIfFailed(SpriteForgeNative.SpriteForge_UIGetElementSnapshots(
            context, document, elementKeys, (uint)elementKeys.Length, values, (uint)values.Length, out uint count),
            "copy settings element snapshots");
        snapshots.Clear();
        for (int index = 0; index < count; index++)
        {
            snapshots.Add(values[index].Key, values[index]);
        }
    }

    private void DrawSlider(DrawingContext drawingContext, ulong key, int value, int minimum, int maximum)
    {
        if (!snapshots.TryGetValue(key, out EngineUiElementSnapshot snapshot))
        {
            return;
        }

        Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
        double normalized = (double)(value - minimum) / (maximum - minimum);
        Rect track = new(bounds.X + 8, bounds.Y + bounds.Height / 2 - 2, bounds.Width - 16, 4);
        drawingContext.DrawRectangle(Brush("#101827"), null, track);
        drawingContext.DrawRectangle(Brush("#80DED9"), null,
            new Rect(track.X, track.Y, track.Width * normalized, track.Height));
        double knobX = track.X + track.Width * normalized;
        drawingContext.DrawEllipse(Brush("#F2E9D8"), null, new Point(knobX, bounds.Y + bounds.Height / 2), 6, 6);
    }

    private void DrawToggle(DrawingContext drawingContext, ulong key, bool enabled)
    {
        if (!snapshots.TryGetValue(key, out EngineUiElementSnapshot snapshot))
        {
            return;
        }

        Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
        Rect indicator = new(bounds.X + 10, bounds.Y + bounds.Height / 2 - 6, 12, 12);
        drawingContext.DrawRectangle(enabled ? Brush("#80DED9") : Brush("#101827"), null, indicator);
    }

    private void DrawText(DrawingContext drawingContext, ulong key, string text, double fontSize, string color, bool centered)
    {
        if (!snapshots.TryGetValue(key, out EngineUiElementSnapshot snapshot))
        {
            return;
        }

        FormattedText formatted = new(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(centered ? "Consolas" : "Segoe UI"),
            fontSize * ActualHeight / LogicalHeight,
            Brush(color),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1, snapshot.Width * ActualWidth / LogicalWidth),
            Trimming = TextTrimming.CharacterEllipsis,
        };
        Rect bounds = Scale(snapshot.X, snapshot.Y, snapshot.Width, snapshot.Height);
        double x = centered ? bounds.X + Math.Max(0, (bounds.Width - formatted.Width) / 2) : bounds.X;
        double y = bounds.Y + Math.Max(0, (bounds.Height - formatted.Height) / 2);
        drawingContext.DrawText(formatted, new Point(x, y));
    }

    private Rect Scale(float x, float y, float width, float height) => new(
        x * ActualWidth / LogicalWidth,
        y * ActualHeight / LogicalHeight,
        width * ActualWidth / LogicalWidth,
        height * ActualHeight / LogicalHeight);

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

    private string ToggleText(bool enabled) => strings.Get(enabled ? "settings.state.on" : "settings.state.off");

    private static ulong Key(string value)
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
            throw new InvalidOperationException($"SpriteForge.dll could not {operation} ({status}, {(int)status}).");
        }
    }

    private static void RequireActionValue(EngineUiAction action, EngineUiActionValueType expected)
    {
        if (action.ValueType != expected)
        {
            throw new InvalidOperationException(
                $"SpriteForge returned {action.ValueType} for settings action {action.Source}; expected {expected}.");
        }
    }
}
