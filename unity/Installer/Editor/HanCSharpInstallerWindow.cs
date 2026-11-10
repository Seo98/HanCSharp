using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>파일 검사 결과를 먼저 보여주고 명시적인 적용 버튼으로만 설치한다. 네트워크·설치는 자동 시작하지 않는다.</summary>
public sealed class HanCSharpInstallerWindow : EditorWindow
{
    private Label _status;
    private TextField _details;
    private Button _apply;
    private VisualElement _actions;
    private Toggle _examples;
    private HanCSharpUpdatePlan _plan;
    private string _backup;
    private bool _busy;

    [MenuItem("Tools/HanCSharp/설치 및 안전한 업데이트")]
    public static void Open() { GetWindow<HanCSharpInstallerWindow>("한씨 설치·업데이트").Show(); }

    public void CreateGUI()
    {
        minSize = new Vector2(680f, 460f);
        string editorFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))).Replace('\\', '/');
        var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(editorFolder + "/HanCSharpInstaller.uss");
        if (styles != null) rootVisualElement.styleSheets.Add(styles);
        rootVisualElement.AddToClassList("han-install");
        rootVisualElement.Add(new Label("한씨 — 설치 전 검사 · 교체 파일 백업 · 사용자 변경 보호"));
        _status = new Label("먼저 .NET 10 Runtime을 설치하세요. 패키지 검사 후 적용할 파일을 확인합니다.");
        _status.name = "installStatus";
        _status.AddToClassList("han-install-status");
        rootVisualElement.Add(_status);
        _actions = new VisualElement();
        _actions.AddToClassList("han-install-actions");
        AddButton("패키지 파일 검사", async () => await LoadArchive(BundledArchive()));
        AddButton("최신 버전 확인·받기", DownloadLatest);
        AddButton("배포 ZIP 직접 선택", SelectArchive);
        AddButton(".NET 설치 안내", () => { Application.OpenURL("https://dotnet.microsoft.com/en-us/download/dotnet/10.0"); return Task.CompletedTask; });
        AddButton("설치 가이드", () => { Application.OpenURL("https://github.com/Seo98/HanCSharp/blob/main/docs/UNITYPACKAGE-INSTALL.md"); return Task.CompletedTask; });
        AddButton("백업 폴더 열기", () => { if (!string.IsNullOrEmpty(_backup)) EditorUtility.RevealInFinder(_backup); return Task.CompletedTask; });
        rootVisualElement.Add(_actions);
        _examples = new Toggle("새 예제 원본도 설치 — 기존 예제는 항상 보존") { value = true };
        _examples.RegisterValueChangedCallback(change => { _plan = null; _apply.SetEnabled(false); _status.text = "선택이 변경됐습니다. 파일을 다시 검사하세요."; });
        rootVisualElement.Add(_examples);
        _apply = new Button(async () => await Apply()) { text = "검사한 파일을 백업 후 적용", name = "applyUpdate" };
        _apply.SetEnabled(false);
        rootVisualElement.Add(_apply);
        _details = new TextField { multiline = true, isReadOnly = true, name = "updateDetails" };
        _details.AddToClassList("han-install-details");
        rootVisualElement.Add(_details);
    }

    private void AddButton(string title, Func<Task> action)
    {
        _actions.Add(new Button(async () => await Guard(action)) { text = title });
    }

    private async Task Guard(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        _actions.SetEnabled(false);
        _apply.SetEnabled(false);
        _examples.SetEnabled(false);
        try { await action(); }
        catch (Exception exception) { _status.text = "중단: " + exception.Message; _plan = null; }
        finally
        {
            if (this != null)
            {
                _busy = false;
                _actions.SetEnabled(true);
                _examples.SetEnabled(true);
                _apply.SetEnabled(_plan != null && _plan.CanApply);
            }
        }
    }

    public async Task LoadArchive(string archive)
    {
        _plan = null;
        _status.text = "압축·해시·.NET 실행 환경을 검사 중…";
        string project = ProjectRoot();
        string stage = Path.Combine(project, "Library/HanCSharp/Updates", Guid.NewGuid().ToString("N"));
        await Task.Run(() => HanCSharpUpdateEngine.Extract(archive, stage));
        var manifest = JsonUtility.FromJson<HanCSharpUpdateManifest>(File.ReadAllText(Path.Combine(stage, "manifest.json")));
        if (!_examples.value) manifest.files = manifest.files.Where(file => !file.path.StartsWith("Assets/HanCSharp/Examples/", StringComparison.Ordinal)).ToArray();
        var baselines = JsonUtility.FromJson<HanCSharpBaselineCatalog>(File.ReadAllText(Path.Combine(stage, "baselines.json")));
        string state = Path.Combine(project, HanCSharpUpdateEngine.StateRelativePath);
        var installed = File.Exists(state) ? JsonUtility.FromJson<HanCSharpUpdateManifest>(File.ReadAllText(state)) : null;
        var plan = await Task.Run(() => HanCSharpUpdateEngine.Plan(project, stage, manifest, installed, baselines));
        try { await Task.Run(() => HanCSharpInstallRuntime.Check(Path.Combine(stage, "Tools/HanCSharp/HanCSharp.Cli.dll"))); }
        catch (Exception exception) { throw new IOException(".NET 10 Runtime 설치 후 Unity를 다시 열고 검사하세요. " + exception.Message, exception); }
        _plan = plan;
        var text = new StringBuilder();
        foreach (var change in plan.Changes)
            text.Append(ActionText(change.Action)).Append("  ").Append(change.File.path).Append('\n');
        _details.value = text.ToString();
        _status.text = "검사 완료 v" + plan.Incoming.version + (plan.CanApply ? " — 변경 내용을 확인한 뒤 적용하세요." : " — 사용자 변경/알 수 없는 파일이 있어 전체 적용을 차단했습니다.");
        _apply.SetEnabled(plan.CanApply);
    }

    private Task SelectArchive()
    {
        string selected = EditorUtility.OpenFilePanel("공식 HanCSharp-UnityPayload.zip 선택", "", "zip");
        return string.IsNullOrEmpty(selected) ? Task.CompletedTask : LoadArchive(selected);
    }

    private async Task DownloadLatest()
    {
        _plan = null;
        _status.text = "공식 GitHub 최신 배포와 첨부 해시를 확인 중…";
        string folder = Path.Combine(ProjectRoot(), "Library/HanCSharp/Downloads", Guid.NewGuid().ToString("N"));
        string archive = await HanCSharpReleaseDownload.Latest(folder);
        await LoadArchive(archive);
    }

    private async Task Apply()
    {
        await Guard(async () =>
        {
            if (_plan == null || !_plan.CanApply) return;
            var plan = _plan;
            _status.text = "파일을 재검사하고 백업 후 적용 중…";
            _backup = await Task.Run(() => HanCSharpUpdateEngine.Apply(plan, manifest => JsonUtility.ToJson(manifest, true)));
            string[] examples = plan.Changes.Where(change => change.Action == HanCSharpUpdateAction.Add
                && change.File.path.EndsWith(".han", StringComparison.Ordinal)).Select(change => change.File.path).ToArray();
            string flag = Path.Combine(ProjectRoot(), "Library/HanCSharp/installation.pending");
            if (examples.Length > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(flag));
                File.WriteAllLines(flag, examples);
            }
            _plan = null;
            _status.text = "v" + plan.Incoming.version + " 적용 완료. Unity 컴파일을 기다리세요. 백업: " + _backup;
            AssetDatabase.Refresh();
        });
    }

    private string BundledArchive()
    {
        string script = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(script), "..", "HanCSharpPayload.bytes"));
    }

    private static string ProjectRoot() => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    private static string ActionText(HanCSharpUpdateAction action)
    {
        switch (action)
        {
            case HanCSharpUpdateAction.Add: return "추가";
            case HanCSharpUpdateAction.Replace: return "백업·교체";
            case HanCSharpUpdateAction.Keep: return "보존";
            default: return "충돌";
        }
    }
}
