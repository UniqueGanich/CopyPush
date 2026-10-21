using System.Globalization;
using System.Windows.Media;

namespace CopyPush.Services;

/// <summary>
/// Локализация. ru -> "Скопировано", все остальные -> "Copied".
/// Сделано кодом (а не resx), чтобы при single-file publish не тянуть satellite-сборки.
/// </summary>
internal static class Loc
{
    public static string Copied { get; } = Build();

    public static string Enabled => Tr("Включено", "Enabled");
    public static string Disabled => Tr("Выключено", "Disabled");
    public static string Notifications => Tr("Показывать уведомление", "Show notification");
    public static string AnyClipboardChange => Tr("Показывать при любом изменении буфера", "Show on any clipboard change");
    public static string Position => Tr("Позиция", "Position");
    public static string AtCursor => Tr("У курсора", "At cursor");
    public static string ScreenCenter => Tr("По центру экрана", "Screen center");
    public static string BottomRight => Tr("В правом нижнем углу", "Bottom right");
    public static string Style => Tr("Оформление", "Style");
    public static string StyleSystem => Tr("Системный", "System");
    public static string StyleAccent => Tr("Акцентный", "Accent");
    public static string StyleContrast => Tr("Контрастный", "Contrast");
    public static string Duration => Tr("Длительность", "Duration");
    public static string Fast => Tr("Быстро", "Fast");
    public static string Normal => Tr("Обычно", "Normal");
    public static string Slow => Tr("Медленно", "Slow");
    public static string RunAtStartup => Tr("Запускать вместе с Windows", "Run at startup");
    public static string ShowTest => Tr("Показать пример", "Show test");
    public static string Exit => Tr("Выход", "Exit");
    public static string Title => Tr("CopyPush — уведомление о копировании", "CopyPush — copy notification");

    private static string Build()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase)
            ? "Скопировано"
            : "Copied";
    }

    private static string Tr(string ru, string en) => Build() == "Скопировано" ? ru : en;
}

/// <summary>Позиция плашки на экране.</summary>
internal enum ToastPosition
{
    AtCursor = 0,
    ScreenCenter = 1,
    BottomRight = 2,
}

/// <summary>Длительность анимации.</summary>
internal enum ToastDuration
{
    Fast = 0,
    Normal = 1,
    Slow = 2,
}
