using System.Globalization;
using CopyPush.Services;

namespace CopyPush.Tests;

/// <summary>
/// Проверка выбора языка по языку системы пользователя.
/// </summary>
internal static class Program
{
    private static int _failed;

    private static int Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // --- язык системы есть в списке поддерживаемых ---
        Check("ru-RU", "ru");
        Check("en-US", "en");
        Check("en-GB", "en");
        Check("uk-UA", "uk");
        Check("de-DE", "de");
        Check("de-AT", "de");
        Check("fr-FR", "fr");
        Check("fr-CA", "fr");
        Check("es-ES", "es");
        Check("es-MX", "es");
        Check("ja-JP", "ja");
        Check("ko-KR", "ko");

        // --- китайский: упрощённый или традиционный по региону ---
        Check("zh-CN", "zh-Hans");
        Check("zh-SG", "zh-Hans");
        Check("zh-Hans", "zh-Hans");
        Check("zh-TW", "zh-Hant");
        Check("zh-HK", "zh-Hant");
        Check("zh-MO", "zh-Hant");
        Check("zh-Hant", "zh-Hant");

        // --- языка системы нет: английский по умолчанию ---
        Check("he-IL", "en");
        Check("ar-EG", "en");
        Check("hi-IN", "en");
        Check("th-TH", "en");
        Check("pt-BR", "en");
        Check("nl-NL", "en");
        Check("pl-PL", "en");
        Check("tr-TR", "en");
        Check("sv-SE", "en");
        Check("cs-CZ", "en");

        // --- каждая поддерживаемая культура имеет полный набор строк ---
        foreach (var lang in Loc.Supported)
        {
            CheckAllKeysPresent(lang);
        }

        // --- неизвестный код языка в настройках не ломает приложение ---
        CheckPreferenceFallback("zz-ZZ", "en");
        CheckPreferenceFallback("", "en");   // авто: язык системы ru-RU в этом тесте

        Console.WriteLine();
        if (_failed > 0)
        {
            Console.WriteLine($"ПРОВАЛЕНО проверок: {_failed}");
            return 1;
        }

        Console.WriteLine("Все проверки пройдены.");
        return 0;
    }

    private static void Check(string cultureName, string expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            var actual = Loc.SystemLanguage();
            var ok = actual == expected;

            if (!ok) _failed++;

            Console.WriteLine($"  {cultureName,-10} -> {actual,-9} ожидалось {expected,-9} {(ok ? "OK" : "FAIL")}");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private static void CheckAllKeysPresent(string language)
    {
        // Переключаем язык и проверяем, что ключи отдаются, а не падают.
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Loc.Apply(language);

            var ok = true;
            foreach (var key in new[] { "toast.copied", "menu.notifications", "menu.language", "menu.exit" })
            {
                var value = Loc.Get(key);
                if (string.IsNullOrWhiteSpace(value) || value == key)
                {
                    ok = false;
                    Console.WriteLine($"  {language,-9} ключ {key,-20} НЕ НАЙДЕН");
                }
            }

            if (!ok) _failed++;
            else Console.WriteLine($"  {language,-9} ключи на месте              OK");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
            Loc.Apply("");
        }
    }

    private static void CheckPreferenceFallback(string preference, string expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Loc.Apply(preference);
            var actual = Loc.Language;
            var ok = actual == expected;

            if (!ok) _failed++;
            Console.WriteLine($"  настройка '{preference,-8}' -> {actual,-9} ожидалось {expected,-9} {(ok ? "OK" : "FAIL")}");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
            Loc.Apply("");
        }
    }
}
