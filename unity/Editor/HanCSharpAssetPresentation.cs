using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>한씨 자산의 식별 표시와 원본 접근만 담당한다. 기존 컴포넌트 Inspector는 대체하지 않는다.</summary>
[InitializeOnLoad]
public static class HanCSharpAssetPresentation
{
    static HanCSharpAssetPresentation()
    {
        Editor.finishedDefaultHeaderGUI += DrawHeader;
        EditorApplication.projectWindowItemOnGUI += DrawIcon;
    }

    public static string FindSource(UnityEngine.Object target)
    {
        if (target == null)
            return null;
        var component = target as MonoBehaviour;
        string path = AssetDatabase.GetAssetPath(component != null ? MonoScript.FromMonoBehaviour(component) : target);
        return HanCSharpProjectIntegration.ResolveHanSource(path);
    }

    private static void DrawHeader(Editor editor)
    {
        if (editor.targets.Length != 1)
            return;
        string source = FindSource(editor.target);
        if (source == null)
            return;
        GUILayout.Label("한씨 원본: " + Path.GetFileName(source), EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("한씨 원본 열기"))
            HanCSharpProjectIntegration.OpenSourceAtLine(source, 1);
        if (GUILayout.Button("변환 미리보기"))
            HanCSharpWorkbench.Open(source);
        GUILayout.EndHorizontal();
    }

    private static void DrawIcon(string guid, Rect rect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (!path.EndsWith(".han", StringComparison.OrdinalIgnoreCase))
            return;
        bool grid = rect.height > 20f;
        float size = grid ? Mathf.Min(48f, rect.width) : 16f;
        var icon = new Rect(grid ? rect.x + (rect.width - size) * 0.5f : rect.x, rect.y, size, size);
        EditorGUI.DrawRect(icon, new Color(0.05f, 0.43f, 0.48f));
        var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = grid ? 26 : 11 };
        style.normal.textColor = Color.white;
        GUI.Label(icon, "한", style);
    }

    [MenuItem("CONTEXT/MonoBehaviour/한씨 원본 열기")]
    private static void OpenComponent(MenuCommand command)
    {
        string source = FindSource(command.context);
        if (source != null)
            HanCSharpProjectIntegration.OpenSourceAtLine(source, 1);
    }

    [MenuItem("CONTEXT/MonoBehaviour/한씨 원본 열기", true)]
    private static bool CanOpenComponent(MenuCommand command)
    {
        return FindSource(command.context) != null;
    }
}
