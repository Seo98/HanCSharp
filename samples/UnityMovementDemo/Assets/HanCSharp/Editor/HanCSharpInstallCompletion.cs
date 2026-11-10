using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>설치 직후 새로 추가한 예제만 생성한다. 기존 예제·설정·사용자 생성 C#은 설치 도구가 교체하지 않는다.</summary>
public static class HanCSharpInstallCompletion
{
    private const string Flag = "Library/HanCSharp/installation.pending";

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        if (File.Exists(Flag)) EditorApplication.delayCall += Complete;
    }

    private static void Complete()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Complete;
            return;
        }
        if (!File.Exists(Flag)) return;
        string[] paths = File.ReadAllLines(Flag);
        File.Delete(Flag);
        var settings = HanCSharpSettings.LoadOrCreate();
        if (!settings.AutoGenerate) return;
        bool changed = false;
        foreach (string path in paths)
        {
            if (path.StartsWith("Assets/HanCSharp/Examples/", StringComparison.Ordinal)
                && path.EndsWith(".han", StringComparison.Ordinal) && File.Exists(path))
                changed |= HanCSharpGenerator.Generate(path, settings);
        }
        if (changed) AssetDatabase.Refresh();
    }
}
