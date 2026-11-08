$ErrorActionPreference = 'Stop'
$solutionPath = Join-Path $PSScriptRoot 'HanCSharp.sln'
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswherePath)) { throw 'Visual Studio Installer를 찾지 못했습니다.' }
$installationPath = (& $vswherePath -latest -products '*' -version '[17.0,18.0)' -property installationPath | Select-Object -First 1)
if (-not $installationPath) {
    $installationPath = (& $vswherePath -latest -products '*' -property installationPath | Select-Object -First 1)
}
if (-not $installationPath) { throw '설치된 Visual Studio를 찾지 못했습니다.' }
$visualStudioPath = Join-Path $installationPath 'Common7\IDE\devenv.exe'
# 사용자가 직접 작업할 편집기 창을 연다.
Start-Process -FilePath $visualStudioPath -ArgumentList ('"' + $solutionPath + '"')
