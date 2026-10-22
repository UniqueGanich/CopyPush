using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using CopyPush.Interop;
using CopyPush.Services;

// UseWindowsForms=true тянет за собой System.Drawing, где тоже есть Color и Point.
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Brushes = System.Windows.Media.Brushes;

namespace CopyPush.Views;

/// <summary>
/// Всплывающая плашка "Скопировано".
///
/// Окно: AllowsTransparency -> layered window -> попиксельная альфа,
/// поэтому и тень, и затухание выглядят мягко.
/// Расширенные стили WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT:
/// окно не активируется, не перехватывает клики и не попадает в Alt+Tab.
///
/// Интерфейс строится в коде (без XAML) - так меньше сборочных зависимостей.
/// </summary>
internal sealed class ToastWindow : Window
{
    private const double CursorGapPx = 26;
    private const double StackStepPx = 40;

    /// <summary>Насколько плашка поднимается за время жизни.</summary>
    private const double RiseDistance = 48;

    private readonly Grid _host;
    private readonly Border _root;
    private readonly TextBlock _icon;
    private readonly TextBlock _label;
    private ToastDuration _duration;
    private double _contentHeight;
    private Storyboard? _storyboard;
    private bool _everShown;

    private Color _foreground = Colors.White;

    public ToastWindow(ThemeService theme, ToastStyle style, ToastDuration duration)
    {
        _duration = duration;

        Title = "CopyPush";
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        Topmost = true;
        ShowActivated = false;
        Focusable = false;
        IsHitTestVisible = false;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        WindowStartupLocation = WindowStartupLocation.Manual;

        _icon = new TextBlock
        {
            Text = "\uE73E", // галочка в Segoe Fluent Icons / Segoe MDL2 Assets
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
        };

        _label = new TextBlock
        {
            Text = Loc.Get("toast.copied"),
            Margin = new Thickness(8, 0, 0, 0),
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, Tahoma"),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(_icon);
        panel.Children.Add(_label);

        _root = new Border
        {
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(14, 8, 14, 8),
            BorderThickness = new Thickness(1),
            Child = panel,
            Effect = new DropShadowEffect
            {
                BlurRadius = 20,
                ShadowDepth = 3,
                Direction = 270,
                Color = Colors.Black,
                Opacity = 0.5,
            },
        };

        _host = new Grid { Opacity = 0 };
        // Контент прижат к низу окна: окно на RiseDistance выше самого пилюля,
        // чтобы во время подъёма ничего не обрезалось границами окна.
        _root.VerticalAlignment = VerticalAlignment.Bottom;
        _root.HorizontalAlignment = HorizontalAlignment.Left;
        _host.Children.Add(_root);
        Content = _host;

        Prepare(theme, style, duration);
    }

    /// <summary>
    /// Готовит окно к показу: цвета, текст, длительность.
    /// Вызывается при создании и при каждом повторном использовании окна.
    /// </summary>
    public void Prepare(ThemeService theme, ToastStyle style, ToastDuration duration)
    {
        _duration = duration;

        ApplyTheme(theme, style);
        _label.Text = Loc.Get("toast.copied");
    }

    private void ApplyTheme(ThemeService theme, ToastStyle style)
    {
        var (surface, foreground, border) = theme.Resolve(style);
        _foreground = foreground;

        _root.Background = new SolidColorBrush(surface);
        _root.BorderBrush = new SolidColorBrush(border);
        _root.BorderThickness = border.A == 0 ? new Thickness(0) : new Thickness(1);

        var fgBrush = new SolidColorBrush(foreground);
        _icon.Foreground = fgBrush;
        _label.Foreground = fgBrush;

        if (_root.Effect is DropShadowEffect shadow)
        {
            shadow.Color = theme.Shadow;
            shadow.Opacity = theme.Shadow.A / 255.0;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;
        int ex = NativeMethods.GetWindowLong32(hwnd, NativeMethods.GWL_EXSTYLE);
        ex |= NativeMethods.WS_EX_NOACTIVATE
            | NativeMethods.WS_EX_TOOLWINDOW
            | NativeMethods.WS_EX_TRANSPARENT
            | NativeMethods.WS_EX_LAYERED;
        NativeMethods.SetWindowLong32(hwnd, NativeMethods.GWL_EXSTYLE, ex);
    }

    /// <summary>Анимация завершена, окно спрятано и готово к повторному показу.</summary>
    public event Action<ToastWindow>? Finished;

    /// <summary>Показывает окно в нужной точке и запускает анимацию.</summary>
    public void ShowAt(ToastPosition position, int stackIndex, double workAreaLeft, double workAreaTop, double workAreaWidth, double workAreaHeight)
    {
        // Повторное использование окна: гасим предыдущую анимацию и прозрачность.
        if (_storyboard is not null)
        {
            try { _storyboard.Stop(); } catch { /* уже остановлена */ }
            _storyboard = null;
        }

        _host.Opacity = 0;
        _host.Visibility = Visibility.Visible;

        // Замеряем сам пилюль (а не окно) - у окна DesiredSize на этом этапе ещё пустой.
        _root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var desired = _root.DesiredSize;
        if (desired.Width <= 0 || double.IsNaN(desired.Width)) desired = new Size(150, 38);

        _contentHeight = desired.Height;

        // Окно на RiseDistance выше пилюля: пилюль прижат к низу и поднимается внутри окна.
        Width = desired.Width;
        Height = desired.Height + RiseDistance;
        SizeToContent = SizeToContent.Manual;

        double left;
        double top;

        switch (position)
        {
            case ToastPosition.ScreenCenter:
                left = workAreaLeft + (workAreaWidth - desired.Width) / 2;
                top = workAreaTop + (workAreaHeight - desired.Height) / 2 - RiseDistance / 2;
                break;

            case ToastPosition.BottomRight:
                left = workAreaLeft + workAreaWidth - desired.Width - 24;
                top = workAreaTop + workAreaHeight - desired.Height - 24 - RiseDistance;
                break;

            default:
                {
                    var cursor = CursorPositionInDip();
                    left = cursor.X - desired.Width / 2;
                    // Низ пилюля - на CursorGapPx выше курсора.
                    top = cursor.Y - CursorGapPx - desired.Height - RiseDistance;
                    break;
                }
        }

        // Стек: каждый следующий тост выше предыдущего.
        top -= stackIndex * StackStepPx;

        // Не выпускаем за пределы рабочей области.
        left = Math.Clamp(left, workAreaLeft + 4, workAreaLeft + Math.Max(4, workAreaWidth - desired.Width - 4));
        top = Math.Clamp(top, workAreaTop + 4, workAreaTop + Math.Max(4, workAreaHeight - Height - 4));

        Left = left;
        Top = top;

        // Первое показание создаёт HWND; при повторном использовании окно просто
        // показывается заново, без пересоздания и без задержки на первом кадре.
        if (!_everShown)
        {
            Show();
            _everShown = true;
        }
        else
        {
            Visibility = Visibility.Visible;
        }

        // После показа размер известен точно - уточняем позицию.
        double actualW = ActualWidth > 0 ? ActualWidth : desired.Width;
        double actualH = ActualHeight > 0 ? ActualHeight : Height;

        Logger.Info($"toast shown at {Left:0},{Top:0} ({ActualWidth:0}x{ActualHeight:0})");

        if (position == ToastPosition.AtCursor)
        {
            var cursor = CursorPositionInDip();
            Left = Math.Clamp(
                cursor.X - actualW / 2,
                workAreaLeft + 4,
                workAreaLeft + Math.Max(4, workAreaWidth - actualW - 4));
            Top = Math.Clamp(
                cursor.Y - CursorGapPx - _contentHeight - RiseDistance - stackIndex * StackStepPx,
                workAreaTop + 4,
                workAreaTop + Math.Max(4, workAreaHeight - actualH - 4));
        }

        StartAnimation();
    }

    private void StartAnimation()
    {
        var (totalMs, fadeOutStartMs) = _duration switch
        {
            ToastDuration.Fast => (700, 300),
            ToastDuration.Slow => (1500, 700),
            _ => (1100, 495),
        };

        var scale = new ScaleTransform(0.88, 0.88);
        var translate = new TranslateTransform(0, 0);
        var group = new TransformGroup();
        group.Children.Add(scale);
        group.Children.Add(translate);

        _root.RenderTransform = group;
        _root.RenderTransformOrigin = new Point(0.5, 0.5);

        var easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
        var easeIn = new CubicEase { EasingMode = EasingMode.EaseIn };

        var storyboard = new Storyboard();

        // 1. Проявление 0 -> 1
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = easeOut };
        Storyboard.SetTarget(fadeIn, _host);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeIn);

        // 2. Масштаб 0.88 -> 1.0
        var scaleX = new DoubleAnimation(0.88, 1.0, TimeSpan.FromMilliseconds(180)) { EasingFunction = easeOut };
        Storyboard.SetTarget(scaleX, scale);
        Storyboard.SetTargetProperty(scaleX, new PropertyPath(ScaleTransform.ScaleXProperty));
        storyboard.Children.Add(scaleX);

        var scaleY = new DoubleAnimation(0.88, 1.0, TimeSpan.FromMilliseconds(180)) { EasingFunction = easeOut };
        Storyboard.SetTarget(scaleY, scale);
        Storyboard.SetTargetProperty(scaleY, new PropertyPath(ScaleTransform.ScaleYProperty));
        storyboard.Children.Add(scaleY);

        // 3. Подъём
        var rise = new DoubleAnimation(0, -RiseDistance, TimeSpan.FromMilliseconds(totalMs)) { EasingFunction = easeOut };
        Storyboard.SetTarget(rise, translate);
        Storyboard.SetTargetProperty(rise, new PropertyPath(TranslateTransform.YProperty));
        storyboard.Children.Add(rise);

        // 4. Затухание в последней части подъёма
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(Math.Max(100, totalMs - fadeOutStartMs)))
        {
            BeginTime = TimeSpan.FromMilliseconds(fadeOutStartMs),
            EasingFunction = easeIn,
        };
        Storyboard.SetTarget(fadeOut, _host);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeOut);

        storyboard.Completed += (_, _) =>
        {
            // Окно не закрываем, а прячем: оно переиспользуется при следующем
            // копировании. Создание нового WPF-окна с тенью каждый раз - это
            // лишние аллокации и задержка появления.
            try
            {
                Visibility = Visibility.Hidden;
                Finished?.Invoke(this);
            }
            catch
            {
                // уже закрыто
            }
        };

        _storyboard = storyboard;
        storyboard.Begin(this, isControllable: true);
    }

    /// <summary>Позиция курсора в DIP-координатах текущего монитора.</summary>
    private static Point CursorPositionInDip()
    {
        if (!NativeMethods.GetCursorPos(out var p))
        {
            return new Point(0, 0);
        }

        var monitor = NativeMethods.MonitorFromPoint(p, NativeMethods.MONITOR_DEFAULTTONEAREST);
        double scale = 1.0;

        try
        {
            if (NativeMethods.GetDpiForMonitor(monitor, NativeMethods.MDT_EFFECTIVE_DPI, out uint dpiX, out _) == 0 && dpiX > 0)
            {
                scale = dpiX / 96.0;
            }
        }
        catch
        {
            scale = 1.0;
        }

        return new Point(p.X / scale, p.Y / scale);
    }
}
