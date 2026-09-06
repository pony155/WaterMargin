using System.Runtime.InteropServices;

namespace Spelljammer.Interop;

internal enum EngineStatus
{
    Success = 0,
    Failure = -1,
    InvalidArgument = -2,
    OutOfResource = -3,
    OutOfMemory = -4,
    InvalidResource = -5,
    NotSupported = -10,
    InvalidState = -11,
    DeviceLost = -14,
    BackendUnavailable = -15,
    InitializationFailed = -16,
    SkipFrame = -20,
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineCamera2D
{
    internal float PositionX;
    internal float PositionY;
    internal float RotationRadians;
    internal float PixelsPerWorldUnit;
    internal uint IntegerZoom;
    internal uint PixelPerfect;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineSpriteDraw
{
    internal uint Texture;
    internal uint SourceX;
    internal uint SourceY;
    internal uint SourceWidth;
    internal uint SourceHeight;
    internal int PivotX;
    internal int PivotY;
    internal uint UntrimmedWidth;
    internal uint UntrimmedHeight;
    internal float PositionX;
    internal float PositionY;
    internal float ScaleX;
    internal float ScaleY;
    internal float RotationRadians;
    internal float ColorR;
    internal float ColorG;
    internal float ColorB;
    internal float ColorA;
    internal int Layer;
    internal int Order;
    internal uint FlipX;
    internal uint FlipY;
    internal uint PixelSnap;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiColor
{
    internal float Red;
    internal float Green;
    internal float Blue;
    internal float Alpha;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiTheme
{
    internal EngineUiColor Panel;
    internal EngineUiColor Button;
    internal EngineUiColor ButtonHovered;
    internal EngineUiColor ButtonPressed;
    internal EngineUiColor ButtonFocused;
    internal EngineUiColor ButtonDisabled;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiDocumentDescription
{
    internal ulong RootKey;
    internal uint LogicalWidth;
    internal uint LogicalHeight;
    internal uint MaximumElements;
    internal uint MaximumActions;
    internal EngineUiTheme Theme;
}

internal enum EngineUiElementKind : uint
{
    Container,
    Text,
}

internal enum EngineUiBehavior : uint
{
    None,
    Button,
    Toggle,
    Slider,
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiElementDescription
{
    internal ulong Key;
    internal ulong ParentKey;
    internal ulong Action;
    internal ulong DismissAction;
    internal float X;
    internal float Y;
    internal float Width;
    internal float Height;
    internal float SliderMinimum;
    internal float SliderMaximum;
    internal float SliderValue;
    internal float SliderStep;
    internal int TabOrder;
    internal EngineUiElementKind Kind;
    internal EngineUiBehavior Behavior;
    internal uint ToggleChecked;
    internal uint Visible;
    internal uint Enabled;
    internal uint HitTestable;
    internal uint Modal;
    internal uint Focusable;
    internal uint CustomColor;
    internal EngineUiColor Color;
    internal nint AccessibleNameUtf8;
    internal uint AccessibleNameBytes;
}

internal enum EngineUiInputType : uint
{
    PointerMoved,
    PointerDown,
    PointerUp,
    Navigation,
    Cancel,
}

internal enum EngineUiNavigation : uint
{
    None,
    Next,
    Previous,
    Left,
    Right,
    Up,
    Down,
    Accept,
    Cancel,
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiInput
{
    internal EngineUiInputType Type;
    internal EngineUiNavigation Navigation;
    internal float X;
    internal float Y;
    internal ulong Sequence;
    internal uint PointerId;
    internal uint InsideViewport;
}

internal enum EngineUiActionValueType : uint
{
    None,
    Boolean,
    Scalar,
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiAction
{
    internal ulong Type;
    internal ulong Source;
    internal ulong InteractionSequence;
    internal EngineUiActionValueType ValueType;
    internal float ScalarValue;
    internal uint BooleanValue;
    internal uint Preview;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiElementSnapshot
{
    internal ulong Key;
    internal float X;
    internal float Y;
    internal float Width;
    internal float Height;
    internal uint Visible;
    internal uint Enabled;
    internal uint Focused;
    internal uint Selected;
    internal uint ToggleChecked;
    internal float SliderValue;
}

[StructLayout(LayoutKind.Sequential)]
internal struct EngineUiPresentationCommand
{
    internal ulong Source;
    internal float X;
    internal float Y;
    internal float Width;
    internal float Height;
    internal EngineUiColor Color;
    internal int Layer;
    internal int Order;
}

internal static class SpriteForgeNative
{
    private const string LibraryName = "SpriteForge.dll";

    static SpriteForgeNative()
    {
        VerifyLayout<EngineUiDocumentDescription>(120);
        VerifyLayout<EngineUiElementDescription>(136);
        VerifyLayout<EngineUiInput>(32);
        VerifyLayout<EngineUiAction>(40);
        VerifyLayout<EngineUiElementSnapshot>(48);
        VerifyLayout<EngineUiPresentationCommand>(48);
    }

    private static void VerifyLayout<T>(int expected) where T : struct
    {
        int actual = Marshal.SizeOf<T>();
        if (actual != expected)
        {
            throw new TypeLoadException(
                $"SpriteForge UI interop layout '{typeof(T).Name}' is {actual} bytes; expected {expected}.");
        }
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint SpriteForge_GetInteropVersion();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint SpriteForge_GetUIInteropVersion();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_CreateRenderer(
        nint nativeWindow,
        uint logicalWidth,
        uint logicalHeight,
        uint maxSprites,
        uint verticalSync,
        out nint renderer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void SpriteForge_DestroyRenderer(nint renderer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_CreateRgba8Texture(
        nint renderer,
        uint width,
        uint height,
        byte[] pixels,
        nuint sizeBytes,
        out uint texture);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_DestroyTexture(
        nint renderer,
        uint texture);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_RenderSprites(
        nint renderer,
        in EngineCamera2D camera,
        [In] EngineSpriteDraw[] draws,
        uint drawCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_CreateUIContext(
        in EngineUiDocumentDescription description,
        out nint context,
        out ulong document);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void SpriteForge_DestroyUIContext(nint context);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_UIAddElements(
        nint context,
        ulong document,
        [In] EngineUiElementDescription[] elements,
        uint elementCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_UIProcessInput(
        nint context,
        ulong document,
        [In] EngineUiInput[] inputs,
        uint inputCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_UIConsumeActions(
        nint context,
        ulong document,
        [Out] EngineUiAction[] actions,
        uint capacity,
        out uint actionCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_UIGetElementSnapshots(
        nint context,
        ulong document,
        [In] ulong[] keys,
        uint keyCount,
        [Out] EngineUiElementSnapshot[] snapshots,
        uint capacity,
        out uint snapshotCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_UIBuildPresentation(
        nint context,
        ulong document,
        [Out] EngineUiPresentationCommand[] commands,
        uint capacity,
        out uint commandCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern EngineStatus SpriteForge_UICancelInput(
        nint context,
        ulong document);
}
