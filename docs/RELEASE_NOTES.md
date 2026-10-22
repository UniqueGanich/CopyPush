# CopyPush 1.1.0

[![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows)](https://microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green)](https://github.com/UniqueGanich/CopyPush/blob/main/LICENSE)

</div>

---

## 🇬🇧 English

## What's new

- **Smaller binaries** — removed `WinRT UISettings` dependency (accent colour now read from registry `Explorer\Accent\AccentPalette`), excluded unused framework assemblies (`WinForms.Design`, `DiaSymReader`, `System.Xml`, `System.Data`, `System.Net`), dropped `Windows SDK` and `WinRT` projections from the bundle.
  - Full (self-contained): **68.88 → 55.65 MB**
  - Small (framework-dependent): **24.45 → 0.22 MB**
- **Accent colour via registry** — replaced `WinRtThemeProbe` (WinRT `UISettings`) with `AccentPaletteReader` reading `HKCU\...\Explorer\Accent\AccentPalette`. Values are identical to WinRT; archive shrinks by ~24 MB.

---

## 🇷🇺 Русский

## Что нового

- **Меньше размеров сборок** — убрана зависимость от `WinRT UISettings` (акцентный цвет теперь читается из реестра `Explorer\Accent\AccentPalette`), исключены неиспользуемые сборки фреймворка (`WinForms.Design`, `DiaSymReader`, `System.Xml`, `System.Data`, `System.Net`), удалены проекции Windows SDK и WinRT из бандла.
  - Полная версия (self-contained): **68.88 → 55.65 МБ**
  - Мелкая версия (framework-dependent): **24.45 → 0.22 МБ**
- **Акцентный цвет через реестр** — заменён `WinRtThemeProbe` (WinRT `UISettings`) на `AccentPaletteReader`, читающий `HKCU\...\Explorer\Accent\AccentPalette`. Значения идентичны WinRT; архив стал легче на ~24 МБ.
