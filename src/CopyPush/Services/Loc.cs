using System.Globalization;

namespace CopyPush.Services;

/// <summary>
/// Локализация интерфейса.
///
/// По умолчанию язык берётся из системы (Windows preferred UI language) -
/// пользователю не нужно ничего настраивать. Любой язык можно зафиксировать
/// вручную через меню в трее; настройка хранится в HKCU\Software\CopyPush.
///
/// Таблица сделана кодом, а не через .resx, чтобы при single-file publish
/// не появлялись отдельные satellite-сборки рядом с exe.
/// </summary>
internal static class Loc
{
    /// <summary>Пустая строка = язык определяется автоматически по системе.</summary>
    public const string Auto = "";

    /// <summary>Поддерживаемые языки в порядке показа в меню.</summary>
    public static readonly string[] Supported = { "ru", "en", "uk", "de", "fr", "es" };

    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = new()
    {
        ["ru"] = new()
        {
            ["toast.copied"] = "Скопировано",
            ["menu.notifications"] = "Показывать уведомление",
            ["menu.anyClipboardChange"] = "Показывать при любом изменении буфера",
            ["menu.position"] = "Позиция",
            ["menu.position.cursor"] = "У курсора",
            ["menu.position.center"] = "По центру экрана",
            ["menu.position.bottomRight"] = "В правом нижнем углу",
            ["menu.style"] = "Оформление",
            ["menu.style.system"] = "Системный",
            ["menu.style.accent"] = "Акцентный",
            ["menu.style.contrast"] = "Контрастный",
            ["menu.duration"] = "Длительность",
            ["menu.duration.fast"] = "Быстро",
            ["menu.duration.normal"] = "Обычно",
            ["menu.duration.slow"] = "Медленно",
            ["menu.startup"] = "Запускать вместе с Windows",
            ["menu.showTest"] = "Показать пример",
            ["menu.exit"] = "Выход",
            ["menu.language"] = "Язык",
            ["menu.language.auto"] = "Автоматически (системный)",
        },
        ["en"] = new()
        {
            ["toast.copied"] = "Copied",
            ["menu.notifications"] = "Show notification",
            ["menu.anyClipboardChange"] = "Show on any clipboard change",
            ["menu.position"] = "Position",
            ["menu.position.cursor"] = "At cursor",
            ["menu.position.center"] = "Screen center",
            ["menu.position.bottomRight"] = "Bottom right",
            ["menu.style"] = "Style",
            ["menu.style.system"] = "System",
            ["menu.style.accent"] = "Accent",
            ["menu.style.contrast"] = "Contrast",
            ["menu.duration"] = "Duration",
            ["menu.duration.fast"] = "Fast",
            ["menu.duration.normal"] = "Normal",
            ["menu.duration.slow"] = "Slow",
            ["menu.startup"] = "Run at startup",
            ["menu.showTest"] = "Show test",
            ["menu.exit"] = "Exit",
            ["menu.language"] = "Language",
            ["menu.language.auto"] = "Automatic (system)",
        },
        ["uk"] = new()
        {
            ["toast.copied"] = "Скопійовано",
            ["menu.notifications"] = "Показувати сповіщення",
            ["menu.anyClipboardChange"] = "Показувати при будь-якій зміні буфера",
            ["menu.position"] = "Позиція",
            ["menu.position.cursor"] = "Біля курсора",
            ["menu.position.center"] = "По центру екрана",
            ["menu.position.bottomRight"] = "У правому нижньому куті",
            ["menu.style"] = "Оформлення",
            ["menu.style.system"] = "Системний",
            ["menu.style.accent"] = "Акцентний",
            ["menu.style.contrast"] = "Контрастний",
            ["menu.duration"] = "Тривалість",
            ["menu.duration.fast"] = "Швидко",
            ["menu.duration.normal"] = "Звичайно",
            ["menu.duration.slow"] = "Повільно",
            ["menu.startup"] = "Запускати разом із Windows",
            ["menu.showTest"] = "Показати приклад",
            ["menu.exit"] = "Вихід",
            ["menu.language"] = "Мова",
            ["menu.language.auto"] = "Автоматично (системна)",
        },
        ["de"] = new()
        {
            ["toast.copied"] = "Kopiert",
            ["menu.notifications"] = "Benachrichtigung anzeigen",
            ["menu.anyClipboardChange"] = "Bei jeder Änderung der Zwischenablage anzeigen",
            ["menu.position"] = "Position",
            ["menu.position.cursor"] = "Am Mauszeiger",
            ["menu.position.center"] = "Bildschirmmitte",
            ["menu.position.bottomRight"] = "Unten rechts",
            ["menu.style"] = "Stil",
            ["menu.style.system"] = "System",
            ["menu.style.accent"] = "Akzent",
            ["menu.style.contrast"] = "Kontrast",
            ["menu.duration"] = "Dauer",
            ["menu.duration.fast"] = "Schnell",
            ["menu.duration.normal"] = "Normal",
            ["menu.duration.slow"] = "Langsam",
            ["menu.startup"] = "Mit Windows starten",
            ["menu.showTest"] = "Beispiel anzeigen",
            ["menu.exit"] = "Beenden",
            ["menu.language"] = "Sprache",
            ["menu.language.auto"] = "Automatisch (System)",
        },
        ["fr"] = new()
        {
            ["toast.copied"] = "Copié",
            ["menu.notifications"] = "Afficher la notification",
            ["menu.anyClipboardChange"] = "Afficher à toute modification du presse-papiers",
            ["menu.position"] = "Position",
            ["menu.position.cursor"] = "Au curseur",
            ["menu.position.center"] = "Au centre de l'écran",
            ["menu.position.bottomRight"] = "En bas à droite",
            ["menu.style"] = "Style",
            ["menu.style.system"] = "Système",
            ["menu.style.accent"] = "Accent",
            ["menu.style.contrast"] = "Contraste",
            ["menu.duration"] = "Durée",
            ["menu.duration.fast"] = "Rapide",
            ["menu.duration.normal"] = "Normale",
            ["menu.duration.slow"] = "Lente",
            ["menu.startup"] = "Démarrer avec Windows",
            ["menu.showTest"] = "Afficher un exemple",
            ["menu.exit"] = "Quitter",
            ["menu.language"] = "Langue",
            ["menu.language.auto"] = "Automatique (système)",
        },
        ["es"] = new()
        {
            ["toast.copied"] = "Copiado",
            ["menu.notifications"] = "Mostrar notificación",
            ["menu.anyClipboardChange"] = "Mostrar con cualquier cambio del portapapeles",
            ["menu.position"] = "Posición",
            ["menu.position.cursor"] = "En el cursor",
            ["menu.position.center"] = "Centro de la pantalla",
            ["menu.position.bottomRight"] = "Abajo a la derecha",
            ["menu.style"] = "Estilo",
            ["menu.style.system"] = "Sistema",
            ["menu.style.accent"] = "Acento",
            ["menu.style.contrast"] = "Contraste",
            ["menu.duration"] = "Duración",
            ["menu.duration.fast"] = "Rápida",
            ["menu.duration.normal"] = "Normal",
            ["menu.duration.slow"] = "Lenta",
            ["menu.startup"] = "Iniciar con Windows",
            ["menu.showTest"] = "Mostrar ejemplo",
            ["menu.exit"] = "Salir",
            ["menu.language"] = "Idioma",
            ["menu.language.auto"] = "Automático (sistema)",
        },
    };

    /// <summary>Выбор пользователя: код языка либо Auto (пустая строка).</summary>
    public static string Preference { get; private set; } = Auto;

    /// <summary>Фактически применяемый язык.</summary>
    public static string Language { get; private set; } = "en";

    /// <summary>Сработало при смене языка.</summary>
    public static event Action? Changed;

    /// <summary>Применяет язык. preference = "" означает "по системе".</summary>
    public static void Apply(string preference)
    {
        Preference = string.IsNullOrWhiteSpace(preference) ? Auto : preference;
        Language = Resolve();
        Changed?.Invoke();
    }

    /// <summary>Строка на текущем языке.</summary>
    public static string Get(string key)
    {
        if (Catalog.TryGetValue(Language, out var table) && table.TryGetValue(key, out var value))
        {
            return value;
        }

        if (Catalog.TryGetValue("en", out var fallback) && fallback.TryGetValue(key, out var en))
        {
            return en;
        }

        return key;
    }

    /// <summary>Название языка - всегда на самом языке (так принято в меню).</summary>
    public static string LanguageName(string code) => code switch
    {
        "ru" => "Русский",
        "en" => "English",
        "uk" => "Українська",
        "de" => "Deutsch",
        "fr" => "Français",
        "es" => "Español",
        _ => code,
    };

    /// <summary>Код языка, определённый по системе пользователя.</summary>
    public static string SystemLanguage()
    {
        try
        {
            var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            return Catalog.ContainsKey(code) ? code : "en";
        }
        catch
        {
            return "en";
        }
    }

    private static string Resolve()
    {
        if (!string.IsNullOrEmpty(Preference))
        {
            return Catalog.ContainsKey(Preference) ? Preference : "en";
        }

        return SystemLanguage();
    }
}
