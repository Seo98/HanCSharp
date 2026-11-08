using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>Unity가 생성하는 Visual Studio 솔루션에 .han을 포함하고 외부 편집기로 연다.</summary>
public static class HanCSharpProjectIntegration
{
    [MenuItem("Tools/HanCSharp/Include Han Files In Solution")]
    public static void EnableHanFiles()
    {
        var extensions = new List<string>(EditorSettings.projectGenerationUserExtensions ?? new string[0]);
        foreach (string extension in extensions)
        {
            if (extension.TrimStart('.').Equals("han", StringComparison.OrdinalIgnoreCase)) return;
        }
        extensions.Add("han");
        EditorSettings.projectGenerationUserExtensions = extensions.ToArray();
        Debug.Log("한씨: Assets > Open C# Project로 솔루션을 다시 생성하면 .han이 포함됩니다.");
    }

    [OnOpenAsset]
    public static bool OpenHan(int instanceId, int line)
    {
        string path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceId));
        // C# 진단의 구체적인 행으로 이동할 때는 해당 C# 위치를 유지한다.
        if (!string.IsNullOrEmpty(path) && path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && line > 1)
            return false;
        string source = ResolveHanSource(path);
        if (source == null)
            return false;
        int sourceLine = path.EndsWith(".han", StringComparison.OrdinalIgnoreCase) ? Math.Max(line, 1) : 1;
        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(source, sourceLine);
        return true;
    }

    /// <summary>원본 또는 소유권 표식이 일치하는 생성 C#만 한씨 원본으로 연결한다. 일반 C#은 가로채지 않는다.</summary>
    public static string ResolveHanSource(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        if (path.EndsWith(".han", StringComparison.OrdinalIgnoreCase))
            return File.Exists(path) ? path : null;
        if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            return null;
        string source = Path.ChangeExtension(path, ".han");
        return File.Exists(source) && HanCSharpGenerator.Owns(path, source) ? source : null;
    }
}
