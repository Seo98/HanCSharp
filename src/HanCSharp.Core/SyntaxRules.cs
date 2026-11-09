using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HanCSharp;

/// <summary>선언과 실행문에서 공유하는 C# 이름·자료형·식 검증 규칙이다.</summary>
internal static class SyntaxRules
{
    public static string Parameters(string text, int line)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        // 제네릭 내부 쉼표는 매개변수 구분자로 취급하지 않는다.
        var parameters = new List<string>();
        int start = 0;
        int depth = 0;
        for (int index = 0; index <= text.Length; index++)
        {
            if (index < text.Length)
            {
                if (text[index] is '<' or '[' or '(') depth++;
                if (text[index] is '>' or ']' or ')') depth--;
            }
            if (index != text.Length && (text[index] != ',' || depth != 0)) continue;
            string parameter = text[start..index].Trim();
            int separator = parameter.LastIndexOf(' ');
            if (separator <= 0)
            {
                throw new LanguageException(line, "매개변수는 '정수 x, 문자열 이름'처럼 작성하세요.");
            }
            parameters.Add($"{Type(parameter[..separator], line)} {Identifier(parameter[(separator + 1)..], line)}");
            start = index + 1;
        }
        return string.Join(", ", parameters);
    }

    public static string Type(string text, int line, bool allowVoid = false)
    {
        if (LanguageDefinition.ToCSharpType(text) == "void")
        {
            if (allowVoid) return "void";
            throw new LanguageException(line, "없음은 함수 반환형에서만 사용할 수 있습니다.");
        }
        TypeSyntax parsed = SyntaxFactory.ParseTypeName(text);
        if (parsed.ContainsDiagnostics) throw new LanguageException(line, $"올바르지 않은 자료형입니다: {text}");
        string type = new TypeAliasRewriter().Visit(parsed)!.ToString();
        if (type == "void" && !allowVoid)
        {
            throw new LanguageException(line, "없음은 함수 반환형에서만 사용할 수 있습니다.");
        }
        return type;
    }

    public static string Identifier(string text, int line)
    {
        if (!LanguageDefinition.IsIdentifier(text)) throw new LanguageException(line, $"올바르지 않은 이름입니다: {text}");
        return text;
    }

    public static string Expression(string text, int line, ISet<string>? declaredBooleanNames = null)
    {
        text = text.Trim();
        if (text.Length == 0 || SyntaxFactory.ParseExpression(text).ContainsDiagnostics)
        {
            throw new LanguageException(line, $"올바르지 않은 식입니다: {text}");
        }
        return declaredBooleanNames is null ? text : new BooleanLiterals(declaredBooleanNames)
            .Visit(SyntaxFactory.ParseExpression(text))!.ToFullString();
    }

    private sealed class TypeAliasRewriter : CSharpSyntaxRewriter
    {
        public override Microsoft.CodeAnalysis.SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Parent is QualifiedNameSyntax or AliasQualifiedNameSyntax) return node;
            string type = LanguageDefinition.ToCSharpType(node.Identifier.Text);
            return type == node.Identifier.Text ? node : SyntaxFactory.ParseTypeName(type);
        }
    }
}
