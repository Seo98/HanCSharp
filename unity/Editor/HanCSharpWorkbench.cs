using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>저장된 한씨와 변환 결과를 비교한다. 변환은 백그라운드에서 수행하고 닫힌 창에는 결과를 적용하지 않는다.</summary>
public sealed class HanCSharpWorkbench : EditorWindow
{
    [SerializeField] private string _source;
    private TextField _sourceText;
    private TextField _generatedText;
    private Label _status;
    private Button _refresh;
    private Button _error;
    private int _errorLine;
    private int _request;
    [SerializeField] private bool _autoRefresh = true;
    private readonly HanCSharpPreviewWatch _watch = new HanCSharpPreviewWatch();
    private double _nextPoll;
    private bool _converting;
    private bool _refreshQueued;
    private bool _active;

    [MenuItem("Tools/HanCSharp/변환 미리보기")]
    public static void OpenSelected()
    {
        Open(HanCSharpAssetPresentation.FindSource(Selection.activeObject));
    }

    public static void Open(string source)
    {
        bool existed = HasOpenInstances<HanCSharpWorkbench>();
        var window = GetWindow<HanCSharpWorkbench>("한씨 변환 미리보기");
        window.minSize = new Vector2(640f, 350f);
        if (!existed)
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            float width = Mathf.Min(1000f, main.width);
            float height = Mathf.Min(650f, main.height);
            window.position = new Rect(main.x + (main.width - width) * 0.5f, main.y + (main.height - height) * 0.5f, width, height);
        }
        if (source != null)
            window._source = source;
        window.Show();
        if (window._status != null)
            window.RefreshPreview();
    }

    private void OnEnable()
    {
        _active = true;
        Selection.selectionChanged += SelectionChanged;
        HanCSharpDiagnostics.Changed += ShowCompilationErrors;
        EditorApplication.update += PollSavedSource;
    }

    private void OnDisable()
    {
        _active = false;
        Selection.selectionChanged -= SelectionChanged;
        HanCSharpDiagnostics.Changed -= ShowCompilationErrors;
        EditorApplication.update -= PollSavedSource;
        _request++;
    }

    public void CreateGUI()
    {
        string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))).Replace('\\', '/');
        var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder + "/HanCSharpWorkbench.uxml");
        if (layout == null)
        {
            rootVisualElement.Add(new Label("미리보기 UXML/USS가 없습니다. 에디터 설치 파일을 확인하세요."));
            return;
        }
        layout.CloneTree(rootVisualElement);
        _status = rootVisualElement.Q<Label>("status");
        _refresh = rootVisualElement.Q<Button>("refresh");
        _error = rootVisualElement.Q<Button>("openError");
        var autoRefresh = rootVisualElement.Q<Toggle>("autoRefresh");
        autoRefresh.SetValueWithoutNotify(_autoRefresh);
        autoRefresh.RegisterValueChangedCallback(change => _autoRefresh = change.newValue);
        rootVisualElement.Q<Button>("safeUpdate").clicked += () =>
        {
            if (!EditorApplication.ExecuteMenuItem("Tools/HanCSharp/설치 및 안전한 업데이트"))
                _status.text = "설치 도구가 없습니다. 최신 HanCSharp.unitypackage를 먼저 가져오세요.";
        };
        _refresh.clicked += RefreshPreview;
        rootVisualElement.Q<Button>("openSource").clicked += () => OpenSource(1);
        _error.clicked += () => OpenSource(_errorLine);
        var split = new TwoPaneSplitView(0, 420, TwoPaneSplitViewOrientation.Horizontal);
        split.AddToClassList("han-split");
        split.Add(CreatePane("한씨 원본 .han — 저장된 내용", out _sourceText));
        split.Add(CreatePane("생성 C# — 읽기 전용 미리보기", out _generatedText));
        rootVisualElement.Q<VisualElement>("panes").Add(split);
        SelectionChanged();
    }

    private static VisualElement CreatePane(string title, out TextField text)
    {
        var pane = new VisualElement();
        pane.AddToClassList("han-pane");
        var heading = new Label(title);
        heading.AddToClassList("han-heading");
        pane.Add(heading);
        var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        scroll.AddToClassList("han-scroll");
        text = new TextField { multiline = true, isReadOnly = true };
        text.AddToClassList("han-code");
        scroll.Add(text);
        pane.Add(scroll);
        return pane;
    }

    private void SelectionChanged()
    {
        string selected = HanCSharpAssetPresentation.FindSource(Selection.activeObject);
        if (selected != null)
            _source = selected;
        if (_status != null)
            RefreshPreview();
    }

    private async void RefreshPreview()
    {
        if (_converting)
        {
            _refreshQueued = true;
            _request++;
            return;
        }
        int request = ++_request;
        _errorLine = 0;
        _error.SetEnabled(false);
        _refresh.SetEnabled(true);
        _sourceText.value = "";
        _generatedText.value = "";
        ShowCompilationErrors();
        rootVisualElement.Q<Label>("sourcePath").text = _source ?? "Project 창에서 .han 또는 생성 C#을 선택하세요.";
        if (string.IsNullOrEmpty(_source) || !File.Exists(_source))
        {
            _status.text = "저장된 .han 원본을 선택하세요.";
            return;
        }
        string source = _source;
        try { _watch.Observe(source, File.ReadAllText(source), EditorApplication.timeSinceStartup, true); }
        catch (IOException) { /* 변환기가 저장 상태를 다시 확인한다. */ }
        catch (System.UnauthorizedAccessException) { /* 변환 결과에 접근 오류를 표시한다. */ }
        var settings = HanCSharpSettings.LoadOrCreate();
        string dotNet = settings.DotNetPath;
        string compiler = settings.CompilerPath;
        _refresh.SetEnabled(false);
        _status.text = "변환 중…";
        _converting = true;
        HanCSharpPreview result;
        try
        {
            result = await Task.Run(() => HanCSharpPreview.Convert(source, dotNet, compiler));
        }
        catch (System.Exception exception)
        {
            if (this != null && _active && request == _request)
            {
                _status.text = exception.Message;
                _refresh.SetEnabled(true);
            }
            return;
        }
        finally
        {
            _converting = false;
            if (_refreshQueued && this != null && _active)
            {
                _refreshQueued = false;
                EditorApplication.delayCall += RefreshIfActive;
            }
        }
        if (this == null || !_active || request != _request)
            return;
        _refresh.SetEnabled(true);
        _sourceText.value = WithLineNumbers(result.SourceText);
        _generatedText.value = WithLineNumbers(result.GeneratedText);
        _status.text = result.Message;
        _errorLine = result.ErrorLine;
        _error.SetEnabled(_errorLine > 0);
        ShowCompilationErrors();
    }

    private void RefreshIfActive()
    {
        if (this != null && _active)
            RefreshPreview();
    }

    private void PollSavedSource()
    {
        if (!_active || !_autoRefresh || _status == null || string.IsNullOrEmpty(_source))
            return;
        double now = EditorApplication.timeSinceStartup;
        if (now < _nextPoll)
            return;
        _nextPoll = now + 0.25;
        try
        {
            string content = File.Exists(_source) ? File.ReadAllText(_source) : null;
            if (_watch.Observe(_source, content, now, _converting))
                RefreshPreview();
        }
        catch (IOException) { /* 편집기가 저장 중이면 다음 주기에 다시 읽는다. */ }
        catch (System.UnauthorizedAccessException) { /* 잠시 접근할 수 없는 원본은 재시도한다. */ }
    }

    private void ShowCompilationErrors()
    {
        var container = rootVisualElement.Q<VisualElement>("compilationErrors");
        if (container == null)
            return;
        container.Clear();
        foreach (HanCSharpDiagnostics.Entry error in HanCSharpDiagnostics.ForSource(_source))
        {
            var button = new Button(() => HanCSharpProjectIntegration.OpenSourceAtLine(error.Source, error.Line))
            {
                text = "한씨 " + error.Line + "행 열기 — " + error.Message,
                tooltip = error.Source + ":" + error.Line
            };
            button.AddToClassList("han-diagnostic");
            container.Add(button);
        }
    }

    private void OpenSource(int line)
    {
        if (!string.IsNullOrEmpty(_source) && File.Exists(_source))
            HanCSharpProjectIntegration.OpenSourceAtLine(_source, Mathf.Max(1, line));
    }

    private static string WithLineNumbers(string text)
    {
        var output = new StringBuilder();
        using (var reader = new StringReader(text))
        {
            string line;
            int number = 1;
            while ((line = reader.ReadLine()) != null)
                output.Append(number++).Append("  ").Append(line).Append('\n');
        }
        return output.ToString();
    }
}
