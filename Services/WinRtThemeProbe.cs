using System.Windows.Media;

namespace CopyPush.Services;

/// <summary>
/// Палитра темы, снятая через WinRT UISettings.
/// Заполняется только те значения, которые WinRT реально отдаёт.
/// </summary>
internal sealed class ThemePalette
{
    public Color? Accent { get; set; }
    public Color? AccentDark1 { get; set; }
    public Color? AccentDark2 { get; set; }
    public Color? AccentDark3 { get; set; }
    public Color? AccentLight1 { get; set; }
    public Color? AccentLight2 { get; set; }
    public Color? AccentLight3 { get; set; }
    public Color? Background { get; set; }
    public Color? Foreground { get; set; }
}

/// <summary>
/// Снятие текущих цветов темы через WinRT <c>Windows.UI.ViewManagement.UISettings</c>.
///
/// Это ОФИЦИАЛЬНЫЙ источник акцентного цвета в Windows 11. Реестровые
/// <c>HKCU\...\DWM\AccentColor</c> и <c>ColorizationColor</c> могут хранить
/// устаревшие значения (например, старый оранжевый), а UISettings отдаёт
/// актуальную палитру, которую сама система использует для окраски приложений.
/// </summary>
internal sealed class WinRtThemeProbe
{
    public bool TryGetPalette(out ThemePalette palette)
    {
        palette = new ThemePalette();
        try
        {
            var ui = new Windows.UI.ViewManagement.UISettings();
            // UISettings по умолчанию следует за системной темой (свойство Theme
            // появилось только в более поздних версиях SDK, поэтому не используем).

            palette.Accent = Read(ui, Windows.UI.ViewManagement.UIColorType.Accent);
            palette.AccentDark1 = Read(ui, Windows.UI.ViewManagement.UIColorType.AccentDark1);
            palette.AccentDark2 = Read(ui, Windows.UI.ViewManagement.UIColorType.AccentDark2);
            palette.AccentDark3 = Read(ui, Windows.UI.ViewManagement.UIColorType.AccentDark3);
            palette.AccentLight1 = Read(ui, Windows.UI.ViewManagement.UIColorType.AccentLight1);
            palette.AccentLight2 = Read(ui, Windows.UI.ViewManagement.UIColorType.AccentLight2);
            palette.AccentLight3 = Read(ui, Windows.UI.ViewManagement.UIColorType.AccentLight3);
            palette.Background = Read(ui, Windows.UI.ViewManagement.UIColorType.Background);
            palette.Foreground = Read(ui, Windows.UI.ViewManagement.UIColorType.Foreground);

            return palette.Accent is not null;
        }
        catch
        {
            return false;
        }
    }

    private static Color? Read(Windows.UI.ViewManagement.UISettings ui, Windows.UI.ViewManagement.UIColorType type)
    {
        try
        {
            var c = ui.GetColorValue(type);
            return Color.FromRgb(c.R, c.G, c.B);
        }
        catch
        {
            return null;
        }
    }
}
