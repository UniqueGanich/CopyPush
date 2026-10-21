using System.Windows.Media;
using Microsoft.Win32;

// UseWindowsForms=true тянет за собой System.Drawing, где тоже есть Color.
using Color = System.Windows.Media.Color;

namespace CopyPush.Services;

/// <summary>
/// Определяет цвета плашки в соответствии с темой Windows.
///
/// ВАЖНО про акцентный цвет:
///  - <c>HKCU\...\DWM\AccentColor</c> хранится в формате DWORD ABGR
///    (байты: Alpha, Blue, Green, Red). Путать порядок байт нельзя.
///  - <c>HKCU\...\Explorer\Accent\AccentPalette</c> - массив из 8 цветов в формате RGBA,
///    где индекс 3 - базовый (выбранный в «Параметры → Цвета») акцент.
///    Этот источник синхроннее остальных, поэтому он основной.
///  - WinRT <c>UISettings</c> отдаёт актуальную палитру (AccentLight2 = базовый акцент).
///
/// Реестровый <c>Control Panel\Colors</c> и <c>SystemColors</c> использовать нельзя:
/// в Windows 11 эти legacy-значения остаются СВЕТЛЫМИ даже при тёмной теме.
/// </summary>
internal sealed class ThemeService
{
    private const string PersonalizeKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string DwmKey = @"SOFTWARE\Microsoft\Windows\DWM";
    private const string ExplorerAccentKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Accent";

    private readonly WinRtThemeProbe _probe = new();

    /// <summary>True - тёмная тема.</summary>
    public bool IsDark { get; private set; }

    /// <summary>Акцентный цвет темы (например #0078D4).</summary>
    public Color Accent { get; private set; }

    /// <summary>Поверхность плашки (цвет окна в стиле Windows 11).</summary>
    public Color Surface { get; private set; }

    /// <summary>Контрастный цвет текста/иконки.</summary>
    public Color Foreground { get; private set; }

    /// <summary>Цвет разделительной рамки.</summary>
    public Color Border { get; private set; }

    /// <summary>Цвет тени.</summary>
    public Color Shadow { get; private set; }

    public event Action? Changed;

    public ThemeService()
    {
        Reload();
    }

    public void Reload()
    {
        IsDark = !ReadAppsUseLightTheme();
        Accent = ReadAccentColor();

        // Материалы Windows 11: тёмная тема - слой #2C2C2C, светлая - #F9F9F9.
        Surface = IsDark ? Color.FromRgb(0x2C, 0x2C, 0x2C) : Color.FromRgb(0xF9, 0xF9, 0xF9);
        Foreground = IsDark ? Colors.White : Colors.Black;
        Border = IsDark ? Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x1A, 0x00, 0x00, 0x00);
        Shadow = IsDark ? Color.FromArgb(0x80, 0x00, 0x00, 0x00) : Color.FromArgb(0x66, 0x00, 0x00, 0x00);

        // Сверка с WinRT: если система отдаёт согласованный с темой foreground -
        // берём его, это точнее хардкода.
        if (_probe.TryGetPalette(out var palette) && palette.Foreground is { } fg)
        {
            Foreground = fg;
        }

        Logger.Info($"theme reloaded: {(IsDark ? "dark" : "light")}, accent=#{Accent.R:X2}{Accent.G:X2}{Accent.B:X2}");

        Changed?.Invoke();
    }

    /// <summary>
    /// Дешёвая проверка: не изменились ли тема/акцент.
    /// Вызывается по таймеру, чтобы смена светлой/тёмной темы или акцента
    /// подхватывалась даже если системное сообщение не дошло.
    /// </summary>
    public void ReloadIfChanged()
    {
        bool dark = !ReadAppsUseLightTheme();
        var accent = ReadAccentFromExplorerPalette() ?? ReadAccentFromDwm();

        if (dark == IsDark && accent is { } a && a == Accent)
        {
            return;
        }

        Reload();
    }

    /// <summary>Цвета конкретного варианта оформления плашки.</summary>
    public (Color Surface, Color Foreground, Color Border) Resolve(ToastStyle style)
    {
        switch (style)
        {
            case ToastStyle.Accent:
                return (Accent, Colors.White, Color.FromArgb(0x00, 0x00, 0x00, 0x00));

            case ToastStyle.Contrast:
                return (Surface, Accent, Accent);

            default:
                return (Surface, Foreground, Border);
        }
    }

    private static bool ReadAppsUseLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                return value != 0;
            }
        }
        catch
        {
            // Реестр недоступен - считаем светлой темой.
        }

        return true;
    }

    /// <summary>
    /// Акцентный цвет: три независимых источника, приоритет:
    /// 1) Explorer\Accent\AccentPalette[3] - самый синхронный с UI
    /// 2) WinRT UISettings.AccentLight2
    /// 3) DWM\AccentColor (DWORD в формате ABGR)
    /// </summary>
    private Color ReadAccentColor()
    {
        var fromPalette = ReadAccentFromExplorerPalette();
        var fromWinRt = _probe.TryGetPalette(out var palette) ? palette.AccentLight2 : null;
        var fromDwm = ReadAccentFromDwm();

        // Если два источника совпали - берём их (наиболее надёжно).
        if (fromPalette is { } a && fromWinRt is { } b && a == b) return a;
        if (fromPalette is { } c && fromDwm is { } d && c == d) return c;
        if (fromWinRt is { } e && fromDwm is { } f && e == f) return e;

        return fromPalette ?? fromWinRt ?? fromDwm ?? Color.FromRgb(0x00, 0x78, 0xD4);
    }

    /// <summary>
    /// Explorer\Accent\AccentPalette - 8 цветов в формате RGBA.
    /// Индекс 3 - базовый акцент (тот, что выбран в «Параметры → Цвета»).
    /// </summary>
    private static Color? ReadAccentFromExplorerPalette()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(ExplorerAccentKey);
            if (key?.GetValue("AccentPalette") is byte[] bytes && bytes.Length >= 20)
            {
                // Байты 12..14 = R,G,B третьего цвета палитры (индекс 3).
                return Color.FromRgb(bytes[12], bytes[13], bytes[14]);
            }
        }
        catch
        {
            // игнорируем
        }

        return null;
    }

    /// <summary>
    /// HKCU\...\DWM\AccentColor - DWORD в формате ABGR:
    /// младший байт = Red, затем Green, затем Blue, старший = Alpha.
    /// </summary>
    private static Color? ReadAccentFromDwm()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(DwmKey);
            if (key?.GetValue("AccentColor") is int dw)
            {
                byte r = (byte)(dw & 0xFF);
                byte g = (byte)((dw >> 8) & 0xFF);
                byte b = (byte)((dw >> 16) & 0xFF);
                if (r + g + b > 0) return Color.FromRgb(r, g, b);
            }
        }
        catch
        {
            // используем значение по умолчанию
        }

        return null;
    }
}
