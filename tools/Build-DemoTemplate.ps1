$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'samples\UnityMovementDemo'
$manifest = Get-Content -LiteralPath (Join-Path $source 'Packages\manifest.json') -Raw | ConvertFrom-Json
if ($manifest.dependencies.'com.unity.pipeline') { throw '일반 배포 예제에서 검증용 Pipeline 패키지를 먼저 제거하세요.' }
$destination = Join-Path $root 'templates\UnityMovementDemo.zip'
New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream = [IO.File]::Open($destination, [IO.FileMode]::Create)
$zip = [IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($folder in @('Assets','Packages','ProjectSettings')) {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $source $folder) -Recurse -File) {
            $name = $file.FullName.Substring($source.Length).TrimStart('\','/').Replace('\','/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$file.FullName,$name) | Out-Null
        }
    }
} finally { $zip.Dispose(); $stream.Dispose() }
Write-Output "예제 템플릿: $destination"
