function ConvertTo-HanArgument([string]$Value) {
    $text = New-Object System.Text.StringBuilder
    [void]$text.Append('"')
    $slashes = 0
    foreach ($character in $Value.ToCharArray()) {
        if ($character -eq '\') { $slashes++; continue }
        $count = if ($character -eq '"') { $slashes * 2 + 1 } else { $slashes }
        [void]$text.Append(('\' * $count))
        [void]$text.Append($character)
        $slashes = 0
    }
    [void]$text.Append(('\' * ($slashes * 2)))
    [void]$text.Append('"')
    return $text.ToString()
}

function Invoke-HanLauncher([string]$Executable, [string[]]$Arguments) {
    $start = New-Object System.Diagnostics.ProcessStartInfo
    $start.FileName = $Executable
    $start.Arguments = ($Arguments | ForEach-Object { ConvertTo-HanArgument $_ }) -join ' '
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.StandardOutputEncoding = New-Object System.Text.UTF8Encoding($false)
    $start.StandardErrorEncoding = New-Object System.Text.UTF8Encoding($false)
    $process = [Diagnostics.Process]::Start($start)
    try {
        $output = $process.StandardOutput.ReadToEndAsync()
        $errorText = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(120000)) { $process.Kill(); throw '한씨 시작 도구 응답 시간이 초과되었습니다.' }
        return [pscustomobject]@{ ExitCode=$process.ExitCode; Output=$output.GetAwaiter().GetResult(); Error=$errorText.GetAwaiter().GetResult() }
    } finally { $process.Dispose() }
}
