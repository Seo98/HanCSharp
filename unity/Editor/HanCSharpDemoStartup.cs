using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>시작 도구가 요청한 데모에서만 이동 씬을 연다. 사용자가 저장하지 않은 씬은 교체하지 않는다.</summary>
public static class HanCSharpDemoStartup
{
    private const string Flag = "UserSettings/HanCSharp/OpenDemo.txt";

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        if (File.Exists(Flag)) EditorApplication.delayCall += OpenWhenReady;
    }

    private static void OpenWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += OpenWhenReady;
            return;
        }
        if (!File.Exists(Flag) || EditorApplication.isPlaying) return;
        const string path = "Assets/HanCSharp/Scenes/MovementDemo.unity";
        if (!File.Exists(path)) return;
        Scene active = SceneManager.GetActiveScene();
        if (active.path != path && active.isDirty)
        {
            Debug.LogWarning("한씨: 저장하지 않은 씬을 유지했습니다. 이동 예제 씬을 직접 열어주세요.");
            File.Delete(Flag);
            return;
        }
        if (active.path != path) EditorSceneManager.OpenScene(path);
        File.Delete(Flag);
    }
}
