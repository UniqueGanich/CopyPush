using System.Windows.Interop;
using CopyPush.Interop;

namespace CopyPush.Hooks;

/// <summary>
/// Скрытое окно-приёмник сообщений. Обеспечивает:
///  - WM_CLIPBOARDUPDATE  (изменение буфера обмена)
///  - WM_SETTINGCHANGE / WM_THEMECHANGED / WM_DWMCOLORIZATIONCOLORCHANGED (смена темы)
/// Также служит якорем для низкоуровневого хука (его callback приходит в поток с message pump).
/// </summary>
internal sealed class MessageWindow : IDisposable
{
    private readonly HwndSource _source;
    private bool _disposed;

    public MessageWindow()
    {
        var parameters = new HwndSourceParameters("CopyPushMessageWindow")
        {
            Width = 0,
            Height = 0,
            PositionX = 0,
            PositionY = 0,
            WindowStyle = 0,
            ExtendedWindowStyle = 0,
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    public IntPtr Handle => _source.Handle;

    /// <summary>Буфер обмена изменился.</summary>
    public event Action? ClipboardChanged;

    /// <summary>Системная тема/акцент изменились.</summary>
    public event Action? ThemeChanged;

    public bool StartClipboardListener()
    {
        return NativeMethods.AddClipboardFormatListener(Handle);
    }

    public void StopClipboardListener()
    {
        NativeMethods.RemoveClipboardFormatListener(Handle);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case NativeMethods.WM_CLIPBOARDUPDATE:
                ClipboardChanged?.Invoke();
                break;

            case NativeMethods.WM_SETTINGCHANGE:
            case NativeMethods.WM_THEMECHANGED:
            case NativeMethods.WM_DWMCOLORIZATIONCOLORCHANGED:
                ThemeChanged?.Invoke();
                break;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopClipboardListener();
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }
}
