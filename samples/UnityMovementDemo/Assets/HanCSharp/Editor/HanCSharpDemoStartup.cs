using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>요청한 데모의 첫 가져오기가 끝나면 이동 씬을 연다. 미저장 씬은 교체하지 않는다.</summary>
public static class HanCSharpDemoStartup
{
    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    private static string Flag => Path.Combine(ProjectRoot, "UserSettings/HanCSharp/OpenDemo.txt");

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess())
            return;
        EditorApplication.update -= OpenWhenReady;
        EditorApplication.update += OpenWhenReady;
    }

    private static void OpenWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying)
            return;
        if (!File.Exists(Flag))
        {
            EditorApplication.update -= OpenWhenReady;
            return;
        }
        const string path = "Assets/HanCSharp/Scenes/MovementDemo.unity";
        // 최초 도메인 초기화 때는 씬의 에셋 등록이 아직 끝나지 않았을 수 있다.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            return;
        Scene active = SceneManager.GetActiveScene();
        if (active.path != path && active.isDirty)
            Debug.LogWarning("한씨: 저장하지 않은 씬을 유지했습니다. 이동 예제 씬을 직접 열어주세요.");
        else if (active.path != path)
            EditorSceneManager.OpenScene(path);
        File.Delete(Flag);
        EditorApplication.update -= OpenWhenReady;
    }
}