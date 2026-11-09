using System;
using System.IO;

/// <summary>저장된 한씨를 임시 파일로 변환한다. 생성 C#과 사용자 소스는 쓰지 않으며 Unity API에 의존하지 않는다.</summary>
public sealed class HanCSharpPreview
{
    public string SourceText { get; private set; } = "";
    public string GeneratedText { get; private set; } = "";
    public string Message { get; private set; } = "";
    public int ErrorLine { get; private set; }
    public bool Success { get; private set; }

    public static HanCSharpPreview Convert(string source, string dotNet, string compiler)
    {
        var result = new HanCSharpPreview();
        string temporary = Path.GetFullPath("Library/HanCSharp/Preview/" + Guid.NewGuid().ToString("N") + ".cs");
        try
        {
            result.SourceText = File.ReadAllText(source);
            Directory.CreateDirectory(Path.GetDirectoryName(temporary));
            result.Success = HanCSharpProcess.Convert(dotNet, compiler, Path.GetFullPath(source), temporary, out string message);
            result.Message = result.Success ? "변환 성공. C# 자료형·참조 오류는 Unity Console에서 확인하세요." : message;
            if (result.Success)
                result.GeneratedText = File.ReadAllText(temporary);
            else
                result.ErrorLine = HanCSharpSourceMap.DiagnosticLine(message);
            if (File.ReadAllText(source) != result.SourceText)
            {
                result.Success = false;
                result.GeneratedText = "";
                result.ErrorLine = 0;
                result.Message = "변환 중 원본이 변경됐습니다. 다시 새로고침하세요.";
            }
        }
        catch (Exception exception)
        {
            result.Message = exception.Message;
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
        return result;
    }
}
