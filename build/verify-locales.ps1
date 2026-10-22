# Проверка целостности таблицы локализации.
# Все языки должны содержать одинаковый набор ключей.
# Запуск:  powershell -ExecutionPolicy Bypass -File build\verify-locales.ps1
param(
    [string]$ProjectDir = (Split-Path -Parent $PSScriptRoot)
)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$locFile = Join-Path $ProjectDir 'src\CopyPush\Services\Localization.cs'
$text = [System.IO.File]::ReadAllText($locFile, [System.Text.Encoding]::UTF8)

$langs = @{}
$current = $null
foreach ($line in ($text -split "`r?`n")) {
    if ($line -match '^\s*\["([a-z]{2}(?:-[A-Za-z]{2,4})?)"\]\s*=\s*new\(\)') { $current = $Matches[1]; $langs[$current] = @{} ; continue }
    if ($line -match '^\s*\["([a-z]{2}(?:-[A-Za-z]{2,4})?)"\]\s*=\s*new$') { $current = $Matches[1]; $langs[$current] = @{} ; continue }
    if ($line -match '^\s*\["([^"]+)"\]\s*=\s*"((?:[^"\\]|\\.)*)"') {
        if ($current) { $langs[$current][$Matches[1]] = $Matches[2] }
    }
}

if ($langs.Count -eq 0) {
    Write-Host "НЕ УДАЛОСЬ РАЗОБРАТЬ Loc.cs" -ForegroundColor Red
    exit 1
}

$reference = $langs.Keys | Select-Object -First 1
$refKeys = $langs[$reference].Keys | Sort-Object

Write-Host "Языков: $($langs.Count)   ключей в эталоне ($reference): $($refKeys.Count)" -ForegroundColor Cyan
Write-Host ""

$problems = 0
foreach ($lang in ($langs.Keys | Sort-Object)) {
    $keys = $langs[$lang].Keys
    $missing = $refKeys | Where-Object { $_ -notin $keys }
    $extra = $keys | Where-Object { $_ -notin $refKeys }
    $empty = $keys | Where-Object { [string]::IsNullOrWhiteSpace($langs[$lang][$_]) }

    $status = "OK"
    if ($missing -or $extra -or $empty) { $status = "ПРОБЛЕМА"; $problems++ }

    "{0,-4} {1,-8} ключей: {2,3}" -f $lang, $status, $keys.Count
    foreach ($m in $missing) { "     отсутствует ключ: $m" }
    foreach ($e in $extra)   { "     лишний ключ:      $e" }
    foreach ($m in $empty)  { "     пустое значение:   $m" }
}

Write-Host ""
if ($problems -gt 0) {
    Write-Host "ПРОБЛЕМЫ: $problems" -ForegroundColor Red
    exit 1
}

Write-Host "Таблица локализации консистентна." -ForegroundColor Green
exit 0
