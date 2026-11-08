namespace HanCSharp;

/// <summary>생성 문장의 조사 선택을 돕는다. 한글 음절과 숫자를 처리하며 다른 문자는 기본 조사를 쓴다.</summary>
internal static class KoreanParticle
{
    public static string Select(string text, string withFinalConsonant, string withoutFinalConsonant)
    {
        string spokenEnd = text.TrimEnd(' ', ')', ']', '}', '\'', '"');
        if (spokenEnd.Length == 0)
        {
            return withoutFinalConsonant;
        }
        char last = spokenEnd[^1];
        bool hasFinal = last is >= '가' and <= '힣'
            ? (last - '가') % 28 != 0
            : last is '0' or '1' or '3' or '6' or '7' or '8';
        return hasFinal ? withFinalConsonant : withoutFinalConsonant;
    }
}
