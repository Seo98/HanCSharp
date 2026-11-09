using Microsoft.CodeAnalysis.CSharp;

namespace HanCSharp;

/// <summary>문장형 언어의 자료형 표기와 C# 자료형의 대응을 관리한다.</summary>
public static class LanguageDefinition
{
    private static readonly IReadOnlyDictionary<string, string> _types = new Dictionary<string, string>
    {
        ["정수"] = "int", ["긴정수"] = "long", ["실수"] = "double",
        ["단정도실수"] = "float", ["십진수"] = "decimal", ["문자열"] = "string",
        ["실수32"] = "float", ["실수64"] = "double",
        ["문자"] = "char", ["논리"] = "bool", ["객체"] = "object",
        ["자동"] = "var", ["없음"] = "void",
        ["벡터2"] = "global::UnityEngine.Vector2", ["벡터3"] = "global::UnityEngine.Vector3",
        ["회전"] = "global::UnityEngine.Quaternion", ["색상"] = "global::UnityEngine.Color",
        ["코루틴"] = "global::System.Collections.IEnumerator"
    };

    public static string ToCSharpType(string type) => _types.GetValueOrDefault(type, type);

    public static string ToKoreanType(string type) =>
        _types.FirstOrDefault(pair => pair.Value == type).Key ?? type;

    /// <summary>식 내부는 C# 표기를 사용한다. 별칭은 자료형 위치에서만 바뀐다.</summary>
    public static bool IsIdentifier(string name)
    {
        var token = SyntaxFactory.ParseToken(name);
        return token.Kind() == SyntaxKind.IdentifierToken && !token.ContainsDiagnostics
            && token.Text == name;
    }
}
