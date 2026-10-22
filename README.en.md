<div align="center">

<img src="docs/images/icon-256.png" alt="CopyPush" width="96" height="96">

# CopyPush

**A tiny Windows utility: whenever you copy with `Ctrl+C`, a neat «Copied» toast
pops up right at your mouse cursor.**

[![Release](https://img.shields.io/github/v/release/UniqueGanich/CopyPush?label=Release&color=blue)](https://github.com/UniqueGanich/CopyPush/releases)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows)](https://microsoft.com/windows)
[![Languages](https://img.shields.io/badge/languages-ru%20%7C%20en%20%7C%20uk%20%7C%20de%20%7C%20fr%20%7C%20es%20%7C%20ja%20%7C%20ko%20%7C%20zh-0078D4)](src/CopyPush/Services/Loc.cs)

[Русский](README.md)

</div>

---

![Screenshot](docs/images/hero.png)

## Why

Pressing `Ctrl+C` and **not knowing whether it worked** is a small but constant
annoyance — you have to paste somewhere just to check. CopyPush removes the question
with a single toast right at your cursor.

## Features

- **Global `Ctrl+C` binding** — works in every application, not just one.
- **Breaks nothing.** `Ctrl+C` is never swallowed: a *read-only* hook is used, so it
  still interrupts a process in the terminal and still acts as a game key.
- **No false positives.** The toast appears only if the clipboard actually changed
  after the keypress.
- **Finds its own language.** the interface and the «Copied» label follow the Windows
  language (русский / english / українська / deutsch / français / español / 日本語 / 한국어 / 简体中文 / 繁體中文). Any of them
  can be pinned manually from the tray menu.
- **Follows the system theme.** The surface matches system windows, the text is
  contrasting. The accent colour is read from the Windows theme and picked up live.
- **Stays out of the way.** The window does not steal clicks, does not activate,
  and never shows up in `Alt+Tab`.
- **Keeps a log** at `%LOCALAPPDATA%\CopyPush\CopyPush.log` in case something
  misbehaves.

## Installation

Prebuilt binaries live in [Releases](https://github.com/UniqueGanich/CopyPush/releases):

| What to download | Size | Requires |
|---|---|---|
| [**CopyPush-1.0.0-win-x64.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64.zip) | ~63 MB | nothing, works out of the box on Windows 10/11 |
| [**CopyPush-1.0.0-win-x64-small.zip**](https://github.com/UniqueGanich/CopyPush/releases/download/v1.0.0/CopyPush-1.0.0-win-x64-small.zip) | ~6 MB | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

1. Download the archive and unpack it anywhere (e.g. `C:\Tools\CopyPush`).
2. Run `CopyPush.exe` — an icon appears in the tray.
3. Done. `Ctrl+C` in any app now shows the «Copied» toast.

Autostart is **off** by default — enable it from the tray menu
(“Run at startup”).

### Uninstall

Run `Uninstall.cmd` from the program folder. It removes autostart, settings and
the program itself. You can also uninstall it from Windows Settings → Apps.

## Settings

Everything lives in the tray icon menu:

| Item | Options |
|---|---|
| Show notification | on / off |
| Show on any clipboard change | on / off (also react to right-click → Copy) |
| Position | at cursor / screen center / bottom right |
| Style | system / accent / contrast |
| Duration | fast / normal / slow |
| **Language** | automatic (system) / русский / english / українська / deutsch / français / español / 日本語 / 한국어 / 简体中文 / 繁體中文 |
| Run at startup | on / off |

### The three styles

| System | Accent | Contrast |
|:---:|:---:|:---:|
| ![](docs/images/style-system.png) | ![](docs/images/style-accent.png) | ![](docs/images/style-contrast.png) |

### Language

By default the language comes from the system: `Windows → Settings → Time & language →
Language & region → Preferred languages`. If Russian is listed first, the interface and
the label will be Russian; if English — English. Any of the six supported languages can
be pinned manually: tray icon menu → **Language**. After changing the Windows language,
just restart CopyPush.

## How it works

```
Ctrl+C  ──►  WH_KEYBOARD_LL (observation only, never swallows the key)
                    │
                    ├──►  "copy requested" flag (1.5 s window)
                    │
 WM_CLIPBOARDUPDATE ─┘
        did the clipboard change?
             │ yes                   │ no
             ▼                       ▼
        show the toast          do nothing
```

Because the app verifies that the clipboard actually changed, `Ctrl+C` in a
terminal (which interrupts the process) and in games never trigger a false
notification.

Technically:

- **Hook** — `SetWindowsHookEx(WH_KEYBOARD_LL)`. The callback always returns
  `CallNextHookEx`, so input reaches the app unchanged.
- **Confirmation** — `AddClipboardFormatListener` + `WM_CLIPBOARDUPDATE`,
  with `GetClipboardSequenceNumber` as a fallback.
- **Toast** — a WPF window with `AllowsTransparency` (layered window, per-pixel
  alpha) and `WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT`.
- **Animation** — a `Storyboard` over `RenderTransform` (48 px rise) and `Opacity`.
- **Theme** — `AppsUseLightTheme` plus the accent from `Explorer\Accent\AccentPalette`
  and WinRT `UISettings`; colours are re-read when the theme changes.
- **Language** — `CultureInfo.CurrentUICulture` (system language) or the language
  pinned in the settings.

## Requirements

- Windows 10 build 19041 (20H1) or newer
- Windows 11 — fully supported, including live theme changes
- **No administrator rights required**

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/UniqueGanich/CopyPush.git
cd CopyPush

# single .exe with everything inside (no .NET needed on the target machine)
dotnet publish src/CopyPush/CopyPush.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true -o out
```

Scripts in `build/`:

| Script | What it does |
|---|---|
| `build\deploy.ps1` | build + install + launch |
| `build\pack-release.ps1` | build + package the release archives |
| `build\make-icon.ps1` | generate the application icon |
| `build\make-screenshots.ps1` | screenshots for the README |
| `build\verify-locales.ps1` | check the translation table for missing keys |
| `build\Uninstall.cmd` | uninstaller |

## Project layout

```
CopyPush/
├── src/CopyPush/            application sources
│   ├── Program.cs           entry point, wires everything together
│   ├── Interop/             P/Invoke: hook, clipboard, window, cursor, DPI
│   ├── Hooks/               KeyboardHook (WH_KEYBOARD_LL), MessageWindow
│   ├── Services/            CopyDetector, ThemeService, ToastService, SettingsService,
│   │                        TrayIcon, Loc (localisation), Logger
│   ├── Views/ToastWindow.cs the toast (UI built in code, no XAML)
│   └── Resources/           application icon
├── build/                   build, packaging and uninstall scripts
├── docs/                    release notes and README images
├── .github/workflows/       CI (build) and Release (publish on tag)
└── global.json              pins the .NET SDK version (8.0.x)
```

## License

[MIT](LICENSE) © 2026 Ivan Zakharov
