using System.Windows;
using CopyPush.Interop;
using CopyPush.Views;

namespace CopyPush.Services;

/// <summary>
/// Показывает плашки и следит за стеком (несколько быстрых Ctrl+C подряд
/// не накладываются друг на друга).
/// </summary>
internal sealed class ToastService
{
    /// <summary>
    /// Минимальный интервал между плашками. Некоторые приложения при копировании
    /// присылают несколько WM_CLIPBOARDUPDATE подряд, а режим "реагировать на любое
    /// изменение буфера" добавляет ещё. Без этого ограничения на экране появлялась бы
    /// груда одинаковых плашек.
    /// </summary>
    private const int DebounceMs = 350;

    private readonly ThemeService _theme;
    private readonly SettingsService _settings;
    private readonly List<ToastWindow> _active = new();
    private DateTime _lastShownUtc = DateTime.MinValue;

    public ToastService(ThemeService theme, SettingsService settings)
    {
        _theme = theme;
        _settings = settings;
    }

    public void ShowCopied()
    {
        try
        {
            var now = DateTime.UtcNow;
            if ((now - _lastShownUtc).TotalMilliseconds < DebounceMs)
            {
                return;
            }

            _lastShownUtc = now;

            var window = new ToastWindow(_theme, _settings.Style, _settings.Duration);
            int index = _active.Count;

            window.Closed += (_, _) =>
            {
                _active.Remove(window);
            };

            _active.Add(window);

            var (left, top, width, height) = GetWorkAreaDip();

            window.ShowAt(_settings.Position, index, left, top, width, height);
        }
        catch
        {
            // Плашка - украшение. Любая ошибка не должна ронять приложение.
        }
    }

    /// <summary>
    /// Прогрев рендера: первый созданный WPF-оконный кадр с эффектом тени
    /// компилируется медленно (300-500 мс). Прогреваем один раз при старте,
    /// показывая окно за пределами экрана с нулевой прозрачностью.
    /// </summary>
    public void WarmUp()
    {
        try
        {
            var window = new ToastWindow(_theme, _settings.Style, _settings.Duration);
            window.Left = -32000;
            window.Top = -32000;
            window.Opacity = 0;
            window.Show();

            var timer = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(150),
                System.Windows.Threading.DispatcherPriority.Background,
                (_, _) =>
                {
                    try { window.Close(); } catch { /* игнорируем */ }
                },
                System.Windows.Threading.Dispatcher.CurrentDispatcher);

            timer.Start();
        }
        catch
        {
            // прогрев не критичен
        }
    }

    public void CloseAll()
    {
        foreach (var w in _active.ToArray())
        {
            try { w.Close(); } catch { /* игнорируем */ }
        }
        _active.Clear();
    }

    /// <summary>Рабочая область монитора с курсором, в DIP-координатах.</summary>
    private static (double Left, double Top, double Width, double Height) GetWorkAreaDip()
    {
        try
        {
            NativeMethods.GetCursorPos(out var pt);
            var monitor = NativeMethods.MonitorFromPoint(pt, NativeMethods.MONITOR_DEFAULTTONEAREST);

            var info = new NativeMethods.MONITORINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFO>() };
            if (NativeMethods.GetMonitorInfo(monitor, ref info))
            {
                double scale = 1.0;
                try
                {
                    if (NativeMethods.GetDpiForMonitor(monitor, NativeMethods.MDT_EFFECTIVE_DPI, out uint dpiX, out _) == 0 && dpiX > 0)
                    {
                        scale = dpiX / 96.0;
                    }
                }
                catch
                {
                    scale = 1.0;
                }

                double left = info.rcWork.Left / scale;
                double top = info.rcWork.Top / scale;
                double width = (info.rcWork.Right - info.rcWork.Left) / scale;
                double height = (info.rcWork.Bottom - info.rcWork.Top) / scale;

                return (left, top, width, height);
            }
        }
        catch
        {
            // fallback ниже
        }

        return (0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
    }
}
