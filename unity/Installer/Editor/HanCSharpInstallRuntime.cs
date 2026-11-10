using System;
using System.Diagnostics;
using System.Text;

/// <summary>외부 .NET 런타임으로 스테이징 DLL을 실행해 실제 실행 가능 여부를 검사한다. SDK나 앱을 자동 설치하지 않는다.</summary>
public static class HanCSharpInstallRuntime
{
    public static string Check(string compiler)
    {
        var start = new ProcessStartInfo("dotnet", Quote(compiler) + " --help")
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        using (var process = Process.Start(start))
        {
            if (process == null) throw new InvalidOperationException("dotnet을 실행할 수 없습니다.");
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(30000))
            {
                process.Kill();
                throw new TimeoutException(".NET 실행 확인 시간이 초과됐습니다.");
            }
            if (process.ExitCode != 0) throw new InvalidOperationException(error.GetAwaiter().GetResult());
            return output.GetAwaiter().GetResult();
        }
    }

    public static string Quote(string value)
    {
        var result = new StringBuilder("\"");
        int slashes = 0;
        foreach (char c in value)
        {
            if (c == '\\') { slashes++; continue; }
            result.Append('\\', c == '"' ? slashes * 2 + 1 : slashes).Append(c);
            slashes = 0;
        }
        return result.Append('\\', slashes * 2).Append('"').ToString();
    }
}
