param([switch]$SmokeTest)
$ErrorActionPreference = 'Stop'
$launcher = Join-Path (Split-Path -Parent $PSScriptRoot) 'launcher\HanCSharp.Start.exe'
if (-not (Test-Path -LiteralPath $launcher)) { throw 'Windows 실행 ZIP을 모두 압축 해제하세요. 소스 개발은 tools/Build-Launcher.ps1을 먼저 실행하세요.' }
if ($SmokeTest) { . (Join-Path $PSScriptRoot 'Invoke-Launcher.ps1'); $result = Invoke-HanLauncher -Executable $launcher -Arguments @('--smoke-test'); Write-Output $result.Output; if($result.Error){Write-Output $result.Error}; exit $result.ExitCode }
Start-Process -FilePath $launcher
