using System.Runtime.InteropServices;
using CopyPush.Interop;

namespace CopyPush.Hooks;

/// <summary>
/// Глобальный низкоуровневый хук клавиатуры. Только НАБЛЮДЕНИЕ:
/// обработчик всегда возвращает CallNextHookEx, поэтому Ctrl+C продолжает работать
/// во всех приложениях как обычно.
/// </summary>
internal sealed class KeyboardHook : IDisposable
{
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private readonly Action _onCtrlC;
    private IntPtr _hook = IntPtr.Zero;
    private bool _disposed;

    public KeyboardHook(Action onCtrlC)
    {
        _onCtrlC = onCtrlC;
        _proc = HookCallback;
    }

    public bool IsInstalled => _hook != IntPtr.Zero;

    public void Install()
    {
        if (_hook != IntPtr.Zero) return;

        _hook = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            _proc,
            NativeMethods.GetModuleHandle(null),
            0);

        if (_hook == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                $"SetWindowsHookEx(WH_KEYBOARD_LL) failed, Win32={Marshal.GetLastWin32Error()}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode == NativeMethods.HC_ACTION)
            {
                int msg = wParam.ToInt32();
                if (msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN)
                {
                    var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                    if (data.vkCode == NativeMethods.VK_C &&
                        (NativeMethods.GetAsyncKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
                    {
                        _onCtrlC();
                    }                }
            }
        }
        catch
        {
            // Хук не должен бросать исключений: любое исключение внутри callback
            // может привести к тому, что Windows отключит хук.
        }

        return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
    }
}
