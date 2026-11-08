namespace HanCSharp;

/// <summary>한국어 원본의 줄 번호를 포함하는 문법 오류다.</summary>
public sealed class LanguageException : Exception
{
    public int LineNumber { get; }

    public LanguageException(int lineNumber, string message)
        : base($"한국어 {lineNumber}행: {message}")
    {
        LineNumber = lineNumber;
    }
}
