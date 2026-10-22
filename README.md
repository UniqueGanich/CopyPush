<div align="center">

<img src="docs/images/icon-256.png" alt="CopyPush" width="96" height="96">

# CopyPush

**Маленькая утилита для Windows: при копировании по `Ctrl+C` у курсора мыши
всплывает аккуратная плашка «Скопировано».**

[![Release](https://img.shields.io/github/v/release/UniqueGanich/CopyPush?label=Release&color=blue)](https://github.com/UniqueGanich/CopyPush/releases)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows)](https://microsoft.com/windows)
[![Languages](https://img.shields.io/badge/languages-ru%20%7C%20en%20%7C%20uk%20%7C%20de%20%7C%20fr%20%7C%20es%20%7C%20ja%20%7C%20ko%20%7C%20zh-0078D4)](src/CopyPush/Services/Loc.cs)

[English](README.en.md)

</div>

---

![Скриншот](docs/images/hero.png)

## Зачем

Нажать `Ctrl+C` и **не знать, сработало ли копирование** — мелкое, но постоянное
раздражание: приходится вставлять в поле, чтобы проверить. CopyPush снимает вопрос
одной плашкой прямо у курсора.

## Что умеет

- **Глобальная привязка `Ctrl+C`** — работает в любом приложении, а не только в вашем.
- **Ничего не ломает.** `Ctrl+C` не перехватывается: используется хук *на чтение*.
  В консоли он по-прежнему прерывает процесс, в играх работает как игровая клавиша.
- **Без ложных срабатываний.** Плашка появляется, только если содержимое буфера
  действительно изменилось после нажатия.
- **Язык подстраивается сам.** интерфейс и надпись «Скопировано» берут язык системы
  (русский / english / українська / deutsch / français / español / 日本語 / 한국어 / 简体中文 / 繁體中文). Язык можно
  зафиксировать вручную в меню в трее.
- **Цвет системной темы.** Фон — как у системных окон, текст контрастный.
  Акцентный цвет берётся из темы Windows и подхватывается на лету.
- **Не мешает.** Окно не перехватывает клики, не активируется и не появляется в `Alt+Tab`.
- **Пишет журнал** в `%LOCALAPPDATA%\CopyPush\CopyPush.log` — удобно, если что-то пойдёт не так.

## Установка

Готовые сборки лежат в [Releases](https://github.com/UniqueGanich/CopyPush/releases):

| Что скачать | Размер | Требования |
|---|---|---|
| [**CopyPush-1.0.0-win-x64.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64.zip) | ~63 МБ | ничего, работает сразу на Windows 10/11 |
| [**CopyPush-1.0.0-win-x64-small.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64-small.zip) | ~6 МБ | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

1. Скачайте архив и распакуйте в любую папку (например `C:\Tools\CopyPush`).
2. Запустите `CopyPush.exe` — появится иконка в трее.
3. Готово: `Ctrl+C` в любом приложении покажет плашку «Скопировано».

Автозапуск по умолчанию **выключен** — включается в меню иконки в трее
(«Запускать вместе с Windows»).

### Удаление

Запустите `Uninstall.cmd` из папки программы — он уберёт автозапуск, настройки и саму программу.
Ещё можно удалить через «Параметры → Приложения».

## Настройки

Всё настраивается в меню иконки в трее:

| Пункт | Варианты |
|---|---|
| Показывать уведомление | вкл / выкл |
| Показывать при любом изменении буфера | вкл / выкл (реагировать на копирование правой кнопкой мыши) |
| Позиция | у курсора / по центру экрана / в правом нижнем углу |
| Оформление | системный / акцентный / контрастный |
| Длительность | быстро / обычно / медленно |
| **Язык** | автоматически (системный) / русский / english / українська / deutsch / français / español / 日本語 / 한국어 / 简体中文 / 繁體中文 |
| Запускать вместе с Windows | вкл / выкл |

### Три варианта оформления

| Системный | Акцентный | Контрастный |
|:---:|:---:|:---:|
| ![](docs/images/style-system.png) | ![](docs/images/style-accent.png) | ![](docs/images/style-contrast.png) |

### Язык

По умолчанию язык берётся из системы: `Windows → Параметры → Время и язык → Язык и регион →
Предпочтительные языки`. Если там первым стоит русский — интерфейс и надпись будут русскими,
если английский — английскими. Любой из шести поддерживаемых языков можно зафиксировать
вручную: меню иконки в трее → **Язык**. После смены языка Windows достаточно перезапустить CopyPush.

## Как это работает

```
Ctrl+C  ──►  WH_KEYBOARD_LL (наблюдение, ничего не подавляет)
                    │
                    ├──►  флаг "копирование запрошено" (окно 1.5 с)
                    │
 WM_CLIPBOARDUPDATE ─┘
        буфер изменился?
             │ да                    │ нет
             ▼                       ▼
        показать плашку        ничего не делать
```

Благодаря проверке «буфер реально изменился» `Ctrl+C` в терминале (это прерывание
процесса) и в играх не вызывают ложных уведомлений.

Технически:

- **Хук** — `SetWindowsHookEx(WH_KEYBOARD_LL)`. Обработчик всегда возвращает
  `CallNextHookEx`, поэтому ввод уходит в приложение без изменений.
- **Подтверждение** — `AddClipboardFormatListener` + `WM_CLIPBOARDUPDATE`,
  плюс `GetClipboardSequenceNumber` как страховка.
- **Плашка** — WPF-окно с `AllowsTransparency` (layered window, попиксельная альфа),
  расширенные стили `WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT`.
- **Анимация** — `Storyboard` над `RenderTransform` (подъём на 48 px) и `Opacity`.
- **Тема** — `AppsUseLightTheme` + акцент из `Explorer\Accent\AccentPalette`
  и WinRT `UISettings`; цвета перечитываются при смене темы.
- **Язык** — `CultureInfo.CurrentUICulture` (язык системы) либо зафиксированный в настройках.

## Требования

- Windows 10 версии 19041 (20H1) или новее
- Windows 11 — полная поддержка, включая живую смену темы
- Права администратора **не нужны**

## Сборка из исходников

Нужен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/UniqueGanich/CopyPush.git
cd CopyPush

# один .exe со всем внутри (не требует .NET на компьютере)
dotnet publish src/CopyPush/CopyPush.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true -o out
```

Скрипты в `build/`:

| Скрипт | Что делает |
|---|---|
| `build\deploy.ps1` | сборка + установка + запуск |
| `build\pack-release.ps1` | сборка + упаковка релизных архивов |
| `build\make-icon.ps1` | генерация иконки приложения |
| `build\make-screenshots.ps1` | скриншоты для README |
| `build\verify-locales.ps1` | проверка целостности таблицы переводов |
| `build\Uninstall.cmd` | деинсталлятор |

## Структура проекта

```
CopyPush/
├── src/CopyPush/            исходники приложения
│   ├── Program.cs           точка входа, сборка всех компонентов
│   ├── Interop/             P/Invoke: хук, буфер, окно, курсор, DPI
│   ├── Hooks/               KeyboardHook (WH_KEYBOARD_LL), MessageWindow
│   ├── Services/            CopyDetector, ThemeService, ToastService, SettingsService,
│   │                        TrayIcon, Loc (локализация), Logger
│   ├── Views/ToastWindow.cs плашка (интерфейс строится в коде, без XAML)
│   └── Resources/           иконка приложения
├── build/                   скрипты сборки, упаковки и деинсталлятор
├── docs/                    заметки релиза и изображения для README
├── .github/workflows/       CI (сборка) и Release (публикация по тегу)
└── global.json              фиксация версии .NET SDK (8.0.x)
```

## Лицензия

[MIT](LICENSE) © 2026 Иван Захаров
