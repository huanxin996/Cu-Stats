param(
    [ValidateSet('Release','Debug')] [string]$Configuration = 'Release',
    [string]$GameRoot = 'E:\SteamLibrary\steamapps\common\Casualties Unknown Demo'
)
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dll = Join-Path $Root "bin\$Configuration\CuStats.dll"
if (-not (Test-Path $dll)) { throw "未找到 $dll，先 dotnet build -c $Configuration" }
$dst = Join-Path $GameRoot 'BepInEx\plugins\CuStats'
New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item -LiteralPath $dll -Destination (Join-Path $dst 'CuStats.dll') -Force
"deployed: " + (Get-Item (Join-Path $dst 'CuStats.dll')).Length + " bytes -> $dst"
