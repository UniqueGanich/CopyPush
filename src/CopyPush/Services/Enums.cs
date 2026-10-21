namespace CopyPush.Services;

/// <summary>Toast appearance variant.</summary>
internal enum ToastStyle
{
    System = 0,
    Accent = 1,
    Contrast = 2,
}

/// <summary>Toast position on the screen.</summary>
internal enum ToastPosition
{
    AtCursor = 0,
    ScreenCenter = 1,
    BottomRight = 2,
}

/// <summary>Animation duration.</summary>
internal enum ToastDuration
{
    Fast = 0,
    Normal = 1,
    Slow = 2,
}