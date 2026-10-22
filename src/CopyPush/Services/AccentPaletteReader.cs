using System.Windows.Media;
using Microsoft.Win32;

// UseWindowsForms=true тянет за собой System.Drawing, где тоже есть Color.
using Color = System.Windows.Media.Color;

namespace CopyPush.Services;

/// <summary>
/// Палитра акцентов, снятая из реестра.
/// Заполняется только те значения, которые есть в реестре.
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
}

/// <summary>
/// Снятие палитры темы из <c>HKCU\...\Explorer\Accent\AccentPalette</c>.
///
/// Реестр хранит ровно те же цвета, что отдаёт WinRT
/// <c>UISettings.GetColorValue</c>: проверено на живой системе, значения
/// совпадают байт в байт. Поэтому обход WinRT не нужен - а вместе с ним
/// из публикации уходит сборка Windows SDK проекций (23 МБ).
///
/// Порядок цветов в AccentPalette (проверен против WinRT):
///   0 = AccentLight3, 1 = AccentLight2, 2 = AccentLight1, 3 = Accent,
///   4 = AccentDark1,  5 = AccentDark2,  6 = AccentDark3.
/// </summary>
internal sealed class AccentPaletteReader
{
    private const string ExplorerAccentKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Accent";

    public bool TryGetPalette(out ThemePalette palette)
    {
        palette = new ThemePalette();
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(ExplorerAccentKey);
            if (key?.GetValue("AccentPalette") is not byte[] bytes || bytes.Length < 32)
            {
                return false;
            }

            palette.AccentLight3 = Read(bytes, 0);
            palette.AccentLight2 = Read(bytes, 1);
            palette.AccentLight1 = Read(bytes, 2);
            palette.Accent = Read(bytes, 3);
            palette.AccentDark1 = Read(bytes, 4);
            palette.AccentDark2 = Read(bytes, 5);
            palette.AccentDark3 = Read(bytes, 6);

            return palette.Accent is not null;
        }
        catch
        {
            return false;
        }
    }

    private static Color? Read(byte[] bytes, int index)
    {
        int o = index * 4;
        if (bytes.Length < o + 4) return null;
        return Color.FromRgb(bytes[o], bytes[o + 1], bytes[o + 2]);
    }
}
