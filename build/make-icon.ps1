# Генератор иконки CopyPush (.ico) + .png для README.
# Запуск:  powershell -ExecutionPolicy Bypass -File build\make-icon.ps1
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$Root      = Split-Path -Parent $PSScriptRoot
$AssetsDir = Join-Path $Root 'docs\images'
$ResDir    = Join-Path $Root 'src\CopyPush\Resources'
New-Item -ItemType Directory -Force -Path $AssetsDir, $ResDir | Out-Null

# Акцент Windows 11 по умолчанию (используется, если система другая - см. README)
$Accent = [System.Drawing.Color]::FromArgb(255, 0x00, 0x78, 0xD4)

function New-IconBitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = [math]::Max(1, [int]($size * 0.06))
    $d = $size - 2 * $pad

    # Круг акцентным цветом
    $brush = New-Object System.Drawing.SolidBrush($Accent)
    $g.FillEllipse($brush, $pad, $pad, $d, $d)

    # Белая галочка
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, [math]::Max(1.4, $size * 0.11))
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $p1 = New-Object System.Drawing.PointF(($pad + $d * 0.26), ($pad + $d * 0.52))
    $p2 = New-Object System.Drawing.PointF(($pad + $d * 0.44), ($pad + $d * 0.70))
    $p3 = New-Object System.Drawing.PointF(($pad + $d * 0.76), ($pad + $d * 0.32))
    $g.DrawLines($pen, [System.Drawing.PointF[]]@($p1, $p2, $p3))
    $pen.Dispose(); $brush.Dispose(); $g.Dispose()
    return $bmp
}

function ConvertTo-PngBytes([System.Drawing.Bitmap]$bmp) {
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bytes = $ms.ToArray()
    $ms.Dispose()
    # запятая перед массивом - чтобы PowerShell не развернул byte[] в поток
    return ,$bytes
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$entries = @()

foreach($s in $sizes){
    $bmp = New-IconBitmap $s
    $png = ConvertTo-PngBytes $bmp
    $entries += [pscustomobject]@{ Size = $s; Data = $png }
    if($s -eq 256){
        $bmp.Save((Join-Path $AssetsDir 'icon-256.png'), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    $bmp.Dispose()
}

# ---- Сборка ICO (заголовок + каталог + PNG-данные) ----
$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($ms)
$bw.Write([UInt16]0)          # reserved
$bw.Write([UInt16]1)          # type = icon
$bw.Write([UInt16]$entries.Count)

$offset = 6 + 16 * $entries.Count
foreach($e in $entries){
    $w = if($e.Size -ge 256){ 0 } else { $e.Size }
    $bw.Write([Byte]$w)
    $bw.Write([Byte]$w)
    $bw.Write([Byte]0)         # палитра
    $bw.Write([Byte]0)         # reserved
    $bw.Write([UInt16]1)       # planes
    $bw.Write([UInt16]32)      # bpp
    $bw.Write([UInt32]$e.Data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $e.Data.Length
}
foreach($e in $entries){ $bw.Write([byte[]]$e.Data) }
$bw.Flush()

$icoPath = Join-Path $ResDir 'CopyPush.ico'
[System.IO.File]::WriteAllBytes($icoPath, $ms.ToArray())
$ms.Dispose()

"Иконка: $icoPath ($([math]::Round((Get-Item $icoPath).Length/1KB,1)) КБ, размеры: $($sizes -join ', '))"

# Проверка: .ico должен читаться
$ico = New-Object System.Drawing.Icon($icoPath)
"Проверка чтения: OK, {0}x{1}" -f $ico.Width, $ico.Height
$ico.Dispose()
