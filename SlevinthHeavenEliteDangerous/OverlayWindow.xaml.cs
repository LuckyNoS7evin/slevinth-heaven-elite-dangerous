using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace SlevinthHeavenEliteDangerous;

/// <summary>
/// Full-screen transparent overlay window with mouse click-through.
///
/// Transparency : DwmExtendFrameIntoClientArea(-1) + DwmEnableBlurBehindWindow
///                + TransparentBackdrop + CompositionColorBrush(alpha=0).
/// Click-through: WS_EX_LAYERED | WS_EX_TRANSPARENT + SetLayeredWindowAttributes(alpha=255).
///   With WS_EX_LAYERED the OS composites the whole window (including WinUI 3 child HWNDs)
///   as a single layered entity; WS_EX_TRANSPARENT then routes ALL input to the window below.
///   alpha=255 keeps HUD content fully opaque — the DWM glass handles background transparency.
/// </summary>
public sealed partial class OverlayWindow : Window
{
    #region Win32 interop

    private const int GWL_STYLE          = -16;
    private const int GWL_EXSTYLE        = -20;
    private const int WS_CAPTION         = 0x00C00000;
    private const int WS_THICKFRAME      = 0x00040000;
    private const int WS_EX_LAYERED      = 0x00080000;
    private const int WS_EX_TRANSPARENT  = 0x00000020;
    private const int WS_EX_NOACTIVATE   = 0x08000000;
    private const uint LWA_ALPHA         = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWM_BLURBEHIND
    {
        public uint   dwFlags;
        public bool   fEnable;
        public IntPtr hRgnBlur;
        public bool   fTransitionOnMaximized;
    }
    private const uint DWM_BB_ENABLE = 0x00000001;

    [DllImport("user32.dll")] private static extern int  GetWindowLong(IntPtr hwnd, int nIndex);
    [DllImport("user32.dll")] private static extern int  SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("dwmapi.dll")] private static extern int  DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);
    [DllImport("dwmapi.dll")] private static extern int  DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND pBlurBehind);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE       = 0x0002;
    private const uint SWP_NOSIZE       = 0x0001;
    private const uint SWP_NOACTIVATE   = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;

    #endregion

    private readonly IntPtr _hwnd;
    private readonly AppWindow _appWindow;

    public OverlayWindow()
    {
        InitializeComponent();

        // Prevents WinUI 3 from painting its default theme-coloured host backdrop
        SystemBackdrop = new TransparentBackdrop();

        _hwnd = WindowNative.GetWindowHandle(this);
        _appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd));

        ConfigureWindow();
        Activated += OnFirstActivated;
    }

    private void ConfigureWindow()
    {
        _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        _appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;

        var style = GetWindowLong(_hwnd, GWL_STYLE);
        SetWindowLong(_hwnd, GWL_STYLE, style & ~WS_CAPTION & ~WS_THICKFRAME);

        var displayArea = DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd), DisplayAreaFallback.Primary);
        _appWindow.MoveAndResize(displayArea.OuterBounds);

        // WS_EX_LAYERED: the OS composites the entire window (all child HWNDs included) as
        //   one layered surface — WS_EX_TRANSPARENT then routes ALL pointer input to the
        //   window below without child HWNDs competing for hits.
        // alpha=255: window is fully opaque at the layered level; DWM glass supplies the
        //   background transparency so the HUD remains solid.
        var exStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
        SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);
        SetLayeredWindowAttributes(_hwnd, 0, 255, LWA_ALPHA);

        SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_FRAMECHANGED);
    }

    private bool _compositionReady = false;

    private void OnFirstActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_compositionReady) return;
        _compositionReady = true;

        InitializeTransparentComposition();
    }

    private void InitializeTransparentComposition()
    {
        // DWM glass over entire client area — this is the source of background transparency
        var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
        DwmExtendFrameIntoClientArea(_hwnd, ref margins);

        var blurBehind = new DWM_BLURBEHIND { dwFlags = DWM_BB_ENABLE, fEnable = true };
        DwmEnableBlurBehindWindow(_hwnd, ref blurBehind);

        // Transparent CompositionColorBrush so XAML does not paint a solid background
        if (Content is UIElement root)
        {
            var compositor = ElementCompositionPreview.GetElementVisual(root).Compositor;
            var bgVisual = compositor.CreateSpriteVisual();
            bgVisual.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            bgVisual.RelativeSizeAdjustment = Vector2.One;
            ElementCompositionPreview.SetElementChildVisual(root, bgVisual);
        }
    }

    public void ShowOverlay()
    {
        var displayArea = DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd), DisplayAreaFallback.Primary);
        _appWindow.MoveAndResize(displayArea.OuterBounds);
        Activate();
    }

    public void HideOverlay() => _appWindow.Hide();

    private sealed class TransparentBackdrop : SystemBackdrop { }
}
