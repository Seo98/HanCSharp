using System.IO.Compression;
using System.Text.Json;

string root = Path.GetFullPath(args[0]);
Directory.CreateDirectory(root);
int passed = 0;
int count = 0;
var json = new JsonSerializerOptions { IncludeFields = true };
string Serialize(HanCSharpUpdateManifest value) => JsonSerializer.Serialize(value, json);
Fixture New()
{
    string folder = Path.Combine(root, "case-" + ++count + "-" + Guid.NewGuid().ToString("N"));
    return new Fixture(folder);
}
void Test(string name, Action action)
{
    action(); passed++; Console.WriteLine("통과: " + name);
}
void Assert(bool value) { if (!value) throw new Exception("검증 실패"); }
bool Reject(Action action) { try { action(); return false; } catch (IOException) { return true; } }

Test("신규 설치와 설치 상태 기록", () =>
{
    var f = New(); f.Add("Tools/HanCSharp/Test.dll", "new");
    HanCSharpUpdateEngine.Apply(f.Plan(), Serialize);
    Assert(File.ReadAllText(f.Target("Tools/HanCSharp/Test.dll")) == "new" && File.Exists(f.State));
});
Test("정상 업데이트의 교체 전 백업", () =>
{
    var f = New(); const string path = "Assets/HanCSharp/Editor/Tool.cs";
    f.Add(path, "old"); HanCSharpUpdateEngine.Apply(f.Plan(), Serialize);
    var installed = JsonSerializer.Deserialize<HanCSharpUpdateManifest>(File.ReadAllText(f.State), json);
    f.Add(path, "new"); f.Manifest.version = "0.8.1";
    string backup = HanCSharpUpdateEngine.Apply(f.Plan(installed), Serialize);
    Assert(File.ReadAllText(f.Target(path)) == "new" && File.ReadAllText(Path.Combine(backup, path)) == "old");
});
Test("사용자 도구 변경 시 전체 적용 차단", () =>
{
    var f = New(); const string path = "Assets/HanCSharp/Editor/Tool.cs";
    f.Add(path, "old"); HanCSharpUpdateEngine.Apply(f.Plan(), Serialize);
    var installed = JsonSerializer.Deserialize<HanCSharpUpdateManifest>(File.ReadAllText(f.State), json);
    File.WriteAllText(f.Target(path), "user"); f.Add(path, "next"); f.Add("Tools/HanCSharp/Added.dll", "next");
    var plan = f.Plan(installed);
    Assert(!plan.CanApply && Reject(() => HanCSharpUpdateEngine.Apply(plan, Serialize))
        && File.ReadAllText(f.Target(path)) == "user" && !File.Exists(f.Target("Tools/HanCSharp/Added.dll")));
});
Test("기존 예제와 meta는 수정 여부와 관계없이 보존", () =>
{
    var f = New(); const string han = "Assets/HanCSharp/Examples/User.han"; const string meta = "Assets/HanCSharp/Editor/Tool.cs.meta";
    f.Add(han, "release"); f.Add(meta, "guid: release");
    Directory.CreateDirectory(Path.GetDirectoryName(f.Target(han))); Directory.CreateDirectory(Path.GetDirectoryName(f.Target(meta)));
    File.WriteAllText(f.Target(han), "user source"); File.WriteAllText(f.Target(meta), "guid: user");
    HanCSharpUpdateEngine.Apply(f.Plan(), Serialize);
    Assert(File.ReadAllText(f.Target(han)) == "user source" && File.ReadAllText(f.Target(meta)) == "guid: user");
});
Test("이전 공개 버전 해시로만 레거시 파일 교체", () =>
{
    var f = New(); const string path = "Assets/HanCSharp/Editor/Tool.cs";
    f.Add(path, "new"); Directory.CreateDirectory(Path.GetDirectoryName(f.Target(path))); File.WriteAllText(f.Target(path), "known release");
    var known = new HanCSharpBaselineCatalog { entries = new[] { new HanCSharpKnownFile { path = path, hashes = new[] { HanCSharpUpdateEngine.Hash(f.Target(path)) } } } };
    Assert(f.Plan().CanApply == false);
    var plan = f.Plan(null, known); Assert(plan.CanApply);
    HanCSharpUpdateEngine.Apply(plan, Serialize); Assert(File.ReadAllText(f.Target(path)) == "new");
});
Test("검사 후 사용자 저장은 적용 직전 거부", () =>
{
    var f = New(); const string path = "Tools/HanCSharp/Test.dll"; f.Add(path, "release"); var plan = f.Plan();
    Directory.CreateDirectory(Path.GetDirectoryName(f.Target(path))); File.WriteAllText(f.Target(path), "user changed");
    Assert(Reject(() => HanCSharpUpdateEngine.Apply(plan, Serialize)) && File.ReadAllText(f.Target(path)) == "user changed");
});
Test("스테이징 변조와 해시 불일치 거부", () =>
{
    var f = New(); const string path = "Tools/HanCSharp/Test.dll"; f.Add(path, "release"); var plan = f.Plan();
    File.WriteAllText(Path.Combine(f.Stage, path), "changed");
    Assert(Reject(() => HanCSharpUpdateEngine.Apply(plan, Serialize)) && Reject(() => f.Plan()));
});
Test("상태 저장 실패 시 교체·추가 파일 롤백", () =>
{
    var f = New(); const string path = "Tools/HanCSharp/Test.dll";
    f.Add(path, "old"); HanCSharpUpdateEngine.Apply(f.Plan(), Serialize);
    string oldState = File.ReadAllText(f.State); var installed = JsonSerializer.Deserialize<HanCSharpUpdateManifest>(oldState, json);
    f.Add(path, "new"); f.Add("Assets/HanCSharp/Editor/Added.cs", "new");
    Assert(Reject(() => HanCSharpUpdateEngine.Apply(f.Plan(installed), value => throw new IOException("state write failed"))));
    Assert(File.ReadAllText(f.Target(path)) == "old" && !File.Exists(f.Target("Assets/HanCSharp/Editor/Added.cs")) && File.ReadAllText(f.State) == oldState);
});
Test("경로 이탈·설정·EXE·관리 영역 밖 거부", () =>
{
    foreach (string path in new[] { "../escape", "/root", "C:/outside", "Assets/HanCSharp/Editor/../escape", "Assets/Other/a.cs", "Tools/HanCSharp/run.exe", "Assets/HanCSharp/Editor/HanCSharpSettings.asset", "Tools/HanCSharp/a:stream" })
        Assert(Reject(() => HanCSharpUpdateEngine.Normalize(path)));
});
Test("ZIP 경로 이탈과 중복 항목 거부", () =>
{
    var f = New(); string zip = Path.Combine(f.Root, "bad.zip");
    using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create)) { using var writer = new StreamWriter(archive.CreateEntry("../escaped").Open()); writer.Write("bad"); }
    Assert(Reject(() => HanCSharpUpdateEngine.Extract(zip, Path.Combine(f.Root, "extract"))) && !File.Exists(Path.Combine(f.Root, "escaped")));
    string duplicate = Path.Combine(f.Root, "duplicates.zip");
    using (var archive = ZipFile.Open(duplicate, ZipArchiveMode.Create)) { archive.CreateEntry("manifest.json"); archive.CreateEntry("manifest.json"); }
    Assert(Reject(() => HanCSharpUpdateEngine.Extract(duplicate, Path.Combine(f.Root, "extract2"))));
});
Test("중복 배포 목록과 다운그레이드 거부", () =>
{
    var f = New(); f.Add("Tools/HanCSharp/Test.dll", "x"); var duplicate = f.Manifest.files[0];
    f.Manifest.files = new[] { duplicate, duplicate }; Assert(Reject(() => f.Plan()));
    f.Manifest.files = new[] { duplicate }; Assert(Reject(() => f.Plan(new HanCSharpUpdateManifest { version = "9.0.0" })));
});
Test("파일 대신 폴더가 있으면 충돌", () =>
{
    var f = New(); const string path = "Tools/HanCSharp/Test.dll"; f.Add(path, "x"); Directory.CreateDirectory(f.Target(path));
    Assert(!f.Plan().CanApply);
});
Test("검사 후 설치 상태가 바뀌면 거부", () =>
{
    var f = New(); f.Add("Tools/HanCSharp/Test.dll", "x"); var plan = f.Plan(); File.WriteAllText(f.State, "user state");
    Assert(Reject(() => HanCSharpUpdateEngine.Apply(plan, Serialize)) && File.ReadAllText(f.State) == "user state");
});
Test("저장 연속 이벤트를 한 번으로 합침", () =>
{
    var watch = new HanCSharpPreviewWatch(); Assert(!watch.Observe("a", "old", 0, false));
    Assert(!watch.Observe("a", "new", 0.1, false)); Assert(!watch.Observe("a", "newer", 0.2, false));
    Assert(!watch.Observe("a", "newer", 0.5, false)); Assert(watch.Observe("a", "newer", 0.6, false)); Assert(!watch.Observe("a", "newer", 0.8, false));
});
Test("변환 중 저장은 완료 후 한 번 갱신", () =>
{
    var watch = new HanCSharpPreviewWatch(); watch.Observe("a", "old", 0, false); watch.Observe("a", "new", 0.1, true);
    Assert(!watch.Observe("a", "new", 1, true)); Assert(watch.Observe("a", "new", 1.1, false)); Assert(!watch.Observe("a", "new", 1.2, false));
});
Test("선택 원본 변경과 삭제 상태 감지", () =>
{
    var watch = new HanCSharpPreviewWatch(); watch.Observe("a", "old", 0, false); watch.Observe("a", "new", 0.1, false);
    Assert(!watch.Observe("b", "other", 0.2, false)); Assert(!watch.Observe("b", null, 0.3, false)); Assert(watch.Observe("b", null, 1, false));
});
Console.WriteLine("업데이트·저장 감지 검증: " + passed + "개 통과");

sealed class Fixture
{
    public string Root, Project, Stage;
    public HanCSharpUpdateManifest Manifest = new() { version = "0.8.0" };
    public string State => Path.Combine(Project, HanCSharpUpdateEngine.StateRelativePath);
    public Fixture(string root)
    {
        Root = root; Project = Path.Combine(root, "project"); Stage = Path.Combine(root, "stage");
        Directory.CreateDirectory(Path.Combine(Project, "Assets")); Directory.CreateDirectory(Path.Combine(Project, "ProjectSettings")); Directory.CreateDirectory(Stage);
    }
    public string Target(string relative) => Path.Combine(Project, relative);
    public void Add(string path, string content)
    {
        string file = Path.Combine(Stage, path); Directory.CreateDirectory(Path.GetDirectoryName(file)); File.WriteAllText(file, content);
        var files = Manifest.files.Where(entry => entry.path != path).ToList(); files.Add(new HanCSharpUpdateFile { path = path, sha256 = HanCSharpUpdateEngine.Hash(file) }); Manifest.files = files.ToArray();
    }
    public HanCSharpUpdatePlan Plan(HanCSharpUpdateManifest installed = null, HanCSharpBaselineCatalog known = null) => HanCSharpUpdateEngine.Plan(Project, Stage, Manifest, installed, known);
}
