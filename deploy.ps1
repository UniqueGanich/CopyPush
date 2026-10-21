# Сборка и развёртывание CopyPush
# Использование:  powershell -ExecutionPolicy Bypass -File D:\Projects\CopyPush\deploy.ps1

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$ProjectDir = $PSScriptRoot
$PublishDir = Join-Path $ProjectDir 'publish'
$InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\CopyPush'
$UninstallKey = 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CopyPush'

Write-Host "=== 1. Сборка ===" -ForegroundColor Cyan
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:Path = [Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')

dotnet publish (Join-Path $ProjectDir 'CopyPush.csproj') `
    -c Release -r win-x64 --self-contained false -o $PublishDir -v quiet
if ($LASTEXITCODE -ne 0) { throw "Сборка не удалась" }

Write-Host "=== 2. Остановка старой копии ===" -ForegroundColor Cyan
Get-Process -Name CopyPush -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

Write-Host "=== 3. Развёртывание в $InstallDir ===" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
Copy-Item (Join-Path $PublishDir 'CopyPush.exe') -Destination $InstallDir -Force

Write-Host "=== 4. Скрипт удаления ===" -ForegroundColor Cyan
$uninstallPath = Join-Path $InstallDir 'Uninstall.cmd'
Copy-Item (Join-Path $ProjectDir 'tools\Uninstall.cmd') -Destination $uninstallPath -Force

Write-Host "=== 5. Запись в «Программы и компоненты» ===" -ForegroundColor Cyan
New-Item -Path $UninstallKey -Force | Out-Null
Set-ItemProperty $UninstallKey -Name 'DisplayName'     -Value 'CopyPush — уведомление о копировании'
Set-ItemProperty $UninstallKey -Name 'DisplayVersion'  -Value '1.0.0'
Set-ItemProperty $UninstallKey -Name 'Publisher'       -Value 'CopyPush'
Set-ItemProperty $UninstallKey -Name 'DisplayIcon'     -Value "$InstallDir\CopyPush.exe"
Set-ItemProperty $UninstallKey -Name 'InstallLocation' -Value $InstallDir
Set-ItemProperty $UninstallKey -Name 'UninstallString' -Value "`"$uninstallPath`""
Set-ItemProperty $UninstallKey -Name 'NoModify'        -Value 1 -Type DWord
Set-ItemProperty $UninstallKey -Name 'NoRepair'        -Value 1 -Type DWord
Set-ItemProperty $UninstallKey -Name 'EstimatedSize'   -Value 25000 -Type DWord

Write-Host "=== 6. Запуск ===" -ForegroundColor Cyan
Start-Process -FilePath (Join-Path $InstallDir 'CopyPush.exe')
Start-Sleep -Seconds 4

$proc = Get-Process -Name CopyPush -ErrorAction SilentlyContinue
if ($proc) {
    Write-Host "CopyPush запущен, PID=$($proc.Id), память=$([math]::Round($proc.WorkingSet64/1MB,1)) МБ" -ForegroundColor Green
} else {
    Write-Host "ОШИБКА: процесс не запустился" -ForegroundColor Red
}

Write-Host ""
Write-Host "Журнал: $env:LOCALAPPDATA\CopyPush\CopyPush.log" -ForegroundColor DarkGray
Get-Content "$env:LOCALAPPDATA\CopyPush\CopyPush.log" -Tail 6 -ErrorAction SilentlyContinue
