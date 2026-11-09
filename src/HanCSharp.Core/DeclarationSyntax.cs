using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace HanCSharp;

/// <summary>C# 선언 경계에 해당하는 한국어 문법을 처리한다. 실행문과 파일 입출력은 처리하지 않는다.</summary>
internal static class DeclarationSyntax
{
    private static readonly IReadOnlyDictionary<string, string> _modifiers = new Dictionary<string, string>
    {
        ["공개"] = "public", ["비공개"] = "private", ["보호"] = "protected", ["내부"] = "internal",
        ["정적"] = "static", ["읽기전용"] = "readonly", ["상수"] = "const", ["가상"] = "virtual",
        ["재정의"] = "override", ["추상"] = "abstract", ["봉인"] = "sealed", ["부분"] = "partial",
        ["비동기"] = "async"
    };

    public static string ToKoreanModifier(string text) => _modifiers.FirstOrDefault(p => p.Value == text).Key ?? text;

    public static bool TryParse(string line, int number, out string code, out string? block, ISet<string>? declaredBooleanNames = null)
    {
        code = "";
        block = null;
        if (line.StartsWith("가져오기 ", StringComparison.Ordinal) && line.EndsWith('.'))
        {
            string name = line[5..^1].Trim();
            if (SyntaxFactory.ParseName(name).ContainsDiagnostics)
            {
                throw new LanguageException(number, "가져올 이름공간을 확인하세요.");
            }
            code = $"using {name};";
            return true;
        }
        if (line.StartsWith("이름공간 ", StringComparison.Ordinal) && line.EndsWith(':'))
        {
            string name = line[5..^1].Trim();
            if (SyntaxFactory.ParseName(name).ContainsDiagnostics)
            {
                throw new LanguageException(number, "이름공간 이름을 확인하세요.");
            }
            code = $"namespace {name} {{";
            block = "namespace";
            return true;
        }
        if (line.StartsWith("특성 ", StringComparison.Ordinal) && line.EndsWith('.'))
        {
            string attribute = line[3..^1].Trim();
            if (SyntaxFactory.ParseMemberDeclaration($"[{attribute}] int _attributeCheck;")?.ContainsDiagnostics != false)
            {
                throw new LanguageException(number, "특성은 '특성 SerializeField.' 또는 '특성 Range(0f, 10f).'처럼 작성하세요.");
            }
            code = $"[{attribute}]";
            return true;
        }

        Match declaration = Regex.Match(line, @"^(?<prefix>.*?)\b(?<kind>클래스|필드|속성|이벤트|함수) (?<body>.+)$",
            RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        if (!declaration.Success)
        {
            return false;
        }
        string kind = declaration.Groups["kind"].Value;
        string prefix = declaration.Groups["prefix"].Value.Trim();
        // 수식어가 없는 함수는 기존 지역 함수 문법으로 처리한다.
        if (kind == "함수" && prefix.Length == 0)
        {
            return false;
        }
        string modifiers = ParseModifiers(prefix, number);
        string body = declaration.Groups["body"].Value;
        Match match;
        switch (kind)
        {
            case "클래스":
                match = Match(body, @"^(?<name>\S+?)(?:(?:은|는) (?<base>.+?)(?:을|를) 상속한다)?:$");
                if (!match.Success) break;
                string name = SyntaxRules.Identifier(match.Groups["name"].Value, number);
                string bases = match.Groups["base"].Success
                    ? " : " + string.Join(", ", match.Groups["base"].Value.Split(',').Select(t => SyntaxRules.Type(t.Trim(), number))) : "";
                code = $"{modifiers}class {name}{bases} {{";
                block = "class";
                return true;
            case "함수":
                match = Match(body, @"^(?<name>\S+)\((?<args>.*)\)(?:(?:은|는) (?<type>\S+))?:$");
                if (!match.Success) break;
                string returnType = match.Groups["type"].Success ? SyntaxRules.Type(match.Groups["type"].Value, number, true) : "void";
                code = $"{modifiers}{returnType} {SyntaxRules.Identifier(match.Groups["name"].Value, number)}({SyntaxRules.Parameters(match.Groups["args"].Value, number)}) {{";
                block = "method";
                return true;
            case "필드":
            case "이벤트":
                match = Match(body, @"^(?<type>\S+) (?<name>\S+?)(?:(?:은|는) (?<value>.+)이다)?\.$");
                if (!match.Success) break;
                string initializer = match.Groups["value"].Success ? " = " + SyntaxRules.Expression(match.Groups["value"].Value, number, declaredBooleanNames) : "";
                code = $"{modifiers}{(kind == "이벤트" ? "event " : "")}{SyntaxRules.Type(match.Groups["type"].Value, number)} {SyntaxRules.Identifier(match.Groups["name"].Value, number)}{initializer};";
                return true;
            case "속성":
                match = Match(body, @"^(?<type>\S+) (?<name>\S+?)(?:은|는) (?<value>.+)이다\.$");
                if (!match.Success) break;
                code = $"{modifiers}{SyntaxRules.Type(match.Groups["type"].Value, number)} {SyntaxRules.Identifier(match.Groups["name"].Value, number)} => {SyntaxRules.Expression(match.Groups["value"].Value, number, declaredBooleanNames)};";
                return true;
        }
        throw new LanguageException(number, $"{kind} 선언의 형식을 확인하세요. 문법 안내의 선언 예제를 참고하세요.");
    }

    private static string ParseModifiers(string text, int line)
    {
        var result = new List<string>();
        foreach (string word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!_modifiers.TryGetValue(word, out string? modifier))
            {
                throw new LanguageException(line, $"알 수 없는 수식어입니다: {word}");
            }
            result.Add(modifier);
        }
        return result.Count == 0 ? "" : string.Join(" ", result) + " ";
    }

    private static Match Match(string text, string pattern) => Regex.Match(text, pattern,
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
}
