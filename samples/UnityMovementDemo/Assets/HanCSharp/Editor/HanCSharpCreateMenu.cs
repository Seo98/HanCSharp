using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

/// <summary>Project 창의 Create 메뉴에서 이름을 입력해 한씨 컴포넌트를 만든다.</summary>
public static class HanCSharpCreateMenu
{
    [MenuItem("Assets/Create/HanCSharp/한씨 컴포넌트", false, 81)]
    public static void CreateComponent()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<HanCSharpNameEditAction>(),
            "NewHanComponent.han", EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D, null);
    }

    /// <summary>Assets 아래에 새 원본만 만든다. 기존 .han 또는 같은 이름의 .cs는 덮어쓰지 않는다.</summary>
    public static UnityEngine.Object CreateAtPath(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string assets = Path.GetFullPath("Assets").TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(assets, StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path) != ".han")
            throw new ArgumentException("Assets 아래의 .han 경로를 지정하세요.", nameof(path));
        string name = Path.GetFileNameWithoutExtension(path);
        string content = HanCSharpScriptTemplates.Component(name);
        if (File.Exists(path) || File.Exists(Path.ChangeExtension(path, ".cs")))
            throw new IOException("같은 이름의 한씨 또는 C# 파일이 있습니다. 다른 이름을 사용하세요: " + path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, content, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(path);
        ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadMainAssetAtPath(path));
        return AssetDatabase.LoadMainAssetAtPath(path);
    }
}

/// <summary>Unity 이름 입력이 끝난 뒤 최종 파일명으로 한씨 클래스를 만든다.</summary>
public sealed class HanCSharpNameEditAction : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        try
        {
            HanCSharpCreateMenu.CreateAtPath(pathName);
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog("한씨 파일을 만들 수 없습니다", exception.Message, "확인");
        }
    }
}
