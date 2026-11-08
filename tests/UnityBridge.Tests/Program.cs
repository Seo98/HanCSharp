using System.Reflection;
using UnityEditor;

if (args.Length != 1)
{
    Console.Error.WriteLine("인수: 비어 있는 검증용 작업 폴더");
    return 1;
}
string original = Directory.GetCurrentDirectory();
string compiler = Path.GetFullPath("app/HanCSharp.Cli.dll");
string root = Path.GetFullPath(args[0]);
Directory.CreateDirectory(root);
Directory.SetCurrentDirectory(root);
try
{
    Directory.CreateDirectory("Assets/Scripts With Spaces");
    var settings = HanCSharpSettings.LoadOrCreate();
    typeof(HanCSharpSettings).GetField("_compilerPath", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(settings, compiler);
    string han = "Assets/Scripts With Spaces/Sample.han";
    string cs = Path.ChangeExtension(han, ".cs");
    File.WriteAllText(han, "공개 클래스 Sample:\n끝.");
    Require(HanCSharpGenerator.Generate(han, settings), "첫 생성");
    Require(HanCSharpGenerator.Owns(cs, han), "생성 파일 소유권");
    Require(File.ReadAllText(cs).Contains("public class Sample"), "실제 변환기 호출");
    Require(!HanCSharpGenerator.Generate(han, settings), "변경 없는 파일 재작성 방지");
    Require(!File.ReadAllText(cs).Contains("#line"), "기본 생성 C#의 행 매핑 표시 생략");
    Require(HanCSharpProjectIntegration.ResolveHanSource(cs) == han, "소유한 생성 C#을 원본에 연결");
    typeof(HanCSharpSettings).GetField("_emitSourceMap", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(settings, true);
    Require(HanCSharpGenerator.Generate(han, settings) && File.ReadAllText(cs).Contains("#line"), "선택한 원본 행 매핑 보존");
    typeof(HanCSharpSettings).GetField("_emitSourceMap", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(settings, false);

    File.WriteAllText(han, "잘못된 문장");
    Require(HanCSharpGenerator.Generate(han, settings) && File.ReadAllText(cs).Contains("#error"), "변환 실패 시 이전 코드 사용 방지");
    File.WriteAllText(han, "공개 클래스 Sample:\n끝.");
    Require(HanCSharpGenerator.Generate(han, settings) && !File.ReadAllText(cs).Contains("#error"), "수정 후 복구");

    File.WriteAllText(cs + ".meta", "guid: keep-this-guid");
    string moved = "Assets/Scripts With Spaces/Moved.han";
    File.Move(han, moved);
    Postprocess([], [], [moved], [han]);
    EditorApplication.Flush();
    string movedCs = Path.ChangeExtension(moved, ".cs");
    Require(!File.Exists(cs) && HanCSharpGenerator.Owns(movedCs, moved), "이동 후 소유권");
    Require(File.ReadAllText(movedCs + ".meta") == "guid: keep-this-guid", "이동 시 meta 유지 (모형)");
    File.Delete(moved);
    Postprocess([], [moved], [], []);
    EditorApplication.Flush();
    Require(!File.Exists(movedCs), "소스 삭제 시 소유한 파일 정리 (모형)");

    File.WriteAllText(han, "공개 클래스 Sample:\n끝.");
    File.WriteAllText(cs, "// 사용자가 직접 작성한 C#");
    Require(!HanCSharpGenerator.Generate(han, settings), "사용자 작성 C# 보호");
    Require(File.ReadAllText(cs) == "// 사용자가 직접 작성한 C#", "사용자 파일 내용 유지");
    Require(HanCSharpProjectIntegration.ResolveHanSource(cs) == null, "사용자 C# 열기는 가로채지 않음");
    HanCSharpProjectIntegration.EnableHanFiles();
    HanCSharpProjectIntegration.EnableHanFiles();
    Require(EditorSettings.projectGenerationUserExtensions.SequenceEqual(new[] { "txt", "han" }), "프로젝트 확장자 추가와 기존 설정 유지");
    string created = "Assets/Scripts With Spaces/이동Component.han";
    HanCSharpCreateMenu.CreateAtPath(created);
    Require(File.ReadAllText(created).Contains("클래스 이동Component는 MonoBehaviour"), "새 한씨 파일명과 클래스명 일치");
    bool refused = false;
    try { HanCSharpCreateMenu.CreateAtPath(created); }
    catch (IOException) { refused = true; }
    Require(refused, "새 파일 생성 시 기존 원본 보호");
    Require(!HanCSharpScriptTemplates.IsValidName("Bad Name") && !HanCSharpScriptTemplates.IsValidName("1Player")
        && !HanCSharpScriptTemplates.IsValidName("class") && HanCSharpScriptTemplates.IsValidName("한글_Player2"), "새 클래스 이름 검사");
    File.WriteAllText("Assets/Scripts With Spaces/Existing.cs", "// user");
    refused = false;
    try { HanCSharpCreateMenu.CreateAtPath("Assets/Scripts With Spaces/Existing.han"); }
    catch (IOException) { refused = true; }
    Require(refused && File.ReadAllText("Assets/Scripts With Spaces/Existing.cs") == "// user", "동일 이름 사용자 C# 보호");
    Console.WriteLine("Unity 연결 모형 검증: 20개 확인 통과. 실제 Unity 실행 증거는 아닙니다.");
    return 0;
}
finally
{
    Directory.SetCurrentDirectory(original);
}

static void Require(bool result, string name)
{
    if (!result) throw new Exception("실패: " + name);
    Console.WriteLine("통과: " + name);
}

static void Postprocess(string[] imported, string[] deleted, string[] moved, string[] previous)
{
    typeof(HanCSharpAssetPostprocessor).GetMethod("OnPostprocessAllAssets", BindingFlags.NonPublic | BindingFlags.Static)!
        .Invoke(null, [imported, deleted, moved, previous]);
}
