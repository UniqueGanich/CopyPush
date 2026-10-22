# Changelog

**[Русский](#русский)** · **[English](#english)**

Все значимые изменения в этом проекте.
Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/),
проект использует [Semantic Versioning](https://semver.org/lang/ru/).

## Русский

### [1.0.0] — 2026-10-22

#### Добавлено

- Глобальная привязка `Ctrl+C` с всплывающей плашкой «Скопировано» у курсора мыши
- Определение копирования по изменению буфера обмена — без ложных срабатываний
- Локализация на 10 языков (Русский, English, Українська, Deutsch, Français, Español, 日本語, 한국어, 简体中文, 繁體中文)
  с автоматическим выбором языка системы и ручным переключением в меню трея
- Три варианта оформления плашки: системный, акцентный, контрастный
- Три варианта длительности анимации и три позиции на экране
- Живое обновление цветов при смене темы или акцента Windows
- Журнал событий в `%LOCALAPPDATA%\CopyPush\CopyPush.log`
- Автозапуск (включён по умолчанию) и запись в «Параметры → Приложения» с деинсталлятором
- Сборка и публикация по тегу через GitHub Actions

#### Технические решения

- Низкоуровневый хук `WH_KEYBOARD_LL` только на чтение — `Ctrl+C` не подавляется
- Подтверждение копирования через `WM_CLIPBOARDUPDATE` и `GetClipboardSequenceNumber`
- Окно-уведомление на WPF с `AllowsTransparency` (layered window, попиксельная альфа)
- Акцентный цвет берётся из `Explorer\Accent\AccentPalette` и WinRT `UISettings` —
  реестровый `DWM\AccentColor` может хранить устаревшее значение
- Единый `.exe` в single-file: self-contained (68 МБ) и framework-dependent (24 МБ)

---

## English

### [1.0.0] — 2026-10-22

#### Added

- Global `Ctrl+C` binding with a «Copied» toast popping up at the mouse cursor
- Copy detection based on an actual clipboard change — no false positives
- Localisation into 10 languages (Русский, English, Українська, Deutsch, Français, Español, 日本語, 한국어, 简体中文, 繁體中文),
  with automatic system-language detection and manual switching from the tray menu
- Three toast styles: system, accent, contrast
- Three animation durations and three on-screen positions
- Live colour refresh when the Windows theme or accent changes
- Event log at `%LOCALAPPDATA%\CopyPush\CopyPush.log`
- Autostart (enabled by default) plus an entry in Settings → Apps, with an uninstaller
- Build and publish on tag through GitHub Actions

#### Technical decisions

- A low-level read-only `WH_KEYBOARD_LL` hook — `Ctrl+C` is never swallowed
- Copy confirmation through `WM_CLIPBOARDUPDATE` and `GetClipboardSequenceNumber`
- A WPF toast window with `AllowsTransparency` (layered window, per-pixel alpha)
- The accent colour is taken from `Explorer\Accent\AccentPalette` and WinRT `UISettings` —
  the registry value `DWM\AccentColor` may hold a stale one
- A single `.exe` in single-file form: self-contained (68 MB) and framework-dependent (24 MB)

[1.0.0]: https://github.com/UniqueGanich/CopyPush/releases/tag/v1.0.0
