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

    [OnOpenAsset(-100)]
    public static bool OpenHan(int instanceId, int line)
    {
#if UNITY_6000_3_OR_NEWER
        string path = AssetDatabase.GetAssetPath((EntityId)instanceId);
#else
        string path = AssetDatabase.GetAssetPath(instanceId);
#endif
        string source = ResolveHanSource(path);
        if (source == null)
            return false;
        int sourceLine = path.EndsWith(".han", StringComparison.OrdinalIgnoreCase) ? Math.Max(line, 1) : 1;
        if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && line > 1
            && !HanCSharpSourceMap.TryResolve(path, line, source, out sourceLine))
            return false;
        OpenSourceAtLine(source, sourceLine);
        return true;
    }

    /// <summary>확장자를 먼저 등록해 Visual Studio 연결이 기본 앱 열기로 돌아가 행 번호를 잃지 않도록 한다.</summary>
    public static void OpenSourceAtLine(string source, int line)
    {
        if (string.IsNullOrEmpty(source) || !File.Exists(source))
            return;
        EnableHanFiles();
        string fullPath = Path.GetFullPath(source);
        int sourceLine = Math.Max(1, line);
        if (!Unity.CodeEditor.CodeEditor.CurrentEditor.OpenProject(fullPath, sourceLine, 1))
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath, sourceLine);
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
