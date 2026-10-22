<div align="center">

<img src="docs/images/icon-256.png" alt="CopyPush" width="96" height="96">

# CopyPush

**A small utility for Windows: when copying via `Ctrl+C`, a neat “Copied” banner pops up near the mouse cursor.**

[![Release](https://img.shields.io/github/v/release/UniqueGanich/CopyPush?label=Release&color=blue)](https://github.com/UniqueGanich/CopyPush/releases)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows)](https://microsoft.com/windows)
[![Languages](https://img.shields.io/badge/languages-ru%20%7C%20en%20%7C%20uk%20%7C%20de%20%7C%20fr%20%7C%20es%20%7C%20ja%20%7C%20ko%20%7C%20zh-0078D4)](src/CopyPush/Services/Localization.cs)

## **[English](README.md)**

</div>

---

> **⚠️ WARNING:**
>
> The project was completely created by an AI agent through OpenCode, because it seemed quite convenient to me
> have a copy notification, i don't promise 100% performance or beautiful code.
>
> You can contact me using the methods specified in your account.

![Screenshot](docs/images/hero.png)

# HOW TO CONFIGURE

Everything is configured in the tray icon menu:

| Item | Options |
|---|---|
| Show notification | on / off |
| Show on any buffer change | on / off (react to right‑click copy) |
| Position | at cursor / center of screen / bottom right corner |
| Design | system / accent / contrast |
| Duration | fast / normal / slow |
| **Language** | automatically (system) / Русский / English / Українська / Deutsch / Français / Español / 日本語 / 한국어 / 简体中文 / 繁體中文 |
| Run with Windows | on / off |

## Three design options

| System | Accent | Contrast |
|:---:|:---:|:---:|
| ![](docs/images/style-system.png) | ![](docs/images/style-accent.png) | ![](docs/images/style-contrast.png) |

## Installation

Ready-made builds are available in [Releases](https://github.com/UniqueGanich/CopyPush/releases):

| What to download | Size | Requirements |
|---|---|---|
| [CopyPush-1.1.0.zip](https://github.com/UniqueGanich/CopyPush/releases/download/v1.1.0/CopyPush-1.1.0.zip ) | 55.6 MB | Windows 10/11, works out of the box |
| [CopyPush-1.1.0-Without-NET.zip](https://github.com/UniqueGanich/CopyPush/releases/download/v1.1.0/CopyPush-1.1.0-Without-NET.zip) | 0.2 MB | Windows 10/11, [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

1. Download the archive and unpack it into any folder (for example, `C:\CopyPush`).
2. Run `CopyPush.exe` - an icon will appear in the tray.
3. Done: `Ctrl+C` in any application will display the “Copied” prompt.

Autostart is enabled by default — it can be disabled via the tray icon menu; when autostart is disabled, the program disappears from the list in the Task Manager - switch it in the tray.

## Uninstallation

Run `Uninstall.cmd` from the program folder - it will remove autostart, settings, and the program itself.
You can also uninstall via “Settings → Apps”.

## Requirements

- Windows 10 version 19041 (20H1) or newer
- Windows 11 - full support, including live theme change

## What he can do

- **Works in any application.**
- **There are no false positives.** The bar appears only if the contents of the buffer are
  it really changed after clicking.
- **Language is not a hindrance.** The interface and the "Copied" label can be displayed in the following languages:
  Русский, English, Українська, Deutsch, Français, Español, 日本語, 한국어, 简体中文, 繁體中文
  The language can be fixed manually: menu tray icons → **Language** (this item is always
  it is signed in English so that it can be found, even if the interface turns out to be
  in an unknown language). I can add new languages on request.
- **The color of the system theme.** The background is like that of the system windows, the text is contrasting.
  The accent color is taken from the Windows theme and picked up on the fly.
- ** Does not interfere.**The window does not intercept clicks, does not become active, and does not appear in `Alt+Tab`.**
- **It does not pop up over games and videos.** If the active window occupies the entire monitor, the overlay is not displayed.
- **It logs everything that happens** — including uncaught errors — so that if something goes wrong, the cause will be visible. Log: `%LOCALAPPDATA%\CopyPush\CopyPush.log`.

## Language

By default, the language is taken from the system: `Windows → Settings → Time & language → Language & region →
Preferred languages`. If Russian is listed first there, the interface and text will be in Russian; if English, in English. Any of the ten supported languages can be fixed manually: the tray icon menu → **Language**. After changing the Windows language, it is enough to restart CopyPush.

# Technical details
## How it works

```
Ctrl+C  ──►  WH_KEYBOARD_LL (observation, suppresses nothing)
                    │
                    ├──►  flag “copy requested” (window 1.5 s)
                    │
 WM_CLIPBOARDUPDATE ─┘
        buffer changed?
             │ yes                    │ no
             ▼                       ▼
        show a pop‑up        do nothing
```

Thanks to the “buffer actually changed” check, `Ctrl+C` in the terminal (this is a process interruption) and in games do not trigger false notifications.

Technically:

- **Hook** - `SetWindowsHookEx(WH_KEYBOARD_LL)`. The handler always returns
  `CallNextHookEx`, so the input goes to the application unchanged.
- **Confirmation** - `AddClipboardFormatListener` + `WM_CLIPBOARDUPDATE`,
  plus `GetClipboardSequenceNumber` as a backup.
- **Overlay** - a WPF window with `AllowsTransparency` (layered window, per‑pixel alpha),
  extended styles `WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT`.
- **Animation** - `Storyboard` over `RenderTransform` (rise by 48 px) and `Opacity`.
- **Theme** - `AppsUseLightTheme` + accent from `Explorer\Accent\AccentPalette`
  and WinRT `UISettings`; colors are reloaded when the theme is changed.
- **Language** - `CultureInfo.CurrentUICulture` (system language) or the one fixed in the settings.

## Building from source

You need [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/UniqueGanich/CopyPush.git
cd CopyPush

# one .exe with everything inside (does not require .NET on the computer)
dotnet publish src/CopyPush/CopyPush.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true -o out
```

Scripts in `build/`:

| Script | What it does |
|---|---|
| `build\deploy.ps1` | build + install + run |
| `build\pack-release.ps1` | build + package release archives |
| `build\make-icon.ps1` | generate the application icon |
| `build\make-screenshots.ps1` | screenshots for README |
| `build\verify-locales.ps1` | checks the integrity of the translation table |
| `build\verify-project.ps1` | checks namespaces and homoglyphs in the code |
| `build\Uninstall.cmd` | uninstaller |

## Tests

The repository contains a set of localization checks - a separate project without external dependencies:

```bash
dotnet run --project tests/CopyPush.Tests --configuration Release
```

It checks that the system language is correctly displayed in the supported language (including the division of Chinese into simplified and traditional by region) and that for a language that is not in the list, English is automatically selected. The tests are run in CI.

## Project structure

```
CopyPush/
├── src/CopyPush/            application source code
│   ├── Program.cs           entry point, assembly of all components
│   ├── Interop/             P/Invoke: hook, buffer, window, cursor, DPI
│   ├── Hooks/               KeyboardHook (WH_KEYBOARD_LL), MessageWindow
│   ├── Services/            CopyDetector, ThemeService, ToastService, SettingsService,
│   │                        TrayIcon, Localization (localization), Logger
│   ├── Views/ToastWindow.cs placeholder (the interface is built in code, without XAML)
│   └── Resources/           application icon
,── build/ build, packaging, and uninstaller scripts
├── tests/CopyPush.Tests/    language selection checks
├── docs/                    Release notes and images for
the README .github/workflows/       CI (build) and Release (publication by tag)
└── global.json version commit .NET SDK (8.0.x)
```

## License

[MIT](LICENSE) © 2026 Ivan Zakharov
