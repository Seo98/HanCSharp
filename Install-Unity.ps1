param([Parameter(Mandatory=$true)][string]$ProjectPath,[switch]$IncludeExamples)
$ErrorActionPreference = 'Stop'
$launcher = Join-Path $PSScriptRoot 'launcher\HanCSharp.Start.exe'
if (-not (Test-Path -LiteralPath $launcher)) { throw 'tools/Build-Launcher.ps1을 먼저 실행하거나 Windows 실행 ZIP을 사용하세요.' }
$arguments = @('--install-unity', (Resolve-Path -LiteralPath $ProjectPath).Path)
if ($IncludeExamples) { $arguments += '--include-examples' }
. (Join-Path $PSScriptRoot 'tools\Invoke-Launcher.ps1')
$result = Invoke-HanLauncher -Executable $launcher -Arguments $arguments
Write-Output $result.Output
if ($result.Error) { Write-Output $result.Error }
if ($result.ExitCode -ne 0) { throw '한씨 설치가 완료되지 않았습니다. 위 오류의 경로를 확인하세요. 사용자 파일은 덮어쓰지 않습니다.' }
