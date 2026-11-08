using System;
using System.Collections.Generic;

namespace HanCSharp.EditorSupport
{
    /// <summary>표시용 토큰화다. 불완전한 입력도 처리하며 컴파일러의 문법 판정을 대체하지 않는다.</summary>
    public static class HanLexer
    {
        private static readonly HashSet<string> _keywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "가져오기", "이름공간", "클래스", "필드", "속성", "이벤트", "함수", "특성",
            "공개", "비공개", "보호", "내부", "정적", "읽기전용", "상수", "가상", "재정의", "추상", "봉인", "부분", "비동기",
            "만약", "아니면", "동안", "반복", "끝", "반환", "중단", "계속", "실행", "출력", "기록", "기다린다", "양보종료",
            "상속한다", "넣는다", "더한다", "뺀다", "곱한다", "크면", "작으면", "같으면", "다르면", "이면", "이다",
            "true", "false", "null", "new", "this", "base", "await", "nameof", "typeof", "default",
            "public", "private", "protected", "internal", "static", "class", "namespace", "using", "return", "if", "else", "while", "for", "foreach", "in", "yield", "break", "continue", "throw", "try", "catch", "finally"
        };
        private static readonly HashSet<string> _types = new HashSet<string>(StringComparer.Ordinal)
        {
            "정수", "긴정수", "실수", "단정도실수", "십진수", "문자열", "문자", "논리", "객체", "자동", "없음",
            "벡터2", "벡터3", "회전", "색상", "코루틴", "int", "long", "float", "double", "decimal", "string", "char", "bool", "object", "var", "void",
            "MonoBehaviour", "ScriptableObject", "Transform", "GameObject", "Vector2", "Vector3", "Quaternion", "Color", "IEnumerator", "Action", "List", "Dictionary"
        };

        public static bool IsWord(char character) => char.IsLetterOrDigit(character) || character == '_' || character == '@';

        public static IList<HanToken> Tokenize(string text)
        {
            var tokens = new List<HanToken>();
            int index = 0;
            while (index < text.Length)
            {
                int start = index;
                char current = text[index];
                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }
                if (current == '`')
                {
                    while (index < text.Length && text[index] != '\r' && text[index] != '\n')
                        index++;
                    tokens.Add(new HanToken(start, index - start, HanTokenKind.Fence));
                    continue;
                }
                if (current == '/' && index + 1 < text.Length && text[index + 1] == '/')
                {
                    while (index < text.Length && text[index] != '\r' && text[index] != '\n')
                        index++;
                    tokens.Add(new HanToken(start, index - start, HanTokenKind.Comment));
                    continue;
                }
                if (current == '/' && index + 1 < text.Length && text[index + 1] == '*')
                {
                    int close = text.IndexOf("*/", index + 2, StringComparison.Ordinal);
                    index = close < 0 ? text.Length : close + 2;
                    tokens.Add(new HanToken(start, index - start, HanTokenKind.Comment));
                    continue;
                }
                int quote = StringStart(text, index);
                if (quote >= 0)
                {
                    index = ReadString(text, start, quote);
                    tokens.Add(new HanToken(start, index - start, HanTokenKind.String));
                    continue;
                }
                if (char.IsDigit(current))
                {
                    index++;
                    while (index < text.Length && (char.IsDigit(text[index]) || "abcdefABCDEFxXbBuUlLfFdDmM_eE.".IndexOf(text[index]) >= 0))
                    {
                        if (text[index] == '.' && (index + 1 == text.Length || !char.IsDigit(text[index + 1])))
                            break;
                        index++;
                    }
                    tokens.Add(new HanToken(start, index - start, HanTokenKind.Number));
                    continue;
                }
                if (IsWord(current))
                {
                    while (index < text.Length && IsWord(text[index]))
                        index++;
                    string word = text.Substring(start, index - start);
                    HanTokenKind kind = _types.Contains(word) ? HanTokenKind.Type : _keywords.Contains(word) ? HanTokenKind.Keyword : HanTokenKind.Identifier;
                    tokens.Add(new HanToken(start, index - start, kind));
                    continue;
                }
                index++;
                tokens.Add(new HanToken(start, 1, HanTokenKind.Operator));
            }
            return tokens;
        }

        /// <summary>문자열과 주석에서는 완성을 억제한다. 문자열이 끝나기 전 EOF도 입력 중으로 본다.</summary>
        public static bool IsSuppressed(string text, int position)
        {
            foreach (HanToken token in Tokenize(text))
            {
                if (token.Kind != HanTokenKind.String && token.Kind != HanTokenKind.Comment && token.Kind != HanTokenKind.Fence)
                    continue;
                int end = token.Start + token.Length;
                if (position > token.Start && position < end)
                    return true;
                if (position == end && end == text.Length)
                {
                    string part = text.Substring(token.Start, token.Length);
                    if (token.Kind == HanTokenKind.Comment)
                        return !part.EndsWith("*/", StringComparison.Ordinal);
                    if (token.Kind == HanTokenKind.Fence)
                        return true;
                    // 마지막 위치에서 문자열이 정상 종료됐는지는 문자열을 하나 더 붙여 비교한다.
                    int quote = StringStart(text, token.Start);
                    if (quote >= 0 && ReadString(text + " ", token.Start, quote) > end)
                        return true;
                }
            }
            return false;
        }

        private static int StringStart(string text, int start)
        {
            int index = start;
            while (index < text.Length && (text[index] == '$' || text[index] == '@'))
                index++;
            if (index < text.Length && (text[index] == '"' || index == start && text[index] == '\''))
                return index;
            return -1;
        }

        private static int ReadString(string text, int start, int quote)
        {
            char delimiter = text[quote];
            int quotes = 1;
            while (quote + quotes < text.Length && text[quote + quotes] == delimiter)
                quotes++;
            if (delimiter == '"' && quotes >= 3)
            {
                string endMarker = new string('"', quotes);
                int close = text.IndexOf(endMarker, quote + quotes, StringComparison.Ordinal);
                return close < 0 ? text.Length : close + quotes;
            }
            string prefix = text.Substring(start, quote - start);
            bool verbatim = prefix.IndexOf('@') >= 0;
            bool interpolated = prefix.IndexOf('$') >= 0;
            int braces = 0;
            int index = quote + 1;
            while (index < text.Length)
            {
                char current = text[index];
                if (!verbatim && current == '\\')
                {
                    index = Math.Min(text.Length, index + 2);
                    continue;
                }
                if (interpolated && current == '{')
                {
                    if (braces == 0 && index + 1 < text.Length && text[index + 1] == '{')
                    {
                        index += 2;
                        continue;
                    }
                    braces++;
                }
                else if (interpolated && current == '}' && braces > 0)
                    braces--;
                else if (current == delimiter)
                {
                    if (braces > 0)
                    {
                        index = ReadString(text, index, index);
                        continue;
                    }
                    if (verbatim && index + 1 < text.Length && text[index + 1] == delimiter)
                    {
                        index += 2;
                        continue;
                    }
                    return index + 1;
                }
                if (!verbatim && (current == '\n' || current == '\r') && braces == 0)
                    return index;
                index++;
            }
            return index;
        }
    }
}
