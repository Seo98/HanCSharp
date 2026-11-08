param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Run', 'Check', 'Preview', 'ToKorean')]
    [string]$Action,
    [Parameter(Mandatory = $true)]
    [string]$InputFile
)

$ErrorActionPreference = 'Stop'
$inputPath = (Resolve-Path -LiteralPath $InputFile).Path
$compilerPath = Join-Path $PSScriptRoot '..\app\HanCSharp.Cli.dll'
if ($Action -in @('Run', 'Check', 'Preview') -and [System.IO.Path]::GetExtension($inputPath) -ne '.han') {
    throw '현재 문서로 .han 파일을 선택하세요.'
}
switch ($Action) {
    'Run' { & dotnet $compilerPath run $inputPath }
    'Check' { & dotnet $compilerPath check $inputPath }
    'Preview' {
        $previewDirectory = Join-Path ([System.IO.Path]::GetTempPath()) 'HanCSharp-Preview'
        New-Item -ItemType Directory -Force -Path $previewDirectory | Out-Null
        $target = Join-Path $previewDirectory ([System.IO.Path]::GetFileNameWithoutExtension($inputPath) + '-' + [Guid]::NewGuid().ToString('N') + '.cs')
        & dotnet $compilerPath to-cs $inputPath $target
    }
    'ToKorean' {
        if ([System.IO.Path]::GetExtension($inputPath) -ne '.cs') { throw '현재 문서로 C# 파일을 선택하세요.' }
        # 원본과 기존 한국어 파일을 보호하도록 새 이름을 사용한다.
        $target = [System.IO.Path]::ChangeExtension($inputPath, '.converted.han')
        & dotnet $compilerPath to-ko $inputPath $target
    }
}
exit $LASTEXITCODE
