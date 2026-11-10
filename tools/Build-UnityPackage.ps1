param([Parameter(Mandatory=$true)][string]$UnityProjectPath,[Parameter(Mandatory=$true)][string]$PayloadDirectory,[Parameter(Mandatory=$true)][string]$OutputPath)
$ErrorActionPreference='Stop'
$project=(Resolve-Path -LiteralPath $UnityProjectPath).Path
if(-not (Test-Path -LiteralPath (Join-Path $project 'Assets'))){throw 'Unity 프로젝트 루트가 아닙니다.'}
$destination=Join-Path $project 'Assets\HanCSharpInstaller'
$source=Join-Path ([IO.Path]::GetFullPath($PayloadDirectory)) 'HanCSharpInstaller'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item (Join-Path $source '*') $destination -Recurse -Force
$flag=Join-Path $project 'Library\HanCSharp\package-output.txt'
New-Item -ItemType Directory -Path (Split-Path -Parent $flag) -Force | Out-Null
[IO.File]::WriteAllText($flag,[IO.Path]::GetFullPath($OutputPath),[Text.UTF8Encoding]::new($false))
& unity command script run --file (Join-Path $PSScriptRoot 'Export-UnityPackage.cs') --project-path $project --format json
if($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $OutputPath)){throw 'Unity 패키지 내보내기 실패. 에디터와 Pipeline 연결을 확인하세요.'}
