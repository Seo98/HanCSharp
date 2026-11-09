using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HanCSharp;

/// <summary>
/// C# 구문 트리를 한국어 문장으로 표시한다. 지원 범위 밖의 구문과 주석을 포함한
/// 구문은 C# 원문 블록으로 보존한다. 공백 배치는 원본과 같음을 보장하지 않는다.
/// </summary>
public sealed class CSharpToKorean
{
    public string Convert(string source)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        Diagnostic? error = root.GetDiagnostics().FirstOrDefault(d => d.Severity == DiagnosticSeverity.Error);
        if (error is not null)
        {
            throw new ArgumentException($"입력 C# 문법 오류: {error}");
        }
        var output = new StringBuilder();
        // 전처리기는 활성/비활성 코드 및 파일 순서에 영향을 주므로 파일 전체를 보존한다.
        if (root.ContainsDirectives || root.AttributeLists.Count > 0)
        {
            Raw(output, source, 0);
            return output.ToString();
        }
        foreach (var item in root.Externs)
        {
            Raw(output, item.ToFullString(), 0);
        }
        foreach (var item in root.Usings)
        {
            WriteUsing(output, item, 0);
        }
        foreach (MemberDeclarationSyntax member in root.Members)
        {
            WriteMember(output, member, 0);
        }
        string ending = root.EndOfFileToken.ToFullString();
        if (!string.IsNullOrWhiteSpace(ending))
        {
            Raw(output, ending, 0);
        }
        return output.ToString();
    }

    private static void WriteUsing(StringBuilder output, UsingDirectiveSyntax item, int depth)
    {
        if (!HasComments(item) && item.Alias is null && item.Name is not null
            && item.StaticKeyword.IsKind(SyntaxKind.None) && item.GlobalKeyword.IsKind(SyntaxKind.None))
        {
            Line(output, depth, $"가져오기 {item.Name}.");
        }
        else Raw(output, item.ToFullString(), depth);
    }

    private static void WriteMember(StringBuilder output, MemberDeclarationSyntax member, int depth)
    {
        if (member is GlobalStatementSyntax global)
        {
            WriteStatement(output, global.Statement, depth);
            return;
        }
        if (member is BaseNamespaceDeclarationSyntax space && !HasOutsideComments(space, space.Members.Cast<SyntaxNode>().Concat(space.Usings).Concat(space.Externs)))
        {
            Line(output, depth, $"이름공간 {space.Name}:");
            foreach (var external in space.Externs) Raw(output, external.ToFullString(), depth + 1);
            foreach (var item in space.Usings) WriteUsing(output, item, depth + 1);
            foreach (var child in space.Members) WriteMember(output, child, depth + 1);
            Line(output, depth, "끝.");
            return;
        }
        if (member is ClassDeclarationSyntax type && type.TypeParameterList is null && type.ConstraintClauses.Count == 0
            && type.ParameterList is null && !HasOutsideComments(type, type.Members)
            && type.AttributeLists.All(IsSingleLine)
            && Modifiers(type.Modifiers) is string classModifiers
            && (type.BaseList is null || type.BaseList.Types.All(t => IsReversibleType(t.Type))))
        {
            WriteAttributes(output, type.AttributeLists, depth);
            string inheritance = type.BaseList is null ? "" : KoreanParticle.Select(type.Identifier.Text, "은", "는") + " "
                + string.Join(", ", type.BaseList.Types.Select(t => LanguageDefinition.ToKoreanType(t.Type.ToString()))) + "를 상속한다";
            Line(output, depth, $"{classModifiers}클래스 {type.Identifier.Text}{inheritance}:");
            foreach (var child in type.Members) WriteMember(output, child, depth + 1);
            Line(output, depth, "끝.");
            return;
        }
        if (HasComments(member))
        {
            Raw(output, member.ToFullString(), depth);
            return;
        }
        switch (member)
        {
            case BaseFieldDeclarationSyntax field when field.Declaration.Variables.Count == 1
                && field.AttributeLists.All(IsSingleLine)
                && IsReversibleType(field.Declaration.Type) && Modifiers(field.Modifiers) is string modifiers
                && field.Declaration.Variables[0].ArgumentList is null:
                var variable = field.Declaration.Variables[0];
                string initializer = variable.Initializer is null ? "" : KoreanParticle.Select(variable.Identifier.Text, "은", "는") + " " + variable.Initializer.Value + "이다";
                if (initializer.Contains('\n') || initializer.Contains('\r')) break;
                WriteAttributes(output, field.AttributeLists, depth);
                string kind = field is EventFieldDeclarationSyntax ? "이벤트" : "필드";
                Line(output, depth, $"{modifiers}{kind} {LanguageDefinition.ToKoreanType(field.Declaration.Type.ToString())} {variable.Identifier.Text}{initializer}.");
                return;
            case MethodDeclarationSyntax method when method.Body is not null && method.TypeParameterList is null
                && method.AttributeLists.All(IsSingleLine)
                && method.ExplicitInterfaceSpecifier is null && method.ConstraintClauses.Count == 0
                && IsReversibleType(method.ReturnType) && Modifiers(method.Modifiers) is string methodModifiers
                && CanWriteParameters(method.ParameterList):
                WriteAttributes(output, method.AttributeLists, depth);
                string parameters = WriteParameters(method.ParameterList);
                // 수식어 없는 멤버 함수는 C#의 기본 private를 명시한다.
                if (methodModifiers.Length == 0) methodModifiers = "비공개 ";
                string returns = method.ReturnType.ToString() == "void" ? "" : KoreanParticle.Select(method.Identifier.Text, "은", "는") + " " + LanguageDefinition.ToKoreanType(method.ReturnType.ToString());
                Line(output, depth, $"{methodModifiers}함수 {method.Identifier.Text}({parameters}){returns}:");
                WriteBody(output, method.Body, depth + 1);
                Line(output, depth, "함수 끝.");
                return;
            case PropertyDeclarationSyntax property when property.ExpressionBody is not null
                && property.AttributeLists.All(IsSingleLine)
                && property.AccessorList is null && property.ExplicitInterfaceSpecifier is null
                && IsReversibleType(property.Type) && IsSingleLine(property.ExpressionBody.Expression)
                && Modifiers(property.Modifiers) is string propertyModifiers:
                WriteAttributes(output, property.AttributeLists, depth);
                Line(output, depth, $"{propertyModifiers}속성 {LanguageDefinition.ToKoreanType(property.Type.ToString())} {property.Identifier.Text}{KoreanParticle.Select(property.Identifier.Text, "은", "는")} {property.ExpressionBody.Expression}이다.");
                return;
        }
        Raw(output, member.ToFullString(), depth);
    }

    private static bool HasOutsideComments(SyntaxNode node, IEnumerable<SyntaxNode> children)
    {
        var spans = children.Select(c => c.FullSpan).ToArray();
        return node.DescendantTrivia().Any(t => !t.IsKind(SyntaxKind.WhitespaceTrivia)
            && !t.IsKind(SyntaxKind.EndOfLineTrivia) && !spans.Any(s => s.Contains(t.Span)));
    }

    private static string? Modifiers(SyntaxTokenList modifiers)
    {
        var result = new List<string>();
        foreach (SyntaxToken modifier in modifiers)
        {
            string korean = DeclarationSyntax.ToKoreanModifier(modifier.Text);
            if (korean == modifier.Text) return null;
            result.Add(korean);
        }
        return result.Count == 0 ? "" : string.Join(" ", result) + " ";
    }

    private static void WriteAttributes(StringBuilder output, SyntaxList<AttributeListSyntax> attributes, int depth)
    {
        foreach (var attribute in attributes)
        {
            Line(output, depth, $"특성 {attribute.ToString()[1..^1]}.");
        }
    }

    private static bool CanWriteParameters(ParameterListSyntax parameters) => parameters.Parameters.All(p =>
        p.Type is not null && IsReversibleType(p.Type) && p.Modifiers.Count == 0 && p.AttributeLists.Count == 0 && p.Default is null);

    private static string WriteParameters(ParameterListSyntax parameters) => string.Join(", ", parameters.Parameters.Select(p =>
        $"{LanguageDefinition.ToKoreanType(p.Type!.ToString())} {p.Identifier.Text}"));

    private static void WriteStatement(StringBuilder output, StatementSyntax statement, int depth)
    {
        if (HasComments(statement))
        {
            Raw(output, statement.ToFullString(), depth);
            return;
        }
        string? sentence = SimpleStatement(statement);
        if (sentence is not null && !sentence.Contains('\n') && !sentence.Contains('\r'))
        {
            Line(output, depth, sentence);
            return;
        }
        switch (statement)
        {
            case ForStatementSyntax repeat when RepeatHeader(repeat) is string header:
                Line(output, depth, header);
                WriteBody(output, (BlockSyntax)repeat.Statement, depth + 1);
                Line(output, depth, "반복 끝.");
                return;
            case IfStatementSyntax conditional when conditional.Statement is BlockSyntax body
                && (conditional.Else is null || conditional.Else.Statement is BlockSyntax)
                && IsSingleLine(conditional.Condition):
                Line(output, depth, $"만약 {Condition(conditional.Condition)}:");
                WriteBody(output, body, depth + 1);
                if (conditional.Else?.Statement is BlockSyntax alternative)
                {
                    Line(output, depth, "아니면:");
                    WriteBody(output, alternative, depth + 1);
                }
                Line(output, depth, "조건 끝.");
                return;
            case WhileStatementSyntax loop when loop.Statement is BlockSyntax body && IsSingleLine(loop.Condition):
                Line(output, depth, $"{WhileCondition(loop.Condition)} 동안:");
                WriteBody(output, body, depth + 1);
                Line(output, depth, "반복 끝.");
                return;
            case LocalFunctionStatementSyntax function when CanWriteFunction(function):
                string parameters = string.Join(", ", function.ParameterList.Parameters.Select(p =>
                    $"{LanguageDefinition.ToKoreanType(p.Type!.ToString())} {p.Identifier.Text}"));
                string topic = KoreanParticle.Select(function.Identifier.Text, "은", "는");
                string functionReturns = function.ReturnType.ToString() == "void" ? "" : topic + " " + LanguageDefinition.ToKoreanType(function.ReturnType.ToString());
                Line(output, depth, $"함수 {function.Identifier.Text}({parameters}){functionReturns}:");
                WriteBody(output, function.Body!, depth + 1);
                Line(output, depth, "함수 끝.");
                return;
            default:
                Raw(output, statement.ToFullString(), depth);
                return;
        }
    }

    private static string? SimpleStatement(StatementSyntax statement)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax local when local.Modifiers.Count == 0
                && local.AttributeLists.Count == 0 && local.UsingKeyword.IsKind(SyntaxKind.None)
                && local.AwaitKeyword.IsKind(SyntaxKind.None) && local.Declaration.Variables.Count == 1
                && IsReversibleType(local.Declaration.Type):
                VariableDeclaratorSyntax variable = local.Declaration.Variables[0];
                if (variable.Initializer is null || variable.ArgumentList is not null)
                {
                    return null;
                }
                string topic = KoreanParticle.Select(variable.Identifier.Text, "은", "는");
                return $"{LanguageDefinition.ToKoreanType(local.Declaration.Type.ToString())} {variable.Identifier.Text}{topic} {variable.Initializer.Value}이다.";
            case ExpressionStatementSyntax expression:
                if (expression.Expression is AssignmentExpressionSyntax assignment && assignment.Left is IdentifierNameSyntax identifier)
                {
                    string? verb = assignment.Kind() switch
                    {
                        SyntaxKind.SimpleAssignmentExpression => "넣는다",
                        SyntaxKind.AddAssignmentExpression => "더한다",
                        SyntaxKind.SubtractAssignmentExpression => "뺀다",
                        SyntaxKind.MultiplyAssignmentExpression => "곱한다",
                        _ => null
                    };
                    if (verb is not null)
                    {
                        string particle = verb == "뺀다" ? "에서" : "에";
                        string objectParticle = KoreanParticle.Select(assignment.Right.ToString(), "을", "를");
                        return $"{identifier.Identifier.Text}{particle} {assignment.Right}{objectParticle} {verb}.";
                    }
                }
                if (expression.Expression is InvocationExpressionSyntax invocation
                    && invocation.Expression.ToString() == "global::System.Console.WriteLine"
                    && invocation.ArgumentList.Arguments.Count == 1
                    && invocation.ArgumentList.Arguments[0].NameColon is null
                    && invocation.ArgumentList.Arguments[0].RefKindKeyword.IsKind(SyntaxKind.None))
                {
                    return $"출력 {invocation.ArgumentList.Arguments[0].Expression}.";
                }
                return $"실행 {expression.Expression}.";
            case ReturnStatementSyntax result:
                return result.Expression is null ? "반환." : $"반환 {result.Expression}.";
            case YieldStatementSyntax yieldStatement:
                return yieldStatement.Expression is null ? "양보종료." : $"기다린다 {yieldStatement.Expression}.";
            case BreakStatementSyntax:
                return "중단.";
            case ContinueStatementSyntax:
                return "계속.";
            default:
                return null;
        }
    }

    private static bool CanWriteFunction(LocalFunctionStatementSyntax function) =>
        function.Body is not null && function.Modifiers.Count == 0 && function.AttributeLists.Count == 0
        && function.TypeParameterList is null && function.ConstraintClauses.Count == 0
        && IsReversibleType(function.ReturnType)
        && function.ParameterList.Parameters.All(p => p.Type is not null
            && IsReversibleType(p.Type) && !p.Type.ToString().Contains(',')
            && p.Modifiers.Count == 0 && p.AttributeLists.Count == 0 && p.Default is null);

    private static bool HasComments(SyntaxNode node) => node.DescendantTrivia().Any(t =>
        !t.IsKind(SyntaxKind.WhitespaceTrivia) && !t.IsKind(SyntaxKind.EndOfLineTrivia));

    private static bool IsSingleLine(SyntaxNode node) => !node.ToString().Contains('\n') && !node.ToString().Contains('\r');

    private static bool IsReversibleType(TypeSyntax type)
    {
        string text = type.ToString();
        return !text.Any(char.IsWhiteSpace)
            && LanguageDefinition.ToCSharpType(LanguageDefinition.ToKoreanType(text)) == text;
    }

    private static string? RepeatHeader(ForStatementSyntax loop)
    {
        if (loop.Statement is not BlockSyntax || loop.Initializers.Count != 0
            || loop.Declaration is not { Variables.Count: 1 } declaration
            || !declaration.Type.IsKind(SyntaxKind.PredefinedType) || declaration.Type.ToString() != "int"
            || declaration.Variables[0].Initializer?.Value is not LiteralExpressionSyntax zero || zero.Token.ValueText != "0"
            || loop.Condition is not BinaryExpressionSyntax comparison || !comparison.IsKind(SyntaxKind.LessThanExpression)
            || comparison.Left is not IdentifierNameSyntax left || loop.Incrementors.Count != 1
            || loop.Incrementors[0] is not PostfixUnaryExpressionSyntax increment
            || !increment.IsKind(SyntaxKind.PostIncrementExpression) || increment.Operand is not IdentifierNameSyntax operand)
        {
            return null;
        }
        string name = declaration.Variables[0].Identifier.Text;
        if (left.Identifier.Text != name || operand.Identifier.Text != name || !IsSingleLine(comparison.Right))
        {
            return null;
        }
        ExpressionSyntax count = comparison.Right is ParenthesizedExpressionSyntax parenthesized
            ? parenthesized.Expression : comparison.Right;
        string topic = KoreanParticle.Select(name, "은", "는");
        if (name.StartsWith("__hanRepeat", StringComparison.Ordinal) && int.TryParse(count.ToString(), out int times) && times >= 0
            && !loop.Statement.DescendantTokens().Any(token => token.ValueText == name))
            return $"{times}번 반복:";
        return $"반복 {name}{topic} {count}회:";
    }

    private static string WhileCondition(ExpressionSyntax expression)
    {
        string condition = Condition(expression);
        foreach (var pair in new[] { ("크거나 같으면", "크거나 같은"), ("작거나 같으면", "작거나 같은"),
            ("크면", "큰"), ("작으면", "작은"), ("같으면", "같은"), ("다르면", "다른"), ("이면", "인") })
        {
            if (condition.EndsWith(pair.Item1, StringComparison.Ordinal))
                return condition[..^pair.Item1.Length] + pair.Item2;
        }
        return condition + "인";
    }

    private static string Condition(ExpressionSyntax expression)
    {
        if (expression is BinaryExpressionSyntax binary && Unwrap(binary.Left) is IdentifierNameSyntax)
        {
            string? comparison = binary.Kind() switch
            {
                SyntaxKind.GreaterThanExpression => "보다 크면",
                SyntaxKind.LessThanExpression => "보다 작으면",
                SyntaxKind.GreaterThanOrEqualExpression => "보다 크거나 같으면",
                SyntaxKind.LessThanOrEqualExpression => "보다 작거나 같으면",
                SyntaxKind.EqualsExpression => "와 같으면",
                SyntaxKind.NotEqualsExpression => "와 다르면",
                _ => null
            };
            if (comparison is not null)
            {
                ExpressionSyntax left = Unwrap(binary.Left);
                ExpressionSyntax right = Unwrap(binary.Right);
                string subject = KoreanParticle.Select(left.ToString(), "이", "가");
                if (comparison.StartsWith("와", StringComparison.Ordinal))
                {
                    comparison = KoreanParticle.Select(right.ToString(), "과", "와") + comparison[1..];
                }
                return $"{left}{subject} {right}{comparison}";
            }
        }
        return $"{expression}이면";
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }
        return expression;
    }

    private static void WriteBody(StringBuilder output, BlockSyntax body, int depth)
    {
        foreach (StatementSyntax statement in body.Statements)
        {
            WriteStatement(output, statement, depth);
        }
    }

    private static void Line(StringBuilder output, int depth, string text) =>
        output.Append(' ', depth * 4).Append(text).Append('\n');

    private static void Raw(StringBuilder output, string source, int depth)
    {
        int length = Math.Max(3, Regex.Matches(source, "`+").Select(m => m.Length + 1).DefaultIfEmpty(3).Max());
        string fence = new('`', length);
        Line(output, depth, fence + "csharp");
        output.Append(source);
        if (!source.EndsWith('\n') && !source.EndsWith('\r'))
        {
            output.Append('\n');
        }
        Line(output, depth, fence);
    }
}
