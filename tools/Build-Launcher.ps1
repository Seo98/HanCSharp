$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
& dotnet build (Join-Path $PSScriptRoot 'Launcher\HanCSharp.Launcher.csproj') -c Release -o (Join-Path $root 'launcher') --nologo
if ($LASTEXITCODE -ne 0) { throw '시작 도구 빌드 실패' }
