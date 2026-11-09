param(
    [ValidateSet('2022', '2026', 'All')]
    [string]$Version = '2022'
)

$ErrorActionPreference = 'Stop'
if (@(Get-Process devenv -ErrorAction SilentlyContinue).Count -gt 0) {
    throw '열려 있는 Visual Studio 문서를 저장하고 모든 Visual Studio 창을 닫은 뒤 실행하세요. 자동으로 종료하지 않습니다.'
}
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$instances = @((& $vswherePath -all -products '*' -format json | Out-String | ConvertFrom-Json))
$instances = @($instances | Where-Object {
    $major = ([version]$_.installationVersion).Major
    ($major -eq 17 -and $Version -in @('2022', 'All')) -or ($major -eq 18 -and $Version -in @('2026', 'All'))
})
if ($instances.Count -eq 0) { throw '대상 Visual Studio 설치를 찾지 못했습니다.' }

$projectRoot = Split-Path -Parent $PSScriptRoot
$packagePath = Join-Path $projectRoot 'visualstudio\HanCSharp-0.7.vsix'
if (-not (Test-Path -LiteralPath $packagePath)) { throw 'HanCSharp-0.7.vsix가 없습니다. Build-Extension.ps1로 먼저 빌드하세요.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    foreach ($instance in $instances) {
        $major = ([version]$instance.installationVersion).Major
        $profileRoot = Join-Path $env:LOCALAPPDATA ('Microsoft\VisualStudio\' + $major + '.0_' + $instance.instanceId)
        $target = Join-Path $profileRoot 'Extensions\HanCSharp.EditorSupport.0.3'
        if (Test-Path -LiteralPath $target) {
            $manifestPath = Join-Path $target 'extension.vsixmanifest'
            if (-not (Test-Path -LiteralPath $manifestPath)) { throw "알 수 없는 기존 폴더: $target" }
            [xml]$existing = Get-Content -LiteralPath $manifestPath -Raw
            if ($existing.PackageManifest.Metadata.Identity.Id -ne 'HanCSharp.VisualStudio.EditorSupport') {
                throw "다른 확장의 폴더입니다: $target"
            }
        }
        New-Item -ItemType Directory -Force -Path $target | Out-Null
        foreach ($name in @('extension.vsixmanifest', 'HanCSharp.VisualStudio.dll')) {
            $entry = $zip.GetEntry($name)
            if (-not $entry) { throw "패키지에 파일이 없습니다: $name" }
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $target $name), $true)
        }

        # 오래된 MEF 목록이 새 완성/분류기를 누락하지 않도록 재생성한다. 삭제 대신 같은 프로필에 백업한다.
        $cache = [System.IO.Path]::GetFullPath((Join-Path $profileRoot 'ComponentModelCache'))
        $backup = [System.IO.Path]::GetFullPath((Join-Path $profileRoot ('ComponentModelCache.HanCSharp-backup-' + [Guid]::NewGuid().ToString('N'))))
        $boundary = [System.IO.Path]::GetFullPath($profileRoot).TrimEnd('\') + '\'
        if (-not $cache.StartsWith($boundary, [System.StringComparison]::OrdinalIgnoreCase) -or
            -not $backup.StartsWith($boundary, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw '캐시 백업 경로가 Visual Studio 프로필 범위를 벗어났습니다.'
        }
        if (Test-Path -LiteralPath $cache) { Move-Item -LiteralPath $cache -Destination $backup }
        $devenvPath = Join-Path $instance.installationPath 'Common7\IDE\devenv.com'
        & $devenvPath /UpdateConfiguration
        if ($LASTEXITCODE -ne 0) { throw "Visual Studio 구성 갱신 실패: $($instance.displayName)" }
        Write-Output ("설치 완료: " + $instance.displayName + ' / ' + $target)
    }
} finally { $zip.Dispose() }
Write-Output 'Visual Studio 확장 관리 > 설치됨에서 한씨 언어 도구가 사용 상태인지 확인하세요.'
Write-Output '사용 안 함이면 … > 사용을 선택하고 Visual Studio를 다시 여세요. .han 파일에서 Ctrl+Space로 확인합니다.'
