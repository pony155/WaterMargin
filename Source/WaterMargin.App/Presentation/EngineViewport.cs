using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;
using WaterMargin.Interop;

namespace WaterMargin.Presentation;

public sealed class EngineViewport : HwndHost
{
    private const int FrameWidth = 16;
    private const int FrameHeight = 16;
    private const int FrameCount = 4;
    private const int SheetWidth = FrameWidth * FrameCount;
    private const int FramesPerSecond = 8;
    private const int RenderIntervalMilliseconds = 16;
    private const uint ExpectedInteropVersion = 1;

    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsClipSiblings = 0x04000000;
    private const int WsClipChildren = 0x02000000;

    private readonly DispatcherTimer renderTimer;
    private nint renderer;
    private uint spriteSheet;
    private int currentFrame;
    private bool isPlaying = true;
    private long animationStartTicks;

    public EngineViewport()
    {
        renderTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(RenderIntervalMilliseconds),
        };
        renderTimer.Tick += RenderTimer_Tick;
    }

    internal event EventHandler<int>? FrameChanged;

    internal bool IsPlaying => isPlaying;

    internal void TogglePlayback()
    {
        isPlaying = !isPlaying;
        if (isPlaying)
        {
            animationStartTicks = Environment.TickCount64 -
                currentFrame * (1000 / FramesPerSecond);
        }
        RenderScene();
    }

    internal void StepFrame()
    {
        isPlaying = false;
        SetFrame((currentFrame + 1) % FrameCount);
        RenderScene();
    }

    internal void Restart()
    {
        isPlaying = true;
        animationStartTicks = Environment.TickCount64;
        SetFrame(0);
        RenderScene();
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        nint childWindow = CreateWindowEx(
            0,
            "STATIC",
            string.Empty,
            WsChild | WsVisible | WsClipSiblings | WsClipChildren,
            0,
            0,
            1,
            1,
            hwndParent.Handle,
            nint.Zero,
            nint.Zero,
            nint.Zero);
        if (childWindow == nint.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            uint interopVersion = SpriteForgeNative.SpriteForge_GetInteropVersion();
            if (interopVersion != ExpectedInteropVersion)
            {
                throw new InvalidOperationException(
                    $"SpriteForge.dll exposes interop version {interopVersion}; " +
                    $"WaterMargin expects {ExpectedInteropVersion}.");
            }

            ThrowIfFailed(
                SpriteForgeNative.SpriteForge_CreateRenderer(
                    childWindow, 320, 180, 16, 1, out renderer),
                "create the renderer");

            byte[] pixels = BuildSpriteSheet();
            ThrowIfFailed(
                SpriteForgeNative.SpriteForge_CreateRgba8Texture(
                    renderer,
                    SheetWidth,
                    FrameHeight,
                    pixels,
                    (nuint)pixels.Length,
                    out spriteSheet),
                "upload the sprite sheet");

            animationStartTicks = Environment.TickCount64;
            renderTimer.Start();
            RenderScene();
            return new HandleRef(this, childWindow);
        }
        catch
        {
            ReleaseEngineResources();
            DestroyWindow(childWindow);
            throw;
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        renderTimer.Stop();
        ReleaseEngineResources();
        if (hwnd.Handle != nint.Zero)
            DestroyWindow(hwnd.Handle);
    }

    private static byte[] BuildSpriteSheet()
    {
        const int bytesPerPixel = 4;
        var pixels = new byte[SheetWidth * FrameHeight * bytesPerPixel];

        for (int frame = 0; frame < FrameCount; ++frame)
        {
            int bob = frame % 2;
            FillFrameRectangle(pixels, frame, 5, 1 + bob, 6, 5, 31, 32, 48);
            FillFrameRectangle(pixels, frame, 6, 2 + bob, 4, 3, 255, 204, 128);
            FillFrameRectangle(pixels, frame, 7, 3 + bob, 1, 1, 31, 32, 48);
            FillFrameRectangle(pixels, frame, 4, 6 + bob, 8, 6, 31, 32, 48);
            FillFrameRectangle(pixels, frame, 5, 7 + bob, 6, 4, 48, 187, 201);
            FillFrameRectangle(pixels, frame, 6, 7 + bob, 2, 1, 126, 232, 224);

            int leftArmX = frame < 2 ? 2 : 12;
            int rightArmX = frame < 2 ? 12 : 2;
            FillFrameRectangle(pixels, frame, leftArmX, 7 + bob, 2, 4, 31, 32, 48);
            FillFrameRectangle(pixels, frame, rightArmX, 8 + bob, 2, 3, 31, 32, 48);

            bool alternateStep = frame is 1 or 2;
            FillFrameRectangle(
                pixels, frame, alternateStep ? 4 : 5, 12 + bob,
                3, 3 - bob, 88, 64, 96);
            FillFrameRectangle(
                pixels, frame, alternateStep ? 9 : 8, 12 + bob,
                3, 3 - bob, 88, 64, 96);
        }
        return pixels;
    }

    private static void FillFrameRectangle(
        byte[] pixels,
        int frame,
        int x,
        int y,
        int width,
        int height,
        byte red,
        byte green,
        byte blue)
    {
        int frameOrigin = frame * FrameWidth;
        for (int row = y; row < y + height; ++row)
        {
            for (int column = x; column < x + width; ++column)
            {
                int offset = (row * SheetWidth + frameOrigin + column) * 4;
                pixels[offset] = red;
                pixels[offset + 1] = green;
                pixels[offset + 2] = blue;
                pixels[offset + 3] = byte.MaxValue;
            }
        }
    }

    private static EngineSpriteDraw CreateDraw(
        uint texture,
        int frame,
        float x,
        float y,
        float scale,
        int order)
    {
        return new EngineSpriteDraw
        {
            Texture = texture,
            SourceX = (uint)(frame * FrameWidth),
            SourceY = 0,
            SourceWidth = FrameWidth,
            SourceHeight = FrameHeight,
            PivotX = FrameWidth / 2,
            PivotY = FrameHeight / 2,
            UntrimmedWidth = FrameWidth,
            UntrimmedHeight = FrameHeight,
            PositionX = x,
            PositionY = y,
            ScaleX = scale,
            ScaleY = scale,
            ColorR = 1,
            ColorG = 1,
            ColorB = 1,
            ColorA = 1,
            Order = order,
            PixelSnap = 1,
        };
    }

    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        if (isPlaying)
        {
            long elapsedMilliseconds = Environment.TickCount64 - animationStartTicks;
            int frame = (int)(elapsedMilliseconds * FramesPerSecond / 1000 % FrameCount);
            SetFrame(frame);
        }
        RenderScene();
    }

    private void SetFrame(int frame)
    {
        if (currentFrame == frame)
            return;
        currentFrame = frame;
        FrameChanged?.Invoke(this, currentFrame);
    }

    private void RenderScene()
    {
        if (renderer == nint.Zero || spriteSheet == 0)
            return;

        var camera = new EngineCamera2D
        {
            PixelsPerWorldUnit = 1,
            IntegerZoom = 1,
            PixelPerfect = 1,
        };
        EngineSpriteDraw[] draws =
        [
            CreateDraw(spriteSheet, 0, -96, -54, 3, 0),
            CreateDraw(spriteSheet, 1, -32, -54, 3, 1),
            CreateDraw(spriteSheet, 2, 32, -54, 3, 2),
            CreateDraw(spriteSheet, 3, 96, -54, 3, 3),
            CreateDraw(spriteSheet, currentFrame, 0, 35, 7, 4),
        ];

        EngineStatus status = SpriteForgeNative.SpriteForge_RenderSprites(
            renderer, in camera, draws, (uint)draws.Length);
        if (status is not EngineStatus.Success and not EngineStatus.SkipFrame)
        {
            renderTimer.Stop();
            throw new InvalidOperationException(
                $"SpriteForge.dll failed to render the scene ({status}, {(int)status}).");
        }
    }

    private void ReleaseEngineResources()
    {
        if (renderer == nint.Zero)
            return;
        if (spriteSheet != 0)
        {
            SpriteForgeNative.SpriteForge_DestroyTexture(renderer, spriteSheet);
            spriteSheet = 0;
        }
        SpriteForgeNative.SpriteForge_DestroyRenderer(renderer);
        renderer = nint.Zero;
    }

    private static void ThrowIfFailed(EngineStatus status, string operation)
    {
        if (status != EngineStatus.Success)
        {
            throw new InvalidOperationException(
                $"SpriteForge.dll could not {operation} ({status}, {(int)status}).");
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateWindowEx(
        int extendedStyle,
        string className,
        string windowName,
        int style,
        int x,
        int y,
        int width,
        int height,
        nint parent,
        nint menu,
        nint instance,
        nint parameter);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint window);
}
