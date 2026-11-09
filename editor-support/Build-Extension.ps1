param([string]$VisualStudioPath)
$ErrorActionPreference = 'Stop'
if (-not $VisualStudioPath) {
    $vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $VisualStudioPath = (& $vswherePath -latest -products '*' -version '[17.0,18.0)' -property installationPath | Select-Object -First 1)
}
if (-not $VisualStudioPath) { throw '빌드에 사용할 Visual Studio 2022 경로를 지정하세요.' }
$project = Join-Path $PSScriptRoot 'HanCSharp.VisualStudio\HanCSharp.VisualStudio.csproj'
& dotnet build $project -c Release -m:1 --nologo "-p:VisualStudioInstallPath=$VisualStudioPath"
if ($LASTEXITCODE -ne 0) { throw 'Visual Studio 확장 빌드 실패' }

$destination = Join-Path $PSScriptRoot '..\visualstudio\HanCSharp-0.7.vsix'
$destination = [System.IO.Path]::GetFullPath($destination)
$files = @{
    'extension.vsixmanifest' = (Join-Path $PSScriptRoot 'HanCSharp.VisualStudio\extension.vsixmanifest')
    'HanCSharp.VisualStudio.dll' = (Join-Path $PSScriptRoot 'HanCSharp.VisualStudio\bin\Release\net472\HanCSharp.VisualStudio.dll')
}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream = [System.IO.File]::Open($destination, [System.IO.FileMode]::Create)
$zip = [System.IO.Compression.ZipArchive]::new($stream, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($item in $files.GetEnumerator()) {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $item.Value, $item.Key) | Out-Null
    }
    $entry = $zip.CreateEntry('[Content_Types].xml')
    $writer = [System.IO.StreamWriter]::new($entry.Open(), [System.Text.UTF8Encoding]::new($false))
    try {
        $writer.Write('<?xml version="1.0" encoding="utf-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="vsixmanifest" ContentType="text/xml"/><Default Extension="dll" ContentType="application/octet-stream"/></Types>')
    } finally { $writer.Dispose() }
} finally { $zip.Dispose(); $stream.Dispose() }
Write-Output "VSIX 생성: $destination"
