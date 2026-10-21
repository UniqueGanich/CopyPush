# Снимки экрана для README: hero-кадр и все три варианта оформления плашки.
# Запуск:  powershell -ExecutionPolicy Bypass -File build\make-screenshots.ps1
param(
    [string]$ProjectDir = (Split-Path -Parent $PSScriptRoot)
)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public class D {
  public delegate bool EnumProc(IntPtr h, IntPtr l);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr l);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern uint SendInput(uint n, INPUT[] p, int size);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr c);
  [DllImport("user32.dll")] public static extern int GetSystemMetrics(int i);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  [StructLayout(LayoutKind.Sequential)]
  public struct INPUT { public uint type; public InputUnion U; }
  [StructLayout(LayoutKind.Explicit)]
  public struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public HARDWAREINPUT hi; }
  [StructLayout(LayoutKind.Sequential)]
  public struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
  [StructLayout(LayoutKind.Sequential)]
  public struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
  [StructLayout(LayoutKind.Sequential)]
  public struct HARDWAREINPUT { public uint uMsg; public ushort wParamL; public ushort wParamH; }
  public static void SendCtrlC() {
    const uint KEYUP = 0x0002;
    var i = new INPUT[4];
    for (int k = 0; k < 4; k++) { i[k] = new INPUT(); i[k].type = 1; }
    i[0].U.ki = new KEYBDINPUT { wVk = 0x11, dwFlags = 0, dwExtraInfo = IntPtr.Zero };
    i[1].U.ki = new KEYBDINPUT { wVk = 0x43, dwFlags = 0, dwExtraInfo = IntPtr.Zero };
    i[2].U.ki = new KEYBDINPUT { wVk = 0x43, dwFlags = KEYUP, dwExtraInfo = IntPtr.Zero };
    i[3].U.ki = new KEYBDINPUT { wVk = 0x11, dwFlags = KEYUP, dwExtraInfo = IntPtr.Zero };
    SendInput(4, i, Marshal.SizeOf(typeof(INPUT)));
  }
  public static RECT FindVisibleRect(uint target) {
    RECT res = new RECT(); res.L = -1;
    EnumWindows((h,l) => {
      uint pid; GetWindowThreadProcessId(h, out pid);
      if (pid == target && IsWindowVisible(h)) { GetWindowRect(h, out res); return false; }
      return true;
    }, IntPtr.Zero);
    return res;
  }
  public static int CountVisible(uint target) {
    int n = 0;
    EnumWindows((h,l) => {
      uint pid; GetWindowThreadProcessId(h, out pid);
      if (pid == target && IsWindowVisible(h)) n++;
      return true;
    }, IntPtr.Zero);
    return n;
  }
}
'@

[D]::SetProcessDpiAwarenessContext([IntPtr](-4)) | Out-Null
$screenW = [D]::GetSystemMetrics(0)
$screenH = [D]::GetSystemMetrics(1)

$AssetsDir = Join-Path $ProjectDir 'docs\images'
$Exe       = Join-Path $env:LOCALAPPDATA 'Programs\CopyPush\CopyPush.exe'
$SettingsKey = 'HKCU:\Software\CopyPush'
New-Item -ItemType Directory -Force -Path $AssetsDir | Out-Null

if (-not (Test-Path $Exe)) { Write-Host "CopyPush не установлен: $Exe" -ForegroundColor Red; exit 1 }

# сохраняем настройки и поднимаем окно-приёмник фокуса
$origStyle = (Get-ItemProperty $SettingsKey -ErrorAction SilentlyContinue).Style

$form = New-Object System.Windows.Forms.Form
$form.Text = 'CopyToast screenshots'
$form.FormBorderStyle = 'None'
$form.TopMost = $true
$form.StartPosition = 'Manual'
$form.Location = New-Object System.Drawing.Point(0, 0)
$form.Width = 900
$form.Height = 560
$form.BackColor = [System.Drawing.Color]::FromArgb(238, 240, 244)
$lbl = New-Object System.Windows.Forms.Label
$lbl.Text = 'Ctrl+C'
$lbl.Font = New-Object System.Drawing.Font('Segoe UI', 42)
$lbl.ForeColor = [System.Drawing.Color]::FromArgb(120, 126, 140)
$lbl.AutoSize = $true
$lbl.Location = New-Object System.Drawing.Point(60, 60)
$form.Controls.Add($lbl)
$form.Show()
$form.Refresh()
[D]::SetForegroundWindow($form.Handle) | Out-Null
Start-Sleep -Milliseconds 600

function Restart-App {
    Get-Process -Name CopyPush -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 1
    Start-Process -FilePath $Exe
    Start-Sleep -Seconds 4
}

function Capture-Pill([string]$path, [int]$style, [switch]$Hero) {
    Set-ItemProperty $SettingsKey -Name 'Style' -Value $style -Type DWord
    Restart-App

    for ($i = 0; $i -lt 50; $i++) { if ([D]::CountVisible((Get-Process -Name CopyPush).Id) -eq 0) { break }; Start-Sleep -Milliseconds 100 }

    [D]::SetCursorPos(430, 330) | Out-Null
    Start-Sleep -Milliseconds 400
    [System.Windows.Forms.Clipboard]::Clear()
    Start-Sleep -Milliseconds 250
    [D]::SendCtrlC()
    Start-Sleep -Milliseconds 90
    [System.Windows.Forms.Clipboard]::SetText('screenshot')

    $r = $null
    for ($i = 0; $i -lt 200; $i++) { $r = [D]::FindVisibleRect((Get-Process -Name CopyPush).Id); if ($r.L -ge 0) { break }; Start-Sleep -Milliseconds 5 }
    Start-Sleep -Milliseconds 400

    $bmp = New-Object System.Drawing.Bitmap($screenW, $screenH)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen((New-Object System.Drawing.Point(0, 0)), [System.Drawing.Point]::Empty, (New-Object System.Drawing.Size($screenW, $screenH)))
    $g.Dispose()

    if ($Hero) {
        $rect = New-Object System.Drawing.Rectangle
        $rect.X = 0; $rect.Y = 0; $rect.Width = 900; $rect.Height = 560
    } else {
        $x = [Math]::Max(0, $r.L - 30)
        $y = [Math]::Max(0, $r.B - 35 - 30)
        $rect = New-Object System.Drawing.Rectangle
        $rect.X = $x; $rect.Y = $y
        $rect.Width = ($r.R - $r.L) + 60
        $rect.Height = 95
    }

    $crop = $bmp.Clone($rect, $bmp.PixelFormat)
    $crop.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $crop.Dispose()
    $bmp.Dispose()
    [System.Windows.Forms.Clipboard]::Clear()
    Write-Host ("  {0,-28} {1}" -f (Split-Path $path -Leaf), $path)
}

Write-Host 'Снимаем скриншоты...' -ForegroundColor Cyan
Capture-Pill (Join-Path $AssetsDir 'style-system.png')   0
Capture-Pill (Join-Path $AssetsDir 'style-accent.png')   1
Capture-Pill (Join-Path $AssetsDir 'style-contrast.png') 2
Capture-Pill (Join-Path $AssetsDir 'hero.png')           0 -Hero

# возвращаем исходные настройки
Set-ItemProperty $SettingsKey -Name 'Style' -Value $origStyle -Type DWord
Restart-App

$form.Close()
$form.Dispose()

Write-Host ''
Get-ChildItem $AssetsDir | ForEach-Object { "  {0,-24} {1,7:N1} КБ" -f $_.Name, ($_.Length / 1KB) }
