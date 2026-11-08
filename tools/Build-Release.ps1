param([switch]$SkipPublish, [string]$OutputPath)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$portable = Join-Path $root 'app-portable'
& (Join-Path $PSScriptRoot 'Build-Launcher.ps1')
& (Join-Path $PSScriptRoot 'Build-DemoTemplate.ps1')
if (-not $SkipPublish) {
    & dotnet publish (Join-Path $root 'src\HanCSharp.Cli') -c Release -r win-x64 --self-contained true -o $portable --source https://api.nuget.org/v3/index.json -p:PublishTrimmed=false
    if ($LASTEXITCODE -ne 0) { throw 'Windows 런타임 포함 배포 빌드 실패' }
}
if (-not (Test-Path -LiteralPath (Join-Path $portable 'coreclr.dll'))) { throw '런타임 포함 app-portable 빌드가 필요합니다.' }
$runtime = (Get-Content -LiteralPath (Join-Path $portable 'HanCSharp.Cli.runtimeconfig.json') -Raw | ConvertFrom-Json).runtimeOptions.includedFrameworks[0].version
$licenses = Join-Path $portable 'licenses'
New-Item -ItemType Directory -Path $licenses -Force | Out-Null
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dotnet/runtime/v$runtime/LICENSE.TXT" -OutFile (Join-Path $licenses 'DOTNET-LICENSE.txt')
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/dotnet/runtime/v$runtime/THIRD-PARTY-NOTICES.TXT" -OutFile (Join-Path $licenses 'DOTNET-THIRD-PARTY-NOTICES.txt')
if (-not (Test-Path -LiteralPath (Join-Path $root 'visualstudio\HanCSharp-0.3.vsix'))) { throw 'Build-Extension.ps1로 VSIX를 먼저 만드세요.' }
$artifacts = Join-Path $root 'artifacts'
$stage = Join-Path $artifacts ('stage-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage -Force | Out-Null
# 소스·문서와 배포에 필요한 파일만 스테이징한다. 기존 파일/캐시를 삭제하지 않는다.
$excluded = @('bin','obj','.vs','.git','app','app-portable','artifacts','validation')
foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File -Force) {
    $relative = $file.FullName.Substring($root.Length).TrimStart('\','/')
    if (@($relative.Split([char[]]'\/') | Where-Object { $_ -in $excluded }).Count -gt 0) { continue }
    if ($relative.StartsWith('samples\UnityMovementDemo\', [StringComparison]::OrdinalIgnoreCase) -and
        $relative -notin @('samples\UnityMovementDemo\README.md','samples\UnityMovementDemo\VALIDATION.md')) { continue }
    $target = Join-Path $stage $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target
}
Copy-Item -LiteralPath $portable -Destination (Join-Path $stage 'app') -Recurse
$zip = if ($OutputPath) { [IO.Path]::GetFullPath($OutputPath) } else { Join-Path $artifacts 'HanCSharp-Windows-x64.zip' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
if (Test-Path -LiteralPath $zip) { throw "출력 ZIP이 이미 있습니다. 기존 결과를 보관하고 다른 출력 경로를 지정하세요: $zip" }
[System.IO.Compression.ZipFile]::CreateFromDirectory($stage, $zip)
Write-Output "배포 ZIP: $zip"
Write-Output "검토용 스테이징: $stage"
