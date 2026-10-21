# CopyPush 1.0.0

Маленькая утилита для Windows 10/11: при копировании по **Ctrl+C** у курсора мыши
всплывает плашка **«Скопировано»** — плавно поднимается вверх и растворяется.

---

## ⬇ Скачать / Download

| Файл | Размер | Требования / Requires |
|---|---|---|
| [**CopyPush-1.0.0-win-x64.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64.zip) | ~63 МБ | ничего, работает сразу / nothing, works out of the box |
| [**CopyPush-1.0.0-win-x64-small.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64-small.zip) | ~6 МБ | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

**Установка / Install:** распакуйте в любую папку и запустите `CopyPush.exe`.
Появится иконка в трее — всё, готово.
Unpack anywhere and run `CopyPush.exe` — an icon appears in the tray, that's it.

**Удаление / Uninstall:** запустите `Uninstall.cmd` из той же папки.
Run `Uninstall.cmd` from the same folder.

---

## Что нового

- Глобальная привязка `Ctrl+C` — работает в любом приложении
- `Ctrl+C` не перехватывается: хук работает только на чтение
- Никаких ложных срабатываний: плашка появляется, только если буфер реально изменился
- **Язык подстраивается сам** — русский, english, українська, deutsch, français, español
- Три варианта оформления: системный, акцентный, контрастный
- Цвета системной темы, акцент подхватывается на лету
- Окно не перехватывает клики, не активируется, не светится в `Alt+Tab`
- Журнал событий в `%LOCALAPPDATA%\CopyPush\CopyPush.log`

## Requirements

- Windows 10 build 19041 (20H1) or newer / Windows 11
- No administrator rights required / права администратора не нужны

---

<details>
<summary><b>English</b></summary>

## What it does

A tiny Windows utility: whenever you copy with **Ctrl+C**, a neat **«Copied»** toast
pops up right at your mouse cursor — it rises smoothly and fades out.

- **Global `Ctrl+C` binding** — works in every application
- **Breaks nothing** — `Ctrl+C` is never swallowed, the hook is read-only
- **No false positives** — the toast appears only if the clipboard really changed
- **Finds its own language** — русский, english, українська, deutsch, français, español
- Three styles: system, accent, contrast
- Follows the system theme, accent colour picked up live
- The window never steals clicks, never activates, never shows in `Alt+Tab`
- Event log at `%LOCALAPPDATA%\CopyPush\CopyPush.log`

## Requirements

- Windows 10 build 19041 (20H1) or newer / Windows 11
- No administrator rights required

</details>

---

[Исходный код и подробности / Source and details](https://github.com/UniqueGanich/CopyPush) · [MIT](https://github.com/UniqueGanich/CopyPush/blob/main/LICENSE)
