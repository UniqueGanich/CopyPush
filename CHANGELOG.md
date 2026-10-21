# Changelog

Все значимые изменения в этом проекте.
Формат основан на [Keep a Changelog](https://keepachangelog.com/ru/1.1.0/),
проект использует [Semantic Versioning](https://semver.org/lang/ru/).

## [1.0.0] — 2026-10-21

### Добавлено

- Глобальная привязка `Ctrl+C` с всплывающей плашкой «Скопировано» у курсора мыши
- Определение копирования по изменению буфера обмена — без ложных срабатываний
- Локализация на 6 языков (русский, english, українська, deutsch, français, español)
  с автоматическим выбором языка системы и ручным переключением в меню трея
- Три варианта оформления плашки: системный, акцентный, контрастный
- Три варианта длительности анимации и три позиции на экране
- Живое обновление цветов при смене темы или акцента Windows
- Журнал событий в `%LOCALAPPDATA%\CopyPush\CopyPush.log`
- Автозапуск и запись в «Параметры → Приложения» с деинсталлятором
- Сборка и публикация по тегу через GitHub Actions

### Технические решения

- Низкоуровневый хук `WH_KEYBOARD_LL` только на чтение — `Ctrl+C` не подавляется
- Подтверждение копирования через `WM_CLIPBOARDUPDATE` и `GetClipboardSequenceNumber`
- Окно-уведомление на WPF с `AllowsTransparency` (layered window, попиксельная альфа)
- Акцентный цвет берётся из `Explorer\Accent\AccentPalette` и WinRT `UISettings` —
  реестровый `DWM\AccentColor` может хранить устаревшее значение
- Единый `.exe` в single-file: self-contained (68 МБ) и framework-dependent (24 МБ)

[1.0.0]: https://github.com/UniqueGanich/CopyPush/releases/tag/v1.0.0
