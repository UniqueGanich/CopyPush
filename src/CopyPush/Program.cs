using System.Windows;
using System.Windows.Threading;
using CopyPush.Hooks;
using CopyPush.Services;

using Application = System.Windows.Application;

namespace CopyPush;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        // Один экземпляр на пользователя.
        using var mutex = new Mutex(initiallyOwned: true, @"Local\CopyPush.SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            return 0;
        }

        var app = new Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown,
        };

        app.DispatcherUnhandledException += (_, e) =>
        {
            // Приложение - фоновое: непойманная ошибка не должна его ронять.
            // Но молчать нельзя - пишем в журнал, иначе поломку не заметить.
            Logger.Error("Unhandled dispatcher exception", e.Exception);
            e.Handled = true;
        };

        AppContext? context = null;
        try
        {
            context = new AppContext();
        }
        catch (Exception ex)
        {
            // Не удалось запуститься (например, не установился хук) - сообщаем
            // пользователю и выходим, а не падаем с окном ошибки.
            Logger.Error("CopyPush failed to start", ex);
            try
            {
                MessageBox.Show(
                    "CopyPush could not start." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "CopyPush", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // MessageBox может быть недоступен - не критично.
            }

            return 1;
        }

        try
        {
            app.Run();
        }
        finally
        {
            context.Dispose();
        }

        return 0;
    }
}

/// <summary>Сборка всех компонентов приложения.</summary>
internal sealed class AppContext : IDisposable
{
    private readonly SettingsService _settings = new();
    private readonly ThemeService _theme;
    private readonly MessageWindow _messageWindow;
    private readonly KeyboardHook _keyboardHook;
    private readonly CopyDetector _copyDetector;
    private readonly ToastService _toastService;
    private readonly TrayIcon _trayIcon;
    private readonly DispatcherTimer _themeTimer;

    public AppContext()
    {
        _settings.Load();
        _settings.Save();   // фиксируем дефолты, чтобы их было видно в трее

        // Язык интерфейса: по умолчанию язык системы, можно зафиксировать вручную.
        Loc.Apply(_settings.Language);

        _theme = new ThemeService();
        _theme.Changed += () => _settings.NotifyChanged();

        Logger.Info($"CopyPush starting. Theme={(_theme.IsDark ? "dark" : "light")}, accent={_theme.Accent}, style={_settings.Style}, position={_settings.Position}, duration={_settings.Duration}, language={Loc.Language}");

        _toastService = new ToastService(_theme, _settings);

        _messageWindow = new MessageWindow();
        _messageWindow.ThemeChanged += () => _theme.Reload();

        _copyDetector = new CopyDetector(
            _messageWindow,
            OnCopied,
            () => _settings.Enabled && _settings.ShowOnAnyClipboardChange);

        _keyboardHook = new KeyboardHook(() => _copyDetector.NotifyCtrlC());

        _trayIcon = new TrayIcon(_settings, _theme, ShowTest, Shutdown);

        // Регистрируем автозапуск согласно сохранённой настройке.
        _settings.ApplyAutoStart();

        // Стартуем без окна.
        _keyboardHook.Install();
        _messageWindow.StartClipboardListener();

        Logger.Info("keyboard hook installed, clipboard listener active");
        Logger.Info($"autostart entry: {SettingsService.ExecutablePath()}");

        // Прогреваем рендер WPF, чтобы первая настоящая плашка появилась мгновенно.
        _toastService.WarmUp();

        // Периодическая сверка темы: гарантирует подхват смены акцента и
        // светлой/тёмной темы, даже если системное сообщение не дойдёт.
        _themeTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(2),
        };
        _themeTimer.Tick += (_, _) => _theme.ReloadIfChanged();
        _themeTimer.Start();
    }

    private void OnCopied()
    {
        if (!_settings.Enabled)
        {
            Logger.Info("copy detected, but notifications are disabled");
            return;
        }

        Logger.Info("copy detected -> showing toast");
        _toastService.ShowCopied();
    }

    private void ShowTest()
    {
        _toastService.ShowCopied();
    }

    private void Shutdown()
    {
        // Закрываем видимые плашки, чтобы они не "зависли" на экране.
        _toastService.CloseAll();
        Application.Current?.Shutdown();
    }

    public void Dispose()
    {
        _themeTimer.Stop();
        _keyboardHook.Dispose();
        _copyDetector.Dispose();
        _messageWindow.Dispose();
        _trayIcon.Dispose();
    }
}
