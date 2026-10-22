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

    /// <summary>
    /// Сколько окон плашки держим про запас. Окна переиспользуются: создавать
    /// новое WPF-окно с тенью на каждое копирование - лишние аллокации и задержка.
    /// </summary>
    private const int PoolSize = 4;

    private readonly ThemeService _theme;
    private readonly SettingsService _settings;
    private readonly List<ToastWindow> _pool = new();
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

            // Поверх полноэкранных окон (игры, видео) плашку не показываем -
            // она всё равно никому не нужна, а мешает.
            if (IsForegroundFullscreen())
            {
                Logger.Info("foreground window is fullscreen - toast skipped");
                return;
            }

            // Берём спрятанное окно из пула, иначе создаём новое.
            int visible = 0;
            foreach (var w in _pool)
            {
                if (w.IsVisible) visible++;
            }

            ToastWindow? window = null;
            foreach (var w in _pool)
            {
                if (!w.IsVisible) { window = w; break; }
            }

            if (window is null)
            {
                if (_pool.Count < PoolSize)
                {
                    window = new ToastWindow(_theme, _settings.Style, _settings.Duration);
                    _pool.Add(window);
                }
                else
                {
                    // Пул заполнен - переиспользуем самое старое окно.
                    window = _pool[0];
                }
            }

            window.Prepare(_theme, _settings.Style, _settings.Duration);

            var (left, top, width, height) = GetWorkAreaDip();

            window.ShowAt(_settings.Position, visible, left, top, width, height);
        }
        catch (Exception ex)
        {
            // Плашка - украшение: ошибка не должна ронять приложение.
            // Но её обязательно видно в журнале, иначе поломка незаметна.
            Logger.Error("Failed to show toast", ex);
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
            // Прогреваем одно из пуловых окон и оставляем его в пуле.
            var window = new ToastWindow(_theme, _settings.Style, _settings.Duration);
            _pool.Add(window);

            window.Left = -32000;
            window.Top = -32000;
            window.Opacity = 0;
            window.Show();

            var timer = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(150),
                System.Windows.Threading.DispatcherPriority.Background,
                (_, _) =>
                {
                    try
                    {
                        window.Visibility = Visibility.Hidden;
                        window.Opacity = 1;
                    }
                    catch
                    {
                        // игнорируем
                    }
                },
                System.Windows.Threading.Dispatcher.CurrentDispatcher);

            timer.Start();
        }
        catch (Exception ex)
        {
            // прогрев не критичен, но причина полезна в журнале
            Logger.Error("Toast warm-up failed", ex);
        }
    }

    public void CloseAll()
    {
        foreach (var w in _pool)
        {
            try { w.Close(); } catch { /* игнорируем */ }
        }
        _pool.Clear();
    }

    /// <summary>
    /// Занимает ли активное окно весь монитор (полноэкранный режим).
    /// </summary>
    private static bool IsForegroundFullscreen()
    {
        try
        {
            var hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return false;

            if (!NativeMethods.GetWindowRect(hwnd, out var rect)) return false;

            var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            var info = new NativeMethods.MONITORINFO
            {
                cbSize = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFO>(),
            };

            if (!NativeMethods.GetMonitorInfo(monitor, ref info)) return false;

            return rect.Left <= info.rcMonitor.Left
                && rect.Top <= info.rcMonitor.Top
                && rect.Right >= info.rcMonitor.Right
                && rect.Bottom >= info.rcMonitor.Bottom;
        }
        catch
        {
            return false;
        }
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
