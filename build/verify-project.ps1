# Проверка целостности проекта: namespace'ы, гомоглифы в идентификаторах, локализация.
# Ловит ошибки, которые компилятор не показывает. Например, если namespace написан
# как "CopyToast.Services" вместо "CopyPush.Services": файл компилируется без ошибок,
# но типы из него не видны другим файлам.
# Запуск:  powershell -ExecutionPolicy Bypass -File build\verify-project.ps1
param(
    [string]$ProjectDir = (Split-Path -Parent $PSScriptRoot)
)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$srcDir = Join-Path $ProjectDir 'src\CopyPush'
$rootNs = 'CopyPush'

$script:problems = 0
function Problem([string]$message) {
    Write-Host "  ПРОБЛЕМА: $message" -ForegroundColor Red
    $script:problems++
}

# Кириллица, визуально неотличимая от латиницы - источник невидимых ошибок.
$homoglyphs = @(
    0x0410, 0x0412, 0x0415, 0x041A, 0x041C, 0x041D, 0x041E, 0x0420, 0x0421, 0x0422, 0x0423, 0x0425, 0x0405, 0x0406, 0x0408,
    0x0430, 0x0432, 0x0435, 0x043A, 0x043C, 0x043D, 0x043E, 0x0440, 0x0441, 0x0442, 0x0443, 0x0445, 0x0455, 0x0456, 0x0458
)

# Возвращает только "код" (без комментариев и строковых литералов).
function Get-CodeOnly([string]$text) {
    $sb = New-Object System.Text.StringBuilder
    $i = 0
    $n = $text.Length
    while ($i -lt $n) {
        $c = $text[$i]
        $next = if ($i + 1 -lt $n) { $text[$i + 1] } else { [char]0 }

        if ($c -eq '/' -and $next -eq '/') {          # строковый комментарий
            while ($i -lt $n -and $text[$i] -ne "`n") { $i++ }
            continue
        }
        if ($c -eq '/' -and $next -eq '*') {          # блочный комментарий
            $i += 2
            while ($i -lt $n -and -not ($text[$i] -eq '*' -and $i + 1 -lt $n -and $text[$i + 1] -eq '/')) { $i++ }
            $i += 2
            continue
        }
        if ($c -eq '@' -and $next -eq '"') {          # verbatim-строка
            $i += 2
            while ($i -lt $n) { if ($text[$i] -eq '"') { if ($i + 1 -lt $n -and $text[$i + 1] -eq '"') { $i += 2; continue }; break }; $i++ }
            $i++
            continue
        }
        if ($c -eq '"') {                             # обычная строка
            $i++
            while ($i -lt $n) {
                if ($text[$i] -eq '\\') { $i += 2; continue }
                if ($text[$i] -eq '"') { break }
                $i++
            }
            $i++
            continue
        }
        if ($c -eq "'") {                             # символьный литерал
            $i++
            while ($i -lt $n) {
                if ($text[$i] -eq '\\') { $i += 2; continue }
                if ($text[$i] -eq "'") { break }
                $i++
            }
            $i++
            continue
        }

        [void]$sb.Append($c)
        $i++
    }
    return $sb.ToString()
}

$files = Get-ChildItem $srcDir -Recurse -File -Include *.cs |
         Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' }

Write-Host '=== 1. Пространства имён ===' -ForegroundColor Cyan
$before = $script:problems
foreach ($f in $files) {
    $text = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    $m = [regex]::Match($text, '(?m)^\s*namespace\s+([\w\.]+)')
    if (-not $m.Success) { Problem "$($f.Name): нет объявления namespace"; continue }

    $ns = $m.Groups[1].Value
    if (-not $ns.StartsWith($rootNs)) {
        # Показываем, чем именно отличается: сравниваем по кодовым точкам.
        $codes = ($ns.ToCharArray() | ForEach-Object { 'U+{0:X4}' -f [int]$_ }) -join ' '
        Problem ("{0}: namespace '{1}' не начинается с '{2}'  [{3}]" -f $f.Name, $ns, $rootNs, $codes)
    }
}
if ($script:problems -eq $before) { Write-Host "  все namespace'ы корректны ($($files.Count) файлов)" -ForegroundColor DarkGray }

Write-Host ''
Write-Host '=== 2. Гомоглифы в коде ===' -ForegroundColor Cyan
$before = $script:problems
foreach ($f in $files) {
    $text = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    $code = Get-CodeOnly $text
    $lineNo = 1
    foreach ($ch in $code.ToCharArray()) {
        if ($ch -eq "`n") { $lineNo++; continue }
        if ($homoglyphs -contains [int]$ch) {
            Problem ("{0}: кириллический символ U+{1:X4} в коде (около строки {2})" -f $f.Name, [int]$ch, $lineNo)
        }
    }
}
if ($script:problems -eq $before) { Write-Host "  гомоглифов в коде нет" -ForegroundColor DarkGray }

Write-Host ''
Write-Host '=== 3. Таблица локализации ===' -ForegroundColor Cyan
& (Join-Path $PSScriptRoot 'verify-locales.ps1') -ProjectDir $ProjectDir
if ($LASTEXITCODE -ne 0) { Problem 'таблица локализации неконсистентна' }

Write-Host ''
if ($script:problems -gt 0) {
    Write-Host "ИТОГО ПРОБЛЕМ: $script:problems" -ForegroundColor Red
    exit 1
}

Write-Host 'Все проверки пройдены.' -ForegroundColor Green
exit 0
