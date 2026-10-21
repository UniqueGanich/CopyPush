using System.Windows.Threading;
using CopyPush.Hooks;
using CopyPush.Interop;

namespace CopyPush.Services;

/// <summary>
/// Определяет, что копирование действительно произошло.
///
/// Логика:
///  1. Хук видит Ctrl+C -> ставим флаг "запрошено копирование" (окно 1500 мс).
///  2. WM_CLIPBOARDUPDATE -> если флаг стоит, значит буфер действительно изменился
///     после Ctrl+C -> сообщаем "скопировано".
///  3. Сторожевой таймер снимает флаг, если буфер так и не изменился.
///
/// Благодаря этому Ctrl+C в терминале (прерывание процесса) и в играх
/// НЕ вызывают уведомление - буфер ведь не менялся.
/// </summary>
internal sealed class CopyDetector : IDisposable
{
    private const int CopyWindowMs = 1500;
    private const int WatchdogIntervalMs = 200;

    private readonly MessageWindow _messageWindow;
    private readonly DispatcherTimer _watchdog;
    private readonly Action _onCopied;
    private readonly Func<bool> _anyChangeEnabled;

    private bool _copyRequested;
    private DateTime _requestedAt;
    private uint _sequenceAtRequest;

    public CopyDetector(MessageWindow messageWindow, Action onCopied, Func<bool> anyChangeEnabled)
    {
        _messageWindow = messageWindow;
        _onCopied = onCopied;
        _anyChangeEnabled = anyChangeEnabled;

        _watchdog = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(WatchdogIntervalMs),
        };
        _watchdog.Tick += OnWatchdogTick;

        _messageWindow.ClipboardChanged += OnClipboardChanged;
    }

    /// <summary>Вызывается из хука, когда нажат Ctrl+C.</summary>
    public void NotifyCtrlC()
    {
        _copyRequested = true;
        _requestedAt = DateTime.UtcNow;
        _sequenceAtRequest = NativeMethods.GetClipboardSequenceNumber();

        Logger.Info("Ctrl+C detected");

        _watchdog.Stop();
        _watchdog.Start();
    }

    private void OnClipboardChanged()
    {
        if (!_copyRequested)
        {
            // Изменение буфера без предшествующего Ctrl+C.
            // Показываем только если пользователь явно включил такой режим
            // (например, копирование правой кнопкой мыши).
            if (_anyChangeEnabled())
            {
                _onCopied();
            }
            return;
        }

        if ((DateTime.UtcNow - _requestedAt).TotalMilliseconds > CopyWindowMs)
        {
            _copyRequested = false;
            _watchdog.Stop();
            return;
        }

        // Буфер реально изменился вскоре после Ctrl+C -> копирование состоялось.
        _copyRequested = false;
        _watchdog.Stop();
        Logger.Info("clipboard changed within window -> copy confirmed");
        _onCopied();
    }

    private void OnWatchdogTick(object? sender, EventArgs e)
    {
        if (!_copyRequested) return;

        // Страховка: если WM_CLIPBOARDUPDATE не пришёл, но номер последовательности
        // буфера изменился - копирование всё же состоялось.
        if (NativeMethods.GetClipboardSequenceNumber() != _sequenceAtRequest)
        {
            _copyRequested = false;
            _watchdog.Stop();
            _onCopied();
            return;
        }

        if ((DateTime.UtcNow - _requestedAt).TotalMilliseconds > CopyWindowMs)
        {
            _copyRequested = false;
            _watchdog.Stop();
        }
    }

    public void Dispose()
    {
        _watchdog.Stop();
        _watchdog.Tick -= OnWatchdogTick;
        _messageWindow.ClipboardChanged -= OnClipboardChanged;
    }
}
