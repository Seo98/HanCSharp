using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HanCSharp;

/// <summary>한국어 논리값을 식에서만 해석한다. 파일 안의 기존 선언 이름과 문자열·멤버명·자료형은 보존한다.</summary>
internal sealed class BooleanLiterals(ISet<string> declaredNames) : CSharpSyntaxRewriter
{
    public static HashSet<string> DeclaredNames(string generated)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        SyntaxNode root = CSharpSyntaxTree.ParseText(generated).GetRoot();
        // Unity 등의 외부 전처리 심벌로 활성화될 원문 이름도 보수적으로 보호한다.
        foreach (SyntaxTrivia trivia in root.DescendantTrivia().Where(t => t.IsKind(SyntaxKind.DisabledTextTrivia)))
        {
            foreach (SyntaxToken token in SyntaxFactory.ParseTokens(trivia.ToString()))
            {
                if (token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText is "참" or "거짓")
                    result.Add(token.ValueText);
            }
        }
        foreach (SyntaxNode node in root.DescendantNodes())
        {
            string? name = node switch
            {
                VariableDeclaratorSyntax n => n.Identifier.ValueText,
                ParameterSyntax n => n.Identifier.ValueText,
                MemberDeclarationSyntax n => n switch
                {
                    BaseTypeDeclarationSyntax t => t.Identifier.ValueText,
                    MethodDeclarationSyntax m => m.Identifier.ValueText,
                    PropertyDeclarationSyntax p => p.Identifier.ValueText,
                    EnumMemberDeclarationSyntax e => e.Identifier.ValueText,
                    EventDeclarationSyntax e => e.Identifier.ValueText,
                    DelegateDeclarationSyntax d => d.Identifier.ValueText,
                    _ => null
                },
                LocalFunctionStatementSyntax n => n.Identifier.ValueText,
                ForEachStatementSyntax n => n.Identifier.ValueText,
                SingleVariableDesignationSyntax n => n.Identifier.ValueText,
                CatchDeclarationSyntax n => n.Identifier.ValueText,
                NameEqualsSyntax n => n.Name.Identifier.ValueText,
                TypeParameterSyntax n => n.Identifier.ValueText,
                FromClauseSyntax n => n.Identifier.ValueText,
                LetClauseSyntax n => n.Identifier.ValueText,
                JoinClauseSyntax n => n.Identifier.ValueText,
                JoinIntoClauseSyntax n => n.Identifier.ValueText,
                QueryContinuationSyntax n => n.Identifier.ValueText,
                _ => null
            };
            if (name is "참" or "거짓") result.Add(name);
        }
        return result;
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        string name = node.Identifier.Text;
        if (name is not ("참" or "거짓") || declaredNames.Contains(name) || IsNameOrType(node))
            return node;
        if (node.Ancestors().OfType<InvocationExpressionSyntax>().Any(call => call.Expression.ToString() == "nameof"))
            return node;
        return SyntaxFactory.LiteralExpression(name == "참" ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)
            .WithTriviaFrom(node);
    }

    private static bool IsNameOrType(IdentifierNameSyntax node) => node.Parent switch
    {
        MemberAccessExpressionSyntax member => member.Name == node,
        MemberBindingExpressionSyntax => true,
        QualifiedNameSyntax or AliasQualifiedNameSyntax or NameColonSyntax or TypeArgumentListSyntax => true,
        InvocationExpressionSyntax call => call.Expression == node,
        ObjectCreationExpressionSyntax creation => creation.Type == node,
        CastExpressionSyntax cast => cast.Type == node,
        TypeOfExpressionSyntax or SizeOfExpressionSyntax or DefaultExpressionSyntax => true,
        ArrayTypeSyntax or NullableTypeSyntax or DeclarationPatternSyntax or TypePatternSyntax => true,
        _ => false
    };
}
