using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>.han의 가져오기·이동·삭제를 생성 C#에 반영한다. 실제 처리는 import 콜백 뒤로 미룬다.</summary>
public sealed class HanCSharpAssetPostprocessor : AssetPostprocessor
{
    private static readonly HashSet<string> _pending = new HashSet<string>();
    private static readonly List<Action> _fileChanges = new List<Action>();
    private static bool _scheduled;

    private static bool IsHan(string path) => path.StartsWith("Assets/", StringComparison.Ordinal)
        && path.EndsWith(".han", StringComparison.OrdinalIgnoreCase);

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets) if (IsHan(path)) _pending.Add(path);
        foreach (string path in deletedAssets)
        {
            if (!IsHan(path)) continue;
            string deleted = path;
            _fileChanges.Add(() =>
            {
                string target = Path.ChangeExtension(deleted, ".cs");
                if (HanCSharpGenerator.Owns(target, deleted)) AssetDatabase.DeleteAsset(target);
            });
        }
        for (int index = 0; index < movedAssets.Length; index++)
        {
            string source = movedFromAssetPaths[index];
            string destination = movedAssets[index];
            if (!IsHan(destination)) continue;
            _pending.Add(destination);
            _fileChanges.Add(() => MoveGenerated(source, destination));
        }
        if (_pending.Count > 0 || _fileChanges.Count > 0) Schedule();
    }

    private static void MoveGenerated(string source, string destination)
    {
        string previous = Path.ChangeExtension(source, ".cs");
        string next = Path.ChangeExtension(destination, ".cs");
        if (!HanCSharpGenerator.Owns(previous, source) || File.Exists(next)) return;
        string error = AssetDatabase.MoveAsset(previous, next);
        if (string.IsNullOrEmpty(error)) HanCSharpGenerator.UpdateOwnership(next, source, destination);
        else Debug.LogError("한씨: 생성 파일 이동 실패: " + error);
    }

    private static void Schedule()
    {
        if (_scheduled) return;
        _scheduled = true;
        EditorApplication.delayCall += Flush;
    }

    private static void Flush()
    {
        _scheduled = false;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Schedule();
            return;
        }
        var settings = HanCSharpSettings.LoadOrCreate();
        if (!settings.AutoGenerate)
        {
            _pending.Clear();
            _fileChanges.Clear();
            return;
        }
        Action[] changes = _fileChanges.ToArray();
        _fileChanges.Clear();
        foreach (Action change in changes) change();
        string[] pending = new List<string>(_pending).ToArray();
        _pending.Clear();
        bool changed = changes.Length > 0;
        foreach (string path in pending)
        {
            if (File.Exists(path)) changed |= HanCSharpGenerator.Generate(path, settings);
        }
        if (changed) AssetDatabase.Refresh();
    }

    [MenuItem("Tools/HanCSharp/Settings")]
    private static void SelectSettings() => Selection.activeObject = HanCSharpSettings.LoadOrCreate();

    [MenuItem("Tools/HanCSharp/Generate All")]
    private static void GenerateAll()
    {
        var settings = HanCSharpSettings.LoadOrCreate();
        bool changed = false;
        foreach (string file in Directory.EnumerateFiles("Assets", "*.han", SearchOption.AllDirectories))
        {
            changed |= HanCSharpGenerator.Generate(file.Replace('\\', '/'), settings);
        }
        if (changed) AssetDatabase.Refresh();
    }
}
