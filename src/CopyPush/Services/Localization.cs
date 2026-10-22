using System.Globalization;

namespace CopyPush.Services;

/// <summary>
/// Локализация интерфейса.
///
/// По умолчанию язык берётся из системы (Windows preferred UI language) -
/// пользователю не нужно ничего настраивать. Любой язык можно зафиксировать
/// вручную через меню в трее; настройка хранится в HKCU\Software\CopyPush.
///
/// Если языка системы нет в списке, используется английский.
///
/// Таблица сделана кодом, а не через .resx, чтобы при single-file publish
/// не появлялись отдельные satellite-сборки рядом с exe.
/// </summary>
internal static class Loc
{
    /// <summary>Пустая строка = язык определяется автоматически по системе.</summary>
    public const string Auto = "";

    /// <summary>
    /// Поддерживаемые языки в порядке показа в меню.
    /// zh-Hans и zh-Hant различаются, поэтому здесь полные теги, а не двухбуквенные коды.
    /// </summary>
    public static readonly string[] Supported =
    {
        "ru", "en", "uk", "de", "fr", "es", "ja", "ko", "zh-Hans", "zh-Hant",
    };

    /// <summary>Какой тег использовать для нейтрального кода языка.</summary>
    private static readonly Dictionary<string, string> DefaultVariant = new()
    {
        ["zh"] = "zh-Hans",
    };

    /// <summary>Регионы, где китайский традиционный.</summary>
    private static readonly string[] TraditionalChineseRegions = { "TW", "HK", "MO" };

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
        ["ja"] = new()
        {
            ["toast.copied"] = "コピーしました",
            ["menu.notifications"] = "通知を表示",
            ["menu.anyClipboardChange"] = "クリップボード変更時に表示",
            ["menu.position"] = "位置",
            ["menu.position.cursor"] = "カーソル位置",
            ["menu.position.center"] = "画面中央",
            ["menu.position.bottomRight"] = "右下",
            ["menu.style"] = "スタイル",
            ["menu.style.system"] = "システム",
            ["menu.style.accent"] = "アクセント",
            ["menu.style.contrast"] = "高コントラスト",
            ["menu.duration"] = "表示時間",
            ["menu.duration.fast"] = "短い",
            ["menu.duration.normal"] = "標準",
            ["menu.duration.slow"] = "長い",
            ["menu.startup"] = "Windows 起動時に自動実行",
            ["menu.showTest"] = "サンプルを表示",
            ["menu.exit"] = "終了",
            ["menu.language"] = "言語",
            ["menu.language.auto"] = "自動（システム）",
        },
        ["ko"] = new()
        {
            ["toast.copied"] = "복사됨",
            ["menu.notifications"] = "알림 표시",
            ["menu.anyClipboardChange"] = "클립보드 변경 시 표시",
            ["menu.position"] = "위치",
            ["menu.position.cursor"] = "커서 위치",
            ["menu.position.center"] = "화면 중앙",
            ["menu.position.bottomRight"] = "오른쪽 아래",
            ["menu.style"] = "스타일",
            ["menu.style.system"] = "시스템",
            ["menu.style.accent"] = "강조",
            ["menu.style.contrast"] = "고대비",
            ["menu.duration"] = "표시 시간",
            ["menu.duration.fast"] = "빠르게",
            ["menu.duration.normal"] = "보통",
            ["menu.duration.slow"] = "천천히",
            ["menu.startup"] = "Windows 시작 시 자동 실행",
            ["menu.showTest"] = "예제 표시",
            ["menu.exit"] = "종료",
            ["menu.language"] = "언어",
            ["menu.language.auto"] = "자동 (시스템 설정)",
        },
        ["zh-Hans"] = new()
        {
            ["toast.copied"] = "已复制",
            ["menu.notifications"] = "显示通知",
            ["menu.anyClipboardChange"] = "剪贴板变化时显示",
            ["menu.position"] = "位置",
            ["menu.position.cursor"] = "跟随鼠标",
            ["menu.position.center"] = "屏幕中央",
            ["menu.position.bottomRight"] = "右下角",
            ["menu.style"] = "样式",
            ["menu.style.system"] = "系统",
            ["menu.style.accent"] = "强调色",
            ["menu.style.contrast"] = "高对比",
            ["menu.duration"] = "时长",
            ["menu.duration.fast"] = "快",
            ["menu.duration.normal"] = "普通",
            ["menu.duration.slow"] = "慢",
            ["menu.startup"] = "开机自动启动",
            ["menu.showTest"] = "显示示例",
            ["menu.exit"] = "退出",
            ["menu.language"] = "语言",
            ["menu.language.auto"] = "自动（跟随系统）",
        },
        ["zh-Hant"] = new()
        {
            ["toast.copied"] = "已複製",
            ["menu.notifications"] = "顯示通知",
            ["menu.anyClipboardChange"] = "剪貼簿變更時顯示",
            ["menu.position"] = "位置",
            ["menu.position.cursor"] = "跟隨滑鼠",
            ["menu.position.center"] = "螢幕中央",
            ["menu.position.bottomRight"] = "右下角",
            ["menu.style"] = "樣式",
            ["menu.style.system"] = "系統",
            ["menu.style.accent"] = "強調色",
            ["menu.style.contrast"] = "高對比",
            ["menu.duration"] = "時長",
            ["menu.duration.fast"] = "快",
            ["menu.duration.normal"] = "普通",
            ["menu.duration.slow"] = "慢",
            ["menu.startup"] = "開機自動啟動",
            ["menu.showTest"] = "顯示範例",
            ["menu.exit"] = "結束",
            ["menu.language"] = "語言",
            ["menu.language.auto"] = "自動（跟隨系統）",
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
        "ja" => "日本語",
        "ko" => "한국어",
        "zh-Hans" => "简体中文",
        "zh-Hant" => "繁體中文",
        _ => code,
    };

    /// <summary>
    /// Язык, определённый по системе пользователя.
    /// Если языка системы в списке нет - возвращается английский.
    /// </summary>
    public static string SystemLanguage()
    {
        try
        {
            var culture = CultureInfo.CurrentUICulture;

            // 1. Точное совпадение полного тега, напр. "zh-Hans" или "ja-JP".
            if (Catalog.ContainsKey(culture.Name))
            {
                return culture.Name;
            }

            // 2. Китайский: традиционный или упрощённый - по региону.
            var lang = culture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (lang == "zh")
            {
                var region = culture.Name.Length > 3 ? culture.Name.Substring(3) : "";
                return TraditionalChineseRegions.Contains(region) ? "zh-Hant" : "zh-Hans";
            }

            // 3. Нейтральный код языка, напр. "de" или "ko".
            if (DefaultVariant.TryGetValue(lang, out var variant) && Catalog.ContainsKey(variant))
            {
                return variant;
            }

            if (Catalog.ContainsKey(lang))
            {
                return lang;
            }

            // 4. Языка системы нет - английский по умолчанию.
            return "en";
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
