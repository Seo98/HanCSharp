using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HanCSharp;

/// <summary>대상·값·행동 순서의 초급 문장을 해석한다. 블록 수명과 파일 처리는 호출자가 담당한다.</summary>
internal static class NaturalSentenceSyntax
{
    public static bool TryParse(string line, int number, string? parent, bool inUnity,
        ISet<string>? booleanNames, out string code, out string? block)
    {
        code = "";
        block = null;
        string Expression(string value) => SyntaxRules.Expression(value, number, booleanNames);
        Match match = Match(line, @"^(?<name>\S+?)(?:은|는) 유니티 부품이다:$");
        if (match.Success)
        {
            if (parent != null && parent != "namespace")
                throw new LanguageException(number, "유니티 부품은 다른 동작 안에 넣지 말고 파일의 바깥쪽에 작성하세요.");
            code = $"public sealed class {SyntaxRules.Identifier(match.Groups["name"].Value, number)} : global::UnityEngine.MonoBehaviour {{";
            block = "unity";
            return true;
        }
        if (line is "처음 시작할 때:" or "매 순간:")
        {
            if (parent != "unity")
                throw new LanguageException(number, "'처음 시작할 때:'와 '매 순간:'은 유니티 부품 바로 안에 작성하세요.");
            code = line == "처음 시작할 때:" ? "private void Start() {" : "private void Update() {";
            block = "method";
            return true;
        }
        match = Match(line, @"^(?<name>\S+?)(?:은|는) 이렇게 한다:$");
        if (match.Success)
        {
            string access = parent is "unity" or "class" ? "public " : "";
            code = $"{access}void {SyntaxRules.Identifier(match.Groups["name"].Value, number)}() {{";
            block = "function";
            return true;
        }
        match = Match(line, @"^(?<editable>조절할 )?(?<name>\S+?)(?:은|는) (?:(?<type>\S+)이며 처음에는 )?(?<value>.+)이다\.$");
        if (match.Success)
        {
            string name = SyntaxRules.Identifier(match.Groups["name"].Value, number);
            string value = Expression(match.Groups["value"].Value);
            bool member = parent is "unity" or "class";
            bool editable = match.Groups["editable"].Success;
            if (editable && parent != "unity")
                throw new LanguageException(number, "'조절할' 값은 유니티 부품 바로 안에 작성하세요. Inspector에서 바꿀 수 있는 값입니다.");
            string type = match.Groups["type"].Success ? SyntaxRules.Type(match.Groups["type"].Value, number) : "var";
            if (member && type == "var") type = LiteralType(value, number);
            if (type == "var" && SyntaxFactory.ParseExpression(value).IsKind(SyntaxKind.NullLiteralExpression))
                throw new LanguageException(number, "null만으로는 값의 종류를 알 수 없습니다. '대상은 객체이며 처음에는 null이다.'처럼 종류도 적어 주세요.");
            code = (editable ? "[global::UnityEngine.SerializeField] " : "") + (member ? "private " : "")
                + $"{type} {name} = {value};";
            return true;
        }
        match = Match(line, @"^(?:컴퓨터는 )?(?<value>.+)(?:을|를) 출력한다\.$");
        if (match.Success)
        {
            code = $"global::System.Console.WriteLine({Expression(match.Groups["value"].Value)});";
            return true;
        }
        match = Match(line, @"^(?:이 물체는 )?(?<value>.+)(?:을|를) 기록한다\.$");
        if (match.Success)
        {
            RequireUnity(inUnity, parent, number);
            code = $"global::UnityEngine.Debug.Log({Expression(match.Groups["value"].Value)});";
            return true;
        }
        match = Match(line, @"^이 물체는 (?<direction>오른쪽|왼쪽|위쪽|아래쪽|앞쪽|뒤쪽)으로 초당 (?<speed>.+)만큼 움직인다\.$");
        if (match.Success)
        {
            RequireUnity(inUnity, parent, number);
            string direction = match.Groups["direction"].Value switch
            {
                "오른쪽" => "right", "왼쪽" => "left", "위쪽" => "up", "아래쪽" => "down", "앞쪽" => "forward", _ => "back"
            };
            code = "transform.position += global::UnityEngine.Vector3." + direction
                + " * (float)(" + Expression(match.Groups["speed"].Value) + ") * global::UnityEngine.Time.deltaTime;";
            return true;
        }
        match = Match(line, @"^(?<action>.+)(?:을|를) 실행한다\.$");
        if (match.Success)
        {
            string action = match.Groups["action"].Value;
            if (SyntaxFactory.ParseExpression(action) is IdentifierNameSyntax) action += "()";
            string invocation = Expression(action);
            if (SyntaxFactory.ParseExpression(invocation) is not InvocationExpressionSyntax)
                throw new LanguageException(number, "실행할 동작의 이름을 적어 주세요. 예: '인사하기를 실행한다.'");
            code = invocation + ";";
            return true;
        }
        match = Match(line, @"^(?<value>.+)(?:을|를) 돌려준다\.$");
        if (match.Success)
        {
            code = $"return {Expression(match.Groups["value"].Value)};";
            return true;
        }
        code = line switch
        {
            "여기서 마친다." => "return;", "되풀이를 멈춘다." => "break;", "다음 차례로 넘어간다." => "continue;", _ => ""
        };
        return code.Length > 0;
    }

    private static string LiteralType(string value, int number)
    {
        var expression = SyntaxFactory.ParseExpression(value);
        if (expression is PrefixUnaryExpressionSyntax unary) expression = unary.Operand;
        if (value is "참" or "거짓") return "bool";
        if (expression is LiteralExpressionSyntax literal)
        {
            return literal.Token.Value switch
            {
                int => "int", uint => "uint", long => "long", ulong => "ulong", float => "float", double => "double",
                decimal => "decimal", string => "string", char => "char", bool => "bool",
                _ => throw new LanguageException(number, "부품의 처음 값에는 숫자·글자·참/거짓을 적거나 값의 종류를 함께 적어 주세요.")
            };
        }
        throw new LanguageException(number, "부품에서 계산식으로 값을 만들 때는 종류도 적어 주세요. 예: '체력은 정수이며 처음에는 50 + 50이다.'");
    }

    private static void RequireUnity(bool inUnity, string? parent, int number)
    {
        if (!inUnity || parent == "unity")
            throw new LanguageException(number, "물체의 행동은 유니티 부품의 '처음 시작할 때:' 또는 '매 순간:' 같은 동작 안에 작성하세요.");
    }

    private static Match Match(string text, string pattern) => Regex.Match(text, pattern,
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
}
