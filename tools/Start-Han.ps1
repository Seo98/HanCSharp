param([switch]$SmokeTest)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $root 'app-portable\HanCSharp.Cli.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $root 'app\HanCSharp.Cli.exe' }
if (-not (Test-Path -LiteralPath $compiler)) {
    throw '실행 파일이 없습니다. GitHub Releases의 HanCSharp-Windows-x64.zip을 받아 압축을 모두 풀어주세요. 소스 빌드는 docs/QUICKSTART.md를 참고하세요.'
}
$sample = Join-Path $root 'samples\ConsoleGame\Main.han'
if ($SmokeTest) {
    & $compiler run $sample
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()
$form = New-Object System.Windows.Forms.Form
$form.Text = '한씨 시작하기'
$form.ClientSize = New-Object System.Drawing.Size(600,440)
$form.StartPosition = 'CenterScreen'
$form.Font = New-Object System.Drawing.Font('맑은 고딕',10)
$description = New-Object System.Windows.Forms.Label
$description.SetBounds(20,15,560,55)
$description.Text = "한국어 문장을 C#으로 만드는 한씨입니다.`r`n먼저 예제를 실행하거나, 기존 Unity 프로젝트에 설치하세요."
$form.Controls.Add($description)
$output = New-Object System.Windows.Forms.TextBox
$output.SetBounds(20,205,560,215)
$output.Multiline = $true
$output.ReadOnly = $true
$output.ScrollBars = 'Vertical'
$form.Controls.Add($output)

function Add-Button([string]$text, [int]$x, [int]$y, [scriptblock]$action) {
    $button = New-Object System.Windows.Forms.Button
    $button.Text = $text
    $button.SetBounds($x,$y,270,48)
    $button.Add_Click($action)
    $form.Controls.Add($button)
}

function Invoke-Sample {
    try {
        $output.Text = (& $compiler run $sample 2>&1 | Out-String)
        if ($LASTEXITCODE -ne 0) { throw $output.Text }
    } catch { $output.Text = $_.Exception.Message }
}
Add-Button '① 한씨 예제 바로 실행' 20 80 { Invoke-Sample }
Add-Button '② Unity 프로젝트에 설치' 310 80 {
    $picker = New-Object System.Windows.Forms.FolderBrowserDialog
    $picker.Description = 'Assets와 ProjectSettings가 있는 Unity 프로젝트 폴더를 선택하세요.'
    $picker.ShowNewFolderButton = $false
    try {
        if ($picker.ShowDialog($form) -eq 'OK') {
            $output.Text = (& (Join-Path $root 'Install-Unity.ps1') -ProjectPath $picker.SelectedPath -IncludeExamples | Out-String)
            $output.AppendText("`r`nUnity로 돌아가 컴파일을 기다린 뒤 Project 창 우클릭 → Create → HanCSharp → 한씨 컴포넌트를 사용하세요.")
        }
    } catch { $output.Text = $_.Exception.Message }
    finally { $picker.Dispose() }
}
Add-Button '③ Visual Studio 확장 설치 열기' 20 140 {
    try {
        Start-Process -FilePath (Join-Path $root 'visualstudio\HanCSharp-0.3.vsix')
        $output.Text = 'VSIX 설치 관리자에서 대상을 선택하세요. 설치가 안 되면 visualstudio/EDITOR-SUPPORT.md의 사용자 폴더 배포 방법을 참고하세요.'
    } catch { $output.Text = $_.Exception.Message }
}
Add-Button '처음 사용자 가이드 열기' 310 140 {
    $guide = New-Object System.Windows.Forms.Form
    $guide.Text = '한씨 처음 사용자 가이드'
    $guide.Size = New-Object System.Drawing.Size(850,650)
    $guide.StartPosition = 'CenterParent'
    $text = New-Object System.Windows.Forms.TextBox
    $text.Dock = 'Fill'
    $text.Multiline = $true
    $text.ReadOnly = $true
    $text.ScrollBars = 'Vertical'
    $text.Font = $form.Font
    $text.Text = Get-Content -LiteralPath (Join-Path $root 'docs\QUICKSTART.md') -Raw -Encoding UTF8
    $guide.Controls.Add($text)
    [void]$guide.ShowDialog($form)
    $guide.Dispose()
}
$output.Text = '① 버튼은 Unity나 Visual Studio 없이 한씨 코드를 실행합니다. 배포 ZIP에는 Windows x64용 .NET 런타임이 포함됩니다.'
$form.Add_Shown({ Invoke-Sample })
[void]$form.ShowDialog()
$form.Dispose()
