param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,
    [switch]$IncludeExamples
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath $ProjectPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $projectRoot 'Assets')) -or
    -not (Test-Path -LiteralPath (Join-Path $projectRoot 'ProjectSettings'))) {
    throw 'Assets와 ProjectSettings가 있는 Unity 프로젝트 루트를 지정하세요.'
}

$copies = [System.Collections.Generic.List[object]]::new()
$appFolder = if (Test-Path -LiteralPath (Join-Path $PSScriptRoot 'app-portable\HanCSharp.Cli.exe')) { 'app-portable' } else { 'app' }
$appRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot $appFolder)).Path
foreach ($sourceFile in Get-ChildItem -LiteralPath $appRoot -Recurse -File) {
    $relativePath = $sourceFile.FullName.Substring($appRoot.Length).TrimStart('\', '/')
    $copies.Add(@{ Source = $sourceFile.FullName; Target = (Join-Path $projectRoot ('Tools\HanCSharp\' + $relativePath)) })
}
foreach ($sourceFile in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'unity\Editor') -Filter '*.cs') {
    $copies.Add(@{ Source = $sourceFile.FullName; Target = (Join-Path $projectRoot ('Assets\HanCSharp\Editor\' + $sourceFile.Name)) })
}
if ($IncludeExamples) {
    foreach ($sourceFile in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'samples\UnityScripts') -Filter '*.han') {
        $copies.Add(@{ Source = $sourceFile.FullName; Target = (Join-Path $projectRoot ('Assets\HanCSharp\Examples\' + $sourceFile.Name)) })
    }
}

# 먼저 모든 경로를 검사하여 사용자 수정 파일이 있으면 아무것도 복사하지 않는다.
foreach ($copy in $copies) {
    if (Test-Path -LiteralPath $copy.Target) {
        if ((Get-FileHash -LiteralPath $copy.Source).Hash -ne (Get-FileHash -LiteralPath $copy.Target).Hash) {
            throw "기존 파일과 내용이 다릅니다. 덮어쓰지 않습니다: $($copy.Target)"
        }
    }
}
foreach ($copy in $copies) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $copy.Target) | Out-Null
    if (-not (Test-Path -LiteralPath $copy.Target)) {
        Copy-Item -LiteralPath $copy.Source -Destination $copy.Target
    }
}
Write-Output "설치 완료: $projectRoot"
Write-Output 'Unity에서 Tools > HanCSharp > Settings를 확인하세요. .han을 저장하거나 Generate All로 C#을 생성합니다.'
