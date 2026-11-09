using System;
using System.IO;
using System.IO.Compression;
using HanCSharp.Launcher;

if (args.Length != 1) throw new ArgumentException("짧고 비어 있는 검증 폴더를 지정하세요.");
string folder = Path.GetFullPath(args[0]);
Directory.CreateDirectory(folder);
string root = Path.Combine(folder, "pkg");
Directory.CreateDirectory(Path.Combine(root, "templates"));
Directory.CreateDirectory(Path.Combine(root, "app"));
File.WriteAllText(Path.Combine(root, "VERSION"), "0.5.0");
File.WriteAllText(Path.Combine(root, "app", "coreclr.dll"), "runtime fixture");
using (var zip = ZipFile.Open(Path.Combine(root, "templates", "UnityMovementDemo.zip"), ZipArchiveMode.Create))
{
    Write(zip, "ProjectSettings/ProjectVersion.txt", "m_EditorVersion: 6000.3.25f1");
    Write(zip, "Assets/Demo.han", "original");
}
string project = PackageOperations.PrepareDemo(root, Path.Combine(folder, "demo"));
Assert(File.Exists(Path.Combine(project, "Tools/HanCSharp/coreclr.dll")), "런타임이 짧은 예제 경로에 복사됨");
Assert(File.Exists(Path.Combine(project, "UserSettings/HanCSharp/OpenDemo.txt")), "이동 씬 열기 요청");
File.WriteAllText(Path.Combine(project, "Assets/Demo.han"), "user edit");
PackageOperations.PrepareDemo(root, project);
Assert(File.ReadAllText(Path.Combine(project, "Assets/Demo.han")) == "user edit", "다시 준비할 때 사용자 수정 보존");
string occupied = Path.Combine(folder, "occupied");
Directory.CreateDirectory(occupied);
File.WriteAllText(Path.Combine(occupied, "user.txt"), "keep");
Assert(Throws(() => PackageOperations.PrepareDemo(root, occupied)) && File.ReadAllText(Path.Combine(occupied, "user.txt")) == "keep", "다른 사용자 폴더 보호");
Assert(Throws(() => PackageOperations.PrepareDemo(root, Path.Combine(folder, new string('x', 90)))), "긴 Unity 대상 경로 거부");
string install = Path.Combine(folder, "install");
Directory.CreateDirectory(Path.Combine(install, "Assets"));
Directory.CreateDirectory(Path.Combine(install, "ProjectSettings"));
Directory.CreateDirectory(Path.Combine(root, "unity/Editor"));
Directory.CreateDirectory(Path.Combine(root, "samples/UnityScripts"));
File.WriteAllText(Path.Combine(root, "unity/Editor/Bridge.cs"), "bridge");
File.WriteAllText(Path.Combine(root, "unity/Editor/Preview.uxml"), "layout");
File.WriteAllText(Path.Combine(root, "unity/Editor/Preview.uss"), "styles");
File.WriteAllText(Path.Combine(root, "samples/UnityScripts/Test.han"), "sample");
PackageOperations.InstallUnity(root, install, true);
PackageOperations.InstallUnity(root, install, true);
Assert(File.ReadAllText(Path.Combine(install, "Assets/HanCSharp/Examples/Test.han")) == "sample", "신규 설치와 동일 버전 재설치");
Assert(File.ReadAllText(Path.Combine(install, "Assets/HanCSharp/Editor/Preview.uxml")) == "layout"
    && File.ReadAllText(Path.Combine(install, "Assets/HanCSharp/Editor/Preview.uss")) == "styles", "미리보기 UI 리소스 설치");
File.WriteAllText(Path.Combine(install, "Assets/HanCSharp/Editor/Bridge.cs"), "user code");
Assert(Throws(() => PackageOperations.InstallUnity(root, install, true)) && File.ReadAllText(Path.Combine(install, "Assets/HanCSharp/Editor/Bridge.cs")) == "user code", "사용자 코드 덮어쓰기 거부");
Assert(Throws(() => PackageOperations.InstallUnity(root, Path.Combine(install, "Assets"), true)), "Assets 자체를 프로젝트로 허용하지 않음");
string bad = Path.Combine(folder, "bad");
Directory.CreateDirectory(Path.Combine(bad, "templates"));
File.WriteAllText(Path.Combine(bad, "VERSION"), "0.5.0");
using (var zip = ZipFile.Open(Path.Combine(bad, "templates/UnityMovementDemo.zip"), ZipArchiveMode.Create)) Write(zip, "../escaped.txt", "bad");
Assert(Throws(() => PackageOperations.PrepareDemo(bad, Path.Combine(folder, "badout"))) && !File.Exists(Path.Combine(folder, "escaped.txt")), "압축 파일의 외부 경로 차단");
Console.WriteLine("설치·데모 준비 검증: 10개 통과");

static void Write(ZipArchive zip, string path, string content)
{
    using (var writer = new StreamWriter(zip.CreateEntry(path).Open())) writer.Write(content);
}
static bool Throws(Action action)
{
    try { action(); return false; } catch (IOException) { return true; }
}
static void Assert(bool value, string description)
{
    if (!value) throw new Exception(description);
    Console.WriteLine("통과: " + description);
}
