using HanCSharp;
using HanCSharp.EditorSupport;

int passed = 0;
int failed = 0;
Test("키워드/자료형/숫자 분리", () =>
{
    string code = "정수 체력은 100이다.";
    var tokens = HanLexer.Tokenize(code);
    Assert(tokens.Any(t => t.Kind == HanTokenKind.Type && code.Substring(t.Start, t.Length) == "정수"));
    Assert(tokens.Any(t => t.Kind == HanTokenKind.Number && code.Substring(t.Start, t.Length) == "100"));
    Assert(tokens.Any(t => t.Kind == HanTokenKind.Keyword && code.Substring(t.Start, t.Length) == "이다"));
});
Test("문자열 내부 키워드와 주석 표식 보호", () =>
{
    string code = "출력 \"정수 // 만약\". // 설명";
    var tokens = HanLexer.Tokenize(code);
    Assert(tokens.Count(t => t.Kind == HanTokenKind.String) == 1);
    Assert(tokens.Count(t => t.Kind == HanTokenKind.Comment) == 1);
});
Test("여러 줄 주석과 raw 문자열", () =>
{
    string code = "/* 정수\n만약 */\nvar s = \"\"\"\n정수 a는 0이다.\n\"\"\";";
    Assert(HanLexer.Tokenize(code).Count(t => t.Kind == HanTokenKind.String) == 1);
    Assert(HanLexer.Tokenize(code).Count(t => t.Kind == HanTokenKind.Comment) == 1);
});
Test("문자/이스케이프/verbatim/보간의 따옴표", () =>
{
    foreach (string code in new[] { "'가'", "\"가\\\"나\"", "@\"가\"\"나\"", "$\"{string.Join(\",\", 값들)}\"" })
    {
        Assert(HanLexer.Tokenize(code).Count == 1, code);
        Assert(!HanLexer.IsSuppressed(code, code.Length), code);
    }
});
Test("입력 중 문자열/주석에서 완성 억제", () =>
{
    foreach (string code in new[] { "출력 \"가", "// 정수", "/* 정수", "\"\"\"\n정수" })
        Assert(CompletionCatalog.Suggest(code, code.Length).Count == 0, code);
    Assert(CompletionCatalog.Suggest("출력 \"가\" ", 7).Count > 0);
});
Test("한글·영문 이름과 매개변수 수집", () =>
{
    string code = "공개 클래스 Player:\n    비공개 필드 정수 _health는 100이다.\n    공개 속성 정수 Current는 _health이다.\n    공개 함수 TakeDamage(정수 amount)는 없음:\n        정수 남은체력은 _health - amount이다.\n    끝.\n끝.\n";
    var names = CompletionCatalog.Suggest(code, code.Length).Select(e => e.Display).ToArray();
    foreach (string name in new[] { "Player", "_health", "Current", "TakeDamage", "amount", "남은체력" }) Assert(names.Contains(name), name);
});
Test("문자열·주석의 가짜 선언 제외", () =>
{
    string code = "출력 \"정수 가짜변수는 0이다.\".\n// 정수 주석변수는 0이다.\n";
    var names = CompletionCatalog.Suggest(code, code.Length).Select(e => e.Display).ToArray();
    Assert(!names.Contains("가짜변수") && !names.Contains("주석변수"));
});
Test("Unity 멤버 문맥", () =>
{
    string code = "실행 Time.de";
    var names = CompletionCatalog.Suggest(code, code.Length).Select(e => e.Display).ToArray();
    Assert(names.Contains("deltaTime") && !names.Contains("클래스"));
    code = "실행 transform.";
    Assert(CompletionCatalog.Suggest(code, code.Length).Any(e => e.Display == "Rotate"));
});
Test("유니코드 단어 범위", () =>
{
    string code = "출력 _남은체력";
    Assert(CompletionCatalog.WordStart(code, code.Length) == 3);
    Assert(CompletionCatalog.WordStart("Time.de", 7) == 5);
});
Test("템플릿 들여쓰기와 선택 위치", () =>
{
    var entry = CompletionCatalog.Entries.First(e => e.Display == "클래스 틀");
    var insertion = entry.Prepare("    ");
    Assert(insertion.Text.Substring(insertion.SelectionStart, insertion.SelectionLength) == "PlayerState");
    Assert(insertion.Text.Contains(Environment.NewLine + "        비공개"));
    Assert(!insertion.Text.Contains('«') && !insertion.Text.Contains('»'));
});
Test("모든 문장 템플릿의 실제 한씨 변환/구문 검증", () =>
{
    foreach (var entry in CompletionCatalog.Entries.Where(e => e.IsTemplate))
    {
        string code = entry.Prepare("").Text;
        string generated = new KoreanToCSharp().Convert(code);
        if (new[] { "멤버 함수", "직렬화 필드", "유니티 Update", "유니티 코루틴" }.Contains(entry.Display))
            generated = "class Example { " + generated + " }";
        else if (!new[] { "클래스 틀", "콘솔 프로그램", "유니티 컴포넌트" }.Contains(entry.Display))
            generated = "class Example { void Test() { " + generated + " } }";
        try { new CSharpToKorean().Convert(generated); }
        catch (Exception error) { throw new Exception(entry.Display + ": " + error.Message); }
    }
});
Test("컴파일러 자료형 별칭과 완성 목록의 일치", () =>
{
    foreach (string type in new[] { "정수", "긴정수", "실수", "단정도실수", "십진수", "문자열", "문자", "논리", "객체", "자동", "없음", "벡터2", "벡터3", "회전", "색상", "코루틴" })
    {
        Assert(LanguageDefinition.ToCSharpType(type) != type);
        Assert(CompletionCatalog.Entries.Any(e => e.Display == type));
    }
});
Console.WriteLine($"편집 지원 검증: {passed} 통과 / {failed} 실패");
return failed == 0 ? 0 : 1;

void Test(string name, Action action)
{
    try { action(); passed++; Console.WriteLine("통과: " + name); }
    catch (Exception error) { failed++; Console.Error.WriteLine("실패: " + name + ": " + error.Message); }
}
static void Assert(bool value, string message = "조건 불일치")
{
    if (!value) throw new Exception(message);
}
