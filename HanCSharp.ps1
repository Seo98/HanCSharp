param(
    [Parameter(Position = 0)]
    [string]$Command = 'help',
    [Parameter(Position = 1, ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArguments
)

$applicationPath = Join-Path $PSScriptRoot 'app\HanCSharp.Cli.exe'
if (Test-Path -LiteralPath $applicationPath) {
    & $applicationPath $Command @RemainingArguments
} else {
    $projectPath = Join-Path $PSScriptRoot 'src\HanCSharp.Cli\HanCSharp.Cli.csproj'
    & dotnet run --project $projectPath -- $Command @RemainingArguments
}
exit $LASTEXITCODE
