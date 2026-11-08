using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>한씨 변환기의 경로와 자동 생성 여부를 Inspector에서 조정하는 에디터 설정이다.</summary>
public sealed class HanCSharpSettings : ScriptableObject
{
    public const string AssetPath = "Assets/HanCSharp/Editor/HanCSharpSettings.asset";

    [SerializeField] private string _dotNetPath = "dotnet";
    [SerializeField] private string _compilerPath = "Tools/HanCSharp/HanCSharp.Cli.dll";
    [SerializeField] private bool _autoGenerate = true;
    // 켜면 C# 오류의 경로·행을 한씨 원본에 연결한다. 끄면 생성 C#을 간결하게 표시한다.
    [SerializeField] private bool _emitSourceMap = false;

    public string DotNetPath => _dotNetPath;
    public string CompilerPath => Path.GetFullPath(_compilerPath);
    public bool AutoGenerate => _autoGenerate;
    public bool EmitSourceMap => _emitSourceMap;

    public static HanCSharpSettings LoadOrCreate()
    {
        var settings = AssetDatabase.LoadAssetAtPath<HanCSharpSettings>(AssetPath);
        if (settings != null) return settings;
        Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
        AssetDatabase.Refresh();
        settings = CreateInstance<HanCSharpSettings>();
        AssetDatabase.CreateAsset(settings, AssetPath);
        AssetDatabase.SaveAssets();
        return settings;
    }
}
