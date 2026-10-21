using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CopyPush.Services;
namespace CopyPush.Services;

/// <summary>
/// Иконка в трее + меню настроек.
/// Иконка рисуется в runtime (GDI+), поэтому бинарных ресурсов в проекте нет.
/// </summary>
internal sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly SettingsService _settings;
    private readonly ThemeService _theme;
    private readonly Action _onTest;
    private readonly Action _onExit;

    private Bitmap? _bitmap;
    private Icon? _icon;

    public TrayIcon(SettingsService settings, ThemeService theme, Action onTest, Action onExit)
    {
        _settings = settings;
        _theme = theme;
        _onTest = onTest;
        _onExit = onExit;

        _notifyIcon = new NotifyIcon
        {
            Icon = BuildIcon(),
            Text = "CopyPush",
            Visible = true,
        };

        _notifyIcon.DoubleClick += (_, _) => _onTest();
        _notifyIcon.ContextMenuStrip = BuildMenu();

        _theme.Changed += () => RefreshIcon();
    }

    public void ShowBalloon(string title, string text)
    {
        try
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = text;
            _notifyIcon.ShowBalloonTip(2000);
        }
        catch
        {
            // уведомления Windows могут быть отключены - не критично
        }
    }

    private void RefreshIcon()
    {
        var newIcon = BuildIcon();
        _notifyIcon.Icon = newIcon;

        if (_icon is not null) _icon.Dispose();
        _icon = newIcon;

        if (_bitmap is not null) _bitmap.Dispose();
        _bitmap = null;
    }

    private Icon BuildIcon()
    {
        try
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var (surface, foreground, _) = _theme.Resolve(ToastStyle.System);

                // Иконка в трее: акцентный круг с белой галочкой - заметна
                // и на светлой, и на тёмной панели задач.
                using (var fill = new SolidBrush(ToDrawing(_theme.Accent)))
                {
                    g.FillEllipse(fill, 1, 1, 30, 30);
                }

                using (var pen = new Pen(Color.White, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLines(pen, new[]
                    {
                        new PointF(9.5f, 16.5f),
                        new PointF(14f, 21.5f),
                        new PointF(23f, 11.5f),
                    });
                }
            }

            _bitmap = bmp;
            _icon = Icon.FromHandle(bmp.GetHicon());
            return _icon;
        }
        catch
        {
            return SystemIcons.Application;
        }
    }

    private static Color ToDrawing(System.Windows.Media.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        var enabled = new ToolStripMenuItem(Loc.Get("menu.notifications"))
        {
            Checked = _settings.Enabled,
            CheckOnClick = false,
        };
        enabled.Click += (_, _) =>
        {
            _settings.Enabled = !_settings.Enabled;
            enabled.Checked = _settings.Enabled;
            _settings.Save();
        };
        menu.Items.Add(enabled);

        var anyChange = new ToolStripMenuItem(Loc.Get("menu.anyClipboardChange"))
        {
            Checked = _settings.ShowOnAnyClipboardChange,
        };
        anyChange.Click += (_, _) =>
        {
            _settings.ShowOnAnyClipboardChange = !_settings.ShowOnAnyClipboardChange;
            anyChange.Checked = _settings.ShowOnAnyClipboardChange;
            _settings.Save();
        };
        menu.Items.Add(anyChange);

        menu.Items.Add(new ToolStripSeparator());

        var position = new ToolStripMenuItem(Loc.Get("menu.position"));
        position.DropDownItems.Add(Radio(Loc.Get("menu.position.cursor"), ToastPosition.AtCursor));
        position.DropDownItems.Add(Radio(Loc.Get("menu.position.center"), ToastPosition.ScreenCenter));
        position.DropDownItems.Add(Radio(Loc.Get("menu.position.bottomRight"), ToastPosition.BottomRight));
        menu.Items.Add(position);

        var style = new ToolStripMenuItem(Loc.Get("menu.style"));
        style.DropDownItems.Add(Radio(Loc.Get("menu.style.system"), ToastStyle.System));
        style.DropDownItems.Add(Radio(Loc.Get("menu.style.accent"), ToastStyle.Accent));
        style.DropDownItems.Add(Radio(Loc.Get("menu.style.contrast"), ToastStyle.Contrast));
        menu.Items.Add(style);

        var duration = new ToolStripMenuItem(Loc.Get("menu.duration"));
        duration.DropDownItems.Add(Radio(Loc.Get("menu.duration.fast"), ToastDuration.Fast));
        duration.DropDownItems.Add(Radio(Loc.Get("menu.duration.normal"), ToastDuration.Normal));
        duration.DropDownItems.Add(Radio(Loc.Get("menu.duration.slow"), ToastDuration.Slow));
        menu.Items.Add(duration);

        var language = new ToolStripMenuItem(Loc.Get("menu.language"));
        language.DropDownItems.Add(LanguageItem(Loc.Get("menu.language.auto"), Loc.Auto));
        foreach (var code in Loc.Supported)
        {
            language.DropDownItems.Add(LanguageItem(Loc.LanguageName(code), code));
        }
        menu.Items.Add(language);

        menu.Items.Add(new ToolStripSeparator());

        var startup = new ToolStripMenuItem(Loc.Get("menu.startup"))
        {
            Checked = _settings.RunAtStartup,
        };
        startup.Click += (_, _) =>
        {
            _settings.RunAtStartup = !_settings.RunAtStartup;
            startup.Checked = _settings.RunAtStartup;
            _settings.Save();
            _settings.ApplyAutoStart();
        };
        menu.Items.Add(startup);

        menu.Items.Add(new ToolStripSeparator());

        var test = new ToolStripMenuItem(Loc.Get("menu.showTest"));
        test.Click += (_, _) => _onTest();
        menu.Items.Add(test);

        var exit = new ToolStripMenuItem(Loc.Get("menu.exit"));
        exit.Click += (_, _) => _onExit();
        menu.Items.Add(exit);

        return menu;
    }

    /// <summary>Пункт меню выбора языка.</summary>
    private ToolStripMenuItem LanguageItem(string text, string code)
    {
        var item = new ToolStripMenuItem(text)
        {
            Checked = string.Equals(_settings.Language, code, StringComparison.Ordinal),
        };

        item.Click += (_, _) =>
        {
            _settings.Language = code;
            _settings.Save();

            // Пересобираем меню на новом языке.
            Loc.Apply(code);
            RefreshCheckStates();
        };

        return item;
    }

    private ToolStripMenuItem Radio(string text, object value)
    {
        var item = new ToolStripMenuItem(text);

        var current = value switch
        {
            ToastPosition p => _settings.Position == p,
            ToastStyle s => _settings.Style == s,
            ToastDuration d => _settings.Duration == d,
            _ => false,
        };

        item.Checked = current;

        item.Click += (_, _) =>
        {
            switch (value)
            {
                case ToastPosition p: _settings.Position = p; break;
                case ToastStyle s: _settings.Style = s; break;
                case ToastDuration d: _settings.Duration = d; break;
            }
            _settings.Save();
            RefreshCheckStates();
        };

        return item;
    }

    private ContextMenuStrip? _menu;

    private void RefreshCheckStates()
    {
        // Меню пересоздаётся проще, чем синхронизировать галки по дереву.
        var old = _notifyIcon.ContextMenuStrip;
        _menu = BuildMenu();
        _notifyIcon.ContextMenuStrip = _menu;
        old?.Dispose();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon?.Dispose();
        _bitmap?.Dispose();
    }
}
