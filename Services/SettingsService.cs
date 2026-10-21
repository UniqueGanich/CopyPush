using Microsoft.Win32;

namespace CopyPush.Services;

/// <summary>
/// Настройки хранятся в HKCU\Software\CopyPush.
/// Автозапуск - в HKCU\...\CurrentVersion\Run + StartupApproved.
/// </summary>
internal sealed class SettingsService
{
    private const string SettingsKey = @"Software\CopyPush";
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupApprovedKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string RunValueName = "CopyPush";

    public bool Enabled { get; set; } = true;
    public bool ShowOnAnyClipboardChange { get; set; }
    public ToastPosition Position { get; set; } = ToastPosition.AtCursor;
    public ToastStyle Style { get; set; } = ToastStyle.System;
    public ToastDuration Duration { get; set; } = ToastDuration.Normal;

    /// <summary>Запускать вместе с Windows.</summary>
    public bool RunAtStartup { get; set; } = true;

    public event Action? Changed;

    /// <summary>Сообщить подписчикам, что настройки изменились.</summary>
    public void NotifyChanged() => Changed?.Invoke();

    public void Load()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            if (key is null) return;

            Enabled = ReadBool(key, "Enabled", Enabled);
            ShowOnAnyClipboardChange = ReadBool(key, "ShowOnAnyClipboardChange", ShowOnAnyClipboardChange);
            Position = (ToastPosition)ReadInt(key, "Position", (int)Position);
            Style = (ToastStyle)ReadInt(key, "Style", (int)Style);
            Duration = (ToastDuration)ReadInt(key, "Duration", (int)Duration);
            RunAtStartup = ReadBool(key, "RunAtStartup", RunAtStartup);
        }
        catch
        {
            // повреждённые настройки не должны мешать запуску
        }
    }

    public void Save()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKey);
            if (key is null) return;

            key.SetValue("Enabled", Enabled ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("ShowOnAnyClipboardChange", ShowOnAnyClipboardChange ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("Position", (int)Position, RegistryValueKind.DWord);
            key.SetValue("Style", (int)Style, RegistryValueKind.DWord);
            key.SetValue("Duration", (int)Duration, RegistryValueKind.DWord);
            key.SetValue("RunAtStartup", RunAtStartup ? 1 : 0, RegistryValueKind.DWord);
        }
        catch
        {
            // игнорируем
        }

        Changed?.Invoke();
    }

    /// <summary>Регистрирует/снимает автозапуск.</summary>
    public void ApplyAutoStart()
    {
        try
        {
            using var run = Registry.CurrentUser.CreateSubKey(RunKey);
            if (run is not null)
            {
                if (RunAtStartup)
                {
                    run.SetValue(RunValueName, $"\"{ExecutablePath()}\" --startup", RegistryValueKind.String);
                }
                else
                {
                    if (run.GetValue(RunValueName) is not null) run.DeleteValue(RunValueName, throwOnMissingValue: false);
                }
            }

            using var approved = Registry.CurrentUser.CreateSubKey(StartupApprovedKey);
            if (approved is not null)
            {
                if (RunAtStartup)
                {
                    // 02 00 00 00 ... = "Enabled" (как это делает проводник)
                    approved.SetValue(RunValueName, new byte[] { 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, RegistryValueKind.Binary);
                }
                else
                {
                    if (approved.GetValue(RunValueName) is not null) approved.DeleteValue(RunValueName, throwOnMissingValue: false);
                }
            }
        }
        catch
        {
            // игнорируем
        }
    }

    public static string ExecutablePath()
    {
        // Environment.ProcessPath - реальный путь к запущенному .exe (работает и в single-file).
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path)) return path;

        return System.AppContext.BaseDirectory;
    }

    private static bool ReadBool(RegistryKey key, string name, bool fallback)
    {
        var v = key.GetValue(name);
        return v is int i ? i != 0 : fallback;
    }

    private static int ReadInt(RegistryKey key, string name, int fallback)
    {
        var v = key.GetValue(name);
        return v is int i ? i : fallback;
    }
}
