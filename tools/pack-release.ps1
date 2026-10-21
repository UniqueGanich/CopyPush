# Упаковка релизных артефактов. Используется и локально, и в CI (.github/workflows/release.yml).
#   powershell -ExecutionPolicy Bypass -File tools\pack-release.ps1 -Version 1.0.0
param(
    [string]$Version = '1.0.0',
    [string]$ProjectDir = (Split-Path -Parent $PSScriptRoot)
)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:Path = [Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')

$Dist   = Join-Path $ProjectDir 'dist'
$Out    = Join-Path $ProjectDir 'release'
$Proj   = Join-Path $ProjectDir 'CopyPush.csproj'

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

function Pack([string]$srcDir, [string]$name) {
    $stage = Join-Path $Out $name
    New-Item -ItemType Directory -Force -Path $stage | Out-Null
    Copy-Item (Join-Path $srcDir 'CopyPush.exe') $stage
    Copy-Item (Join-Path $ProjectDir 'tools\Uninstall.cmd') $stage
    Copy-Item (Join-Path $ProjectDir 'README.md') $stage
    Copy-Item (Join-Path $ProjectDir 'LICENSE') $stage
    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath (Join-Path $Out "$name.zip") -CompressionLevel Optimal
    Remove-Item $stage -Recurse -Force
}

Pack (Join-Path $Dist 'self-contained')     "CopyPush-$Version-win-x64"
Pack (Join-Path $Dist 'framework-dependent') "CopyPush-$Version-win-x64-small"

Get-ChildItem $Out -Filter *.zip | ForEach-Object {
    "{0,-42} {1,7:N1} МБ" -f $_.Name, ($_.Length / 1MB)
}
