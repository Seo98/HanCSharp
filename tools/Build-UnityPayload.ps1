param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'artifacts\unity' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $output -Force | Out-Null
$stage = Join-Path $root ('artifacts\unity-stage-' + [Guid]::NewGuid().ToString('N'))
$compiler = Join-Path $stage 'compiler'
& dotnet publish (Join-Path $root 'src\HanCSharp.Cli') -c Release --self-contained false -p:UseAppHost=false -p:RollForward=Major -p:DebugType=None -p:DebugSymbols=false -o $compiler
if ($LASTEXITCODE -ne 0) { throw 'Unity용 DLL 변환기 빌드 실패' }
if ((Get-Content (Join-Path $compiler 'HanCSharp.Cli.runtimeconfig.json') -Raw | ConvertFrom-Json).runtimeOptions.rollForward -ne 'Major') { throw '런타임 전환 설정이 없습니다.' }
$payload = Join-Path $stage 'payload'
$entries = [Collections.Generic.List[object]]::new()
function Add-PayloadFile([string]$Source,[string]$Relative,[bool]$Preserve=$false) {
    $target = Join-Path $payload $Relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
    Copy-Item -LiteralPath $Source -Destination $target
    $entries.Add([ordered]@{path=$Relative.Replace('\','/');sha256=(Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant();preserveExisting=$Preserve})
}
function Add-Asset([string]$Source,[string]$Relative,[bool]$Preserve=$false,[string]$MetaSource) {
    Add-PayloadFile $Source $Relative $Preserve
    if (-not $MetaSource -or -not (Test-Path -LiteralPath $MetaSource)) {
        $sha=[Security.Cryptography.SHA256]::Create()
        try { $guid=([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes('HanCSharp:'+$Relative)))).Replace('-','').Substring(0,32).ToLowerInvariant() } finally { $sha.Dispose() }
        $MetaSource=Join-Path $stage ($guid+'.meta')
        [IO.File]::WriteAllText($MetaSource,"fileFormatVersion: 2`nguid: $guid`n",[Text.UTF8Encoding]::new($false))
    }
    Add-PayloadFile $MetaSource ($Relative+'.meta') $true
}
foreach($file in Get-ChildItem -LiteralPath $compiler -Recurse -File) {
    if ($file.Extension -in @('.exe','.pdb')) { continue }
    $relative=$file.FullName.Substring($compiler.Length).TrimStart('\','/')
    Add-PayloadFile $file.FullName ('Tools/HanCSharp/'+$relative.Replace('\','/'))
}
foreach($file in Get-ChildItem (Join-Path $root 'unity\Editor') -File | Where-Object Extension -in @('.cs','.uxml','.uss')) {
    $meta=Join-Path $root ('samples\UnityMovementDemo\Assets\HanCSharp\Editor\'+$file.Name+'.meta')
    Add-Asset $file.FullName ('Assets/HanCSharp/Editor/'+$file.Name) $false $meta
}
foreach($file in Get-ChildItem (Join-Path $root 'samples\UnityScripts') -Filter '*.han' -File) {
    Add-Asset $file.FullName ('Assets/HanCSharp/Examples/'+$file.Name) $true ''
}
foreach($name in @('UNITYPACKAGE-INSTALL.md','GRAMMAR-0.7.md','UNITY-EDITING.md')) {
    Add-Asset (Join-Path $root ('docs\'+$name)) ('Assets/HanCSharp/Documentation/'+$name) $false ''
}
Add-PayloadFile (Join-Path $root 'LICENSE') 'Tools/HanCSharp/licenses/HanCSharp-LICENSE.txt'
Add-PayloadFile (Join-Path $root 'THIRD-PARTY-NOTICES.md') 'Tools/HanCSharp/licenses/THIRD-PARTY-NOTICES.md'
$manifest=[ordered]@{schemaVersion=1;version=([IO.File]::ReadAllText((Join-Path $root 'VERSION'))).Trim();files=$entries.ToArray()}
[IO.File]::WriteAllText((Join-Path $payload 'manifest.json'),($manifest | ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
Copy-Item (Join-Path $root 'unity\Installer\baselines.json') (Join-Path $payload 'baselines.json')
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=Join-Path $output 'HanCSharp-UnityPayload.zip'
if(Test-Path -LiteralPath $zip){ throw "기존 배포는 보존합니다. 새 출력 폴더를 사용하세요: $zip" }
[IO.Compression.ZipFile]::CreateFromDirectory($payload,$zip)
$assets=Join-Path $output 'HanCSharpInstaller'
New-Item -ItemType Directory -Path (Join-Path $assets 'Editor') -Force | Out-Null
Copy-Item (Join-Path $root 'unity\Installer\Editor\*') (Join-Path $assets 'Editor')
Copy-Item -LiteralPath $zip -Destination (Join-Path $assets 'HanCSharpPayload.bytes')
Copy-Item (Join-Path $root 'docs\UNITYPACKAGE-INSTALL.md') (Join-Path $assets 'INSTALL.md')
Copy-Item (Join-Path $root 'VERSION') (Join-Path $assets 'VERSION.txt')
Write-Output "Unity 설치 자산: $assets"
Write-Output "DLL 전용 업데이트: $zip"
