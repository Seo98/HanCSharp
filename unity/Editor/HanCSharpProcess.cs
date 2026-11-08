using System;
using System.Diagnostics;
using System.IO;
using System.Text;

/// <summary>셸을 거치지 않고 외부 변환기를 실행한다. Unity 런타임에는 포함하지 않는다.</summary>
public static class HanCSharpProcess
{
    public static bool Convert(string dotNetPath, string compilerPath, string input, string output, out string message, bool emitSourceMap = false)
    {
        // Windows 배포본은 런타임이 포함된 EXE를 우선 사용한다. 다른 환경은 dotnet + DLL로 실행한다.
        string executable = Path.ChangeExtension(compilerPath, ".exe");
        bool useExecutable = Path.DirectorySeparatorChar == '\\' && File.Exists(executable);
        var start = new ProcessStartInfo
        {
            FileName = useExecutable ? executable : dotNetPath,
            Arguments = (useExecutable ? "" : Quote(compilerPath) + " ") + "to-cs " + Quote(input) + " " + Quote(output)
                + (emitSourceMap ? " --source-map" : "") + " --force",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        using (var process = Process.Start(start))
        {
            if (process == null) throw new InvalidOperationException("한씨 변환기를 시작하지 못했습니다.");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(15000))
            {
                process.Kill();
                message = "한씨 변환 시간이 15초를 초과했습니다.";
                return false;
            }
            message = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
            return process.ExitCode == 0;
        }
    }

    // Windows argv 규칙에 맞게 공백, 따옴표, 끝의 역슬래시를 인용한다.
    public static string Quote(string value)
    {
        var result = new StringBuilder("\"");
        int slashes = 0;
        foreach (char character in value)
        {
            if (character == '\\')
            {
                slashes++;
                continue;
            }
            if (character == '"') result.Append('\\', slashes * 2 + 1);
            else result.Append('\\', slashes);
            result.Append(character);
            slashes = 0;
        }
        return result.Append('\\', slashes * 2).Append('"').ToString();
    }
}
