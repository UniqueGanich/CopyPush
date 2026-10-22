<div align="center">

<img src="https://raw.githubusercontent.com/UniqueGanich/CopyPush/main/docs/images/icon-256.png" alt="CopyPush" width="96" height="96">

# CopyPush 1.0.0

[![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows)](https://microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/UniqueGanich/CopyPush/blob/main/LICENSE)
[![Languages](https://img.shields.io/badge/languages-10%20languages-0078D4)](https://github.com/UniqueGanich/CopyPush/blob/main/src/CopyPush/Services/Localization.cs)

</div>

---

## 🇬🇧 English

**A tiny Windows utility for Windows 10/11: whenever you copy with `Ctrl+C`, a neat
«Copied» toast pops up right at your mouse cursor — it rises smoothly and fades out.**

### ⬇ Download

| What to download | Size | Requires |
|---|---|---|
| [**CopyPush-1.0.0-win-x64.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64.zip) | 63.5 MB | nothing, works out of the box |
| [**CopyPush-1.0.0-win-x64-small.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64-small.zip) | 6.4 MB | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

**Install:** unpack the archive anywhere and run `CopyPush.exe` — an icon appears in the tray, that's it.
**Uninstall:** run `Uninstall.cmd` from the same folder.

### What's new

- **Global `Ctrl+C` binding** — works in every application, not just yours.
- **Breaks nothing.** `Ctrl+C` is never swallowed: the hook is *read-only*, so it still interrupts
  a process in the console and works as a normal game key in games.
- **No false positives.** The toast appears only if the clipboard really changed after the key press.
- **Finds its own language.** The interface and the «Copied» caption follow the system language:
  Русский, English, Українська, Deutsch, Français, Español, 日本語, 한국어, 简体中文, 繁體中文.
  If the system language is not supported, English is used automatically. Any language can be pinned
  manually: tray icon menu → **Language** (this item is always labelled in English so it can be found
  even if the interface ended up in an unfamiliar language).
- **System theme colours.** The background matches system windows, the text is contrast.
  The accent colour is taken from the Windows theme and picked up live.
- **Three styles:** system, accent, contrast.
- **Three positions:** at the cursor, screen centre, bottom right.
- **Three durations:** fast, normal, slow.
- **Never in the way.** The window never steals clicks, never activates and never shows in `Alt+Tab`.
- **Never over games and video.** If the active window covers the whole monitor, no toast is shown.
- **Runs at startup by default** — switch it off from the tray icon menu.
- **Logs everything that happens**, including unhandled errors — `%LOCALAPPDATA%\CopyPush\CopyPush.log`.

### Requirements

- Windows 10 build 19041 (20H1) or newer
- Windows 11 — fully supported, including live theme changes
- **No administrator rights required**

---

## 🇷🇺 Русский

**Маленькая утилита для Windows 10/11: при копировании по `Ctrl+C` у курсора мыши
всплывает плашка «Скопировано» — плавно поднимается вверх и растворяется.**

### ⬇ Скачать

| Что скачать | Размер | Требования |
|---|---|---|
| [**CopyPush-1.0.0-win-x64.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64.zip) | 63.5 МБ | ничего, работает сразу |
| [**CopyPush-1.0.0-win-x64-small.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64-small.zip) | 6.4 МБ | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

**Установка:** распакуйте архив в любую папку и запустите `CopyPush.exe` — появится иконка в трее, и всё готово.
**Удаление:** запустите `Uninstall.cmd` из той же папки.

### Что нового

- **Глобальная привязка `Ctrl+C`** — работает в любом приложении, а не только в вашем.
- **Ничего не ломает.** `Ctrl+C` не перехватывается: используется хук *на чтение*, поэтому
  в консоли он по-прежнему прерывает процесс, а в играх работает как обычная игровая клавиша.
- **Без ложных срабатываний.** Плашка появляется, только если содержимое буфера действительно
  изменилось после нажатия.
- **Язык подстраивается сам.** Интерфейс и надпись «Скопировано» берут язык системы:
  Русский, English, Українська, Deutsch, Français, Español, 日本語, 한국어, 简体中文, 繁體中文.
  Если языка системы нет — автоматически включается английский. Любой язык можно зафиксировать
  вручную: меню иконки в трее → **Language** (этот пункт всегда подписан по-английски, чтобы его
  можно было найти, даже если интерфейс оказался на незнакомом языке).
- **Цвет системной темы.** Фон — как у системных окон, текст контрастный. Акцентный цвет
  берётся из темы Windows и подхватывается на лету.
- **Три варианта оформления:** системный, акцентный, контрастный.
- **Три положения на экране:** у курсора, по центру экрана, в правом нижнем углу.
- **Три длительности:** быстро, обычно, медленно.
- **Не мешает.** Окно не перехватывает клики, не активируется и не появляется в `Alt+Tab`.
- **Не выскакивает поверх игр и видео.** Если активное окно занимает весь монитор, плашка не показывается.
- **Автозапуск включён по умолчанию** — отключается в меню иконки в трее.
- **Пишет в журнал всё, что происходит**, включая непойманные ошибки, — `%LOCALAPPDATA%\CopyPush\CopyPush.log`.

### Требования

- Windows 10 версии 19041 (20H1) или новее
- Windows 11 — полная поддержка, включая живую смену темы
- Права администратора **не нужны**

---

[Source and details / Исходный код и подробности](https://github.com/UniqueGanich/CopyPush) · [MIT](https://github.com/UniqueGanich/CopyPush/blob/main/LICENSE)
