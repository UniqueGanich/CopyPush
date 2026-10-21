[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$code = @'
using System;
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
Add-Type -TypeDefinition $code
[D]::SetProcessDpiAwarenessContext([IntPtr](-4)) | Out-Null
$w = [D]::GetSystemMetrics(0); $h = [D]::GetSystemMetrics(1)
$assets = 'D:\Projects\CopyPush\assets'
New-Item -ItemType Directory -Force -Path $assets | Out-Null
$ct = Get-Process -Name CopyPush -ErrorAction SilentlyContinue | Select-Object -First 1
if(-not $ct){ "FAIL: CopyPush не запущен"; exit 1 }

# Нейтральный светлый фон для скриншота
$bg = New-Object System.Windows.Forms.Form
$bg.Text = "bg"; $bg.FormBorderStyle = "None"; $bg.TopMost = $true
$bg.StartPosition = "Manual"; $bg.Location = New-Object System.Drawing.Point(0,0)
$bg.Width = 900; $bg.Height = 560
$bg.BackColor = [System.Drawing.Color]::FromArgb(238, 240, 244)
$lbl = New-Object System.Windows.Forms.Label
$lbl.Text = "Ctrl+C"
$lbl.Font = New-Object System.Drawing.Font("Segoe UI", 42)
$lbl.ForeColor = [System.Drawing.Color]::FromArgb(120, 126, 140)
$lbl.AutoSize = $true
$lbl.Location = New-Object System.Drawing.Point(60, 60)
$bg.Controls.Add($lbl)
$bg.Show(); $bg.Refresh()
[D]::SetForegroundWindow($bg.Handle) | Out-Null
Start-Sleep -Milliseconds 700

$cxp = 430; $cyp = 330
[D]::SetCursorPos($cxp, $cyp) | Out-Null
Start-Sleep -Milliseconds 500
[System.Windows.Forms.Clipboard]::Clear()
Start-Sleep -Milliseconds 250
[D]::SendCtrlC()
Start-Sleep -Milliseconds 90
[System.Windows.Forms.Clipboard]::SetText("Привет, мир!")

$r = $null
for($i=0;$i -lt 200;$i++){ $r=[D]::FindVisibleRect([uint32]$ct.Id); if($r.L -ge 0){break}; Start-Sleep -Milliseconds 5 }
Start-Sleep -Milliseconds 380

$bmp = New-Object System.Drawing.Bitmap($w,$h)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen((New-Object System.Drawing.Point(0,0)), [System.Drawing.Point]::Empty, (New-Object System.Drawing.Size($w,$h)))
$g.Dispose()

$rx=[int]$r.L; $ry=[int]$r.T; $rw=[int]$r.R-$rx; $rh=[int]$r.B-$ry
# весь фон целиком (900x560) - чистый hero-кадр
$rect=New-Object System.Drawing.Rectangle
$rect.X=0; $rect.Y=0; $rect.Width=900; $rect.Height=560
$crop=$bmp.Clone($rect,$bmp.PixelFormat)
$crop.Save((Join-Path $assets 'screenshot.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$crop.Dispose()

# крупный план
$x2=[Math]::Max(0,$rx-40); $y2=[Math]::Max(0,$ry+$rh-38-40); $cw2=$rw+80; $ch2=118
$rect2=New-Object System.Drawing.Rectangle
$rect2.X=$x2; $rect2.Y=$y2; $rect2.Width=$cw2; $rect2.Height=$ch2
$crop2=$bmp.Clone($rect2,$bmp.PixelFormat)
$crop2.Save((Join-Path $assets 'toast.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$crop2.Dispose(); $bmp.Dispose()

"Готово:"
Get-ChildItem $assets | Select-Object @{n='KB';e={[math]::Round($_.Length/1KB,1)}}, Name | Format-Table -AutoSize
$bg.Close(); $bg.Dispose()
[System.Windows.Forms.Clipboard]::Clear()
