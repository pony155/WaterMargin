using System.Runtime.InteropServices;

namespace WaterMargin.Interop;

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

internal static class SpriteForgeNative
{
    private const string LibraryName = "SpriteForge.dll";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint SpriteForge_GetInteropVersion();

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
}
