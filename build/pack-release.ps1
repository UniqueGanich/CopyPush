# Упаковка релизных артефактов. Используется и локально, и в CI (.github/workflows/release.yml).
#   powershell -ExecutionPolicy Bypass -File build\pack-release.ps1 -Version 1.0.0
#
# Имена архивов: CopyPush-<ArchiveName>.zip (всё внутри) и
# CopyPush-<ArchiveName>-small.zip (нужен .NET 8 Runtime).
# ArchiveName по умолчанию равен версии; в CI для тега v1.0.0 туда
# подставляется короткая версия 1.0, чтобы ссылки в README были короткими.
param(
    [string]$Version = '1.0.0',
    [string]$ArchiveName = '',
    [string]$ProjectDir = (Split-Path -Parent $PSScriptRoot)
)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ArchiveName)) { $ArchiveName = $Version }

$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:Path = [Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')

$Dist   = Join-Path $ProjectDir 'dist'
$Out    = Join-Path $ProjectDir 'release'
$Proj   = Join-Path $ProjectDir 'src\CopyPush\CopyPush.csproj'

# ---------------------------------------------------------------------------
# Markdown -> обычный текст.
# В архиве README кладёмся без разметки: блокнот Windows её не рисует.
# В репозитории README остаётся в markdown.
#
# Разметка снимается по абзацам, а не по строкам: `**` и обратные кавычки
# могут быть разорваны переносом строки, и по одной строке их не убрать.
# ---------------------------------------------------------------------------

function Convert-InlineToText([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return '' }
    $t = $s

    # Картинка: ![alt](url) -> [alt]
    $t = [regex]::Replace($t, '!\[([^\]]*)\]\(([^)]*)\)', {
        param($m)
        if ([string]::IsNullOrWhiteSpace($m.Groups[1].Value)) { '' } else { '[' + $m.Groups[1].Value + ']' }
    })

    # Ссылка: [текст](url) -> текст (url)
    $t = [regex]::Replace($t, '\[([^\]]*)\]\(([^)]*)\)', {
        param($m)
        $text = $m.Groups[1].Value
        $url  = $m.Groups[2].Value
        if ([string]::IsNullOrWhiteSpace($text)) { $url } else { $text + ' (' + $url + ')' }
    })

    # Выделения. (?s) - чтобы пара `**` или `` ` `` , разорванная переносом
    # строки, тоже снималась.
    $t = $t -replace '(?s)\*\*(.+?)\*\*', '$1'
    $t = $t -replace '(?s)__(.+?)__', '$1'
    $t = $t -replace '(?s)~~(.+?)~~', '$1'
    $t = $t -replace '\*([^*\n]+)\*', '$1'
    $t = $t -replace '(?s)`([^`]+)`', '$1'

    # HTML-теги и сущности
    $t = [regex]::Replace($t, '<[^>]+>', '')
    $t = $t -replace '&nbsp;', ' '
    $t = $t -replace '&lt;', '<'
    $t = $t -replace '&gt;', '>'
    $t = $t -replace '&quot;', '"'
    $t = $t -replace '&amp;', '&'

    return $t.Trim()
}

function Write-TableRows($rows, $out) {
    foreach ($row in $rows) {
        $cells = @($row)
        if (-not ($cells | Where-Object { $_ -ne '' })) { continue }
        $out.Add(($cells -join '  |  '))
    }
}

# Печатает накопленный блок обычного текста, сняв разметку.
function Flush-Block($block, $out) {
    if ($block.Count -eq 0) { return }
    $joined = $block -join "`n"
    $block.Clear()
    $text = Convert-InlineToText $joined
    if ($text -eq '') { return }
    foreach ($l in ($text -split "`n")) { $out.Add($l) }
}

function Convert-MarkdownToText([string]$md) {
    $out = New-Object System.Collections.Generic.List[string]
    $lines = $md -split "`r?`n"
    $inCode = $false
    $table = New-Object System.Collections.Generic.List[object]
    $block = New-Object System.Collections.Generic.List[string]

    foreach ($raw in $lines) {
        $line = $raw

        # Ограждение блока кода
        if ($line -match '^\s*```') { Flush-Block $block $out; $inCode = -not $inCode; continue }
        if ($inCode) { $out.Add($line); continue }

        # Таблица
        if ($line -match '^\s*\|') {
            Flush-Block $block $out
            if ($line -notmatch '^\s*\|[\s:\-\|]+\|\s*$') {
                $cells = @()
                foreach ($c in ($line.Trim().Trim('|') -split '\|')) { $cells += (Convert-InlineToText $c) }
                $table.Add($cells)
            }
            continue
        }
        if ($table.Count -gt 0) {
            if ($out.Count -gt 0 -and $out[$out.Count - 1] -ne '') { $out.Add('') }
            Write-TableRows $table $out
            $out.Add('')
            $table.Clear()
        }

        # Горизонтальная линейка
        if ($line -match '^\s*(-{3,}|\*{3,}|_{3,})\s*$') { Flush-Block $block $out; continue }

        # Строка из одного HTML-тега
        if ($line -match '^\s*</?[a-zA-Z][^>]*>\s*$') { Flush-Block $block $out; continue }

        # Строка только из картинки или бейджа
        if ($line -match '^\s*!\[[^\]]*\]\([^)]*\)\s*$') { Flush-Block $block $out; continue }
        if ($line -match '^\s*\[!\[[^\]]*\]\([^)]*\)\]\([^)]*\)\s*$') { Flush-Block $block $out; continue }

        # Пустая строка - конец абзаца
        if ($line.Trim() -eq '') { Flush-Block $block $out; continue }

        # Заголовок
        if ($line -match '^\s*#{1,6}\s+(.*)$') {
            Flush-Block $block $out
            $out.Add('')
            $out.Add((Convert-InlineToText $Matches[1]))
            $out.Add('')
            continue
        }

        # Цитата
        if ($line -match '^\s*>\s?(.*)$') {
            Flush-Block $block $out
            $out.Add((Convert-InlineToText $Matches[1]))
            continue
        }

        # Обычный текст - часть абзаца
        $block.Add($line)
    }

    Flush-Block $block $out

    if ($table.Count -gt 0) {
        Write-TableRows $table $out
        $table.Clear()
    }

    # Схлопываем пустые строки подряд и убираем хвостовые
    $res = New-Object System.Collections.Generic.List[string]
    $wasEmpty = $true
    foreach ($l in $out) {
        if ($l -eq '') {
            if ($wasEmpty) { continue }
            $wasEmpty = $true
        } else { $wasEmpty = $false }
        $res.Add($l)
    }
    while ($res.Count -gt 0 -and $res[$res.Count - 1] -eq '') { $res.RemoveAt($res.Count - 1) }

    return ($res -join "`r`n")
}

# ---------------------------------------------------------------------------
# Сборка
# ---------------------------------------------------------------------------

Write-Host "=== Сборка self-contained ===" -ForegroundColor Cyan
Remove-Item (Join-Path $Dist 'self-contained') -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish $Proj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true -p:DebugType=none `
    -o (Join-Path $Dist 'self-contained') -v quiet

Write-Host "=== Сборка framework-dependent ===" -ForegroundColor Cyan
Remove-Item (Join-Path $Dist 'framework-dependent') -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish $Proj -c Release -r win-x64 --self-contained false `
    -p:PublishSingleFile=true -p:DebugType=none `
    -o (Join-Path $Dist 'framework-dependent') -v quiet

Write-Host "=== Упаковка ===" -ForegroundColor Cyan
Remove-Item $Out -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $Out | Out-Null

function Pack([string]$srcDir, [string]$stageName, [string]$zipName) {
    # Внутри архива файлы лежат в одноимённой папке: CopyPush-1.1.0/CopyPush.exe
    $stage = Join-Path $Out $zipName
    New-Item -ItemType Directory -Force -Path $stage | Out-Null
    Copy-Item (Join-Path $srcDir 'CopyPush.exe') $stage
    Copy-Item (Join-Path $ProjectDir 'build\Uninstall.cmd') $stage
    Copy-Item (Join-Path $ProjectDir 'LICENSE') $stage

    # README в двух языках, как в репозитории: README.md (английский) и
    # README.ru.md (русский). В архиве разметка убирается - блокнот её не рисует.
    foreach ($pair in @(
        @{ src = 'README.md';    dst = 'README.md' },
        @{ src = 'README.ru.md'; dst = 'README.ru.md' }
    )) {
        $md = [System.IO.File]::ReadAllText((Join-Path $ProjectDir $pair.src), [System.Text.Encoding]::UTF8)
        $text = Convert-MarkdownToText $md
        [System.IO.File]::WriteAllText((Join-Path $stage $pair.dst), $text + "`r`n", (New-Object System.Text.UTF8Encoding($true)))
    }

    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath (Join-Path $Out "$zipName.zip") -CompressionLevel Optimal
    Remove-Item $stage -Recurse -Force
}

# Полный архив: .NET внутри, работает сразу.
Pack (Join-Path $Dist 'self-contained')      'staging-self-contained'      "CopyPush-$ArchiveName"
# Облегчённый архив: нужен .NET 8 Desktop Runtime.
Pack (Join-Path $Dist 'framework-dependent') 'staging-framework-dependent' "CopyPush-$ArchiveName-small"

Get-ChildItem $Out -Filter *.zip | ForEach-Object {
    "{0,-42} {1,7:N1} МБ" -f $_.Name, ($_.Length / 1MB)
}
