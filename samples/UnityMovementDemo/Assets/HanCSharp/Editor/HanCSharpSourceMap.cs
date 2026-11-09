using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>생성 C#을 간결하게 유지하면서 원본 행을 별도 보관한다. 수정된 생성 파일에는 매핑을 적용하지 않는다.</summary>
public static class HanCSharpSourceMap
{
    public static int[] ReadLineMap(string path)
    {
        // 변환기가 생성과 동시에 기록하므로 원문 문자열의 #line도 손상시키지 않는다.
        var lines = new List<int>();
        foreach (string value in File.ReadAllLines(path))
        {
            if (!int.TryParse(value, out int line) || line < 1)
                throw new IOException("한씨 변환기의 행 매핑 형식이 올바르지 않습니다.");
            lines.Add(line);
        }
        return lines.ToArray();
    }

    public static int DiagnosticLine(string message)
    {
        Match match = Regex.Match(message ?? "", @"\(([0-9]+),[0-9]+\):\s*error\s+HCS001:");
        return match.Success && int.TryParse(match.Groups[1].Value, out int line) ? Math.Max(1, line) : 1;
    }

    public static void Save(string generated, string source, string content, int[] sourceLines)
    {
        string path = MapPath(generated);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var text = new StringBuilder(Digest(content)).Append('\n').Append(Path.GetFullPath(source)).Append('\n');
        foreach (int line in sourceLines)
            text.Append(line).Append('\n');
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
    }

    public static bool TryResolve(string generated, int generatedLine, string source, out int sourceLine)
    {
        sourceLine = 1;
        string path = MapPath(generated);
        if (generatedLine < 2 || !File.Exists(path) || !File.Exists(generated))
            return false;
        string[] lines = File.ReadAllLines(path);
        // 생성 C# 첫 행의 소유권 표식 다음부터 매핑한다.
        int index = generatedLine;
        if (index >= lines.Length || lines.Length < 2 || lines[0] != Digest(File.ReadAllText(generated))
            || !lines[1].Equals(Path.GetFullPath(source), StringComparison.OrdinalIgnoreCase))
            return false;
        return int.TryParse(lines[index], out sourceLine) && sourceLine > 0;
    }

    private static string MapPath(string generated)
    {
        return Path.Combine("Library/HanCSharp/Maps", Digest(Path.GetFullPath(generated).ToUpperInvariant()) + ".map");
    }

    private static string Digest(string text)
    {
        using (var hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
    }
}
