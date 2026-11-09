// 검증 전용 Unity API 모형이다. Unity 설치 파일 및 사용자 프로젝트에는 포함하지 않는다.
namespace UnityEngine
{
    public class Object { }
    public class Texture2D : Object { }
    public class GUIContent { public Object image; }
    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }
    public class SerializeField : System.Attribute { }
    public static class Debug
    {
        public static readonly List<string> Errors = new();
        public static void LogError(object message) => Errors.Add(message.ToString());
        public static void Log(object message) { }
    }
}

namespace UnityEditor
{
    public class AssetPostprocessor { }
    public class MenuItem : System.Attribute
    {
        public MenuItem(string name) { }
        public MenuItem(string name, bool validate, int priority) { }
    }
    public static class Selection
    {
        public static UnityEngine.Object activeObject;
    }
    public static class EditorSettings
    {
        public static string[] projectGenerationUserExtensions { get; set; } = ["txt"];
    }
    public static class EditorUtility
    {
        public static UnityEngine.Object InstanceIDToObject(int id) => null;
        public static bool DisplayDialog(string title, string message, string ok) => true;
    }
    public static class EditorGUIUtility
    {
        public static UnityEngine.GUIContent IconContent(string name) => new();
    }
    public static class ProjectWindowUtil
    {
        public static void StartNameEditingIfProjectWindowExists(int id, ProjectWindowCallback.EndNameEditAction action, string path, UnityEngine.Texture2D icon, string resource) { }
        public static void ShowCreatedAsset(UnityEngine.Object value) { }
    }
    public static class EditorApplication
    {
        public static bool isCompiling;
        public static bool isUpdating;
        public static event Action delayCall;
        public static void Flush()
        {
            Action pending = delayCall;
            delayCall = null;
            pending?.Invoke();
        }
    }
    public static class AssetDatabase
    {
        private static readonly Dictionary<string, UnityEngine.Object> _assets = new();
        public static string GetAssetPath(UnityEngine.Object value) => "";
        public static string GetAssetPath(int instanceId) => "";
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object =>
            _assets.TryGetValue(path, out var value) ? value as T : null;
        public static void CreateAsset(UnityEngine.Object value, string path)
        {
            _assets[path] = value;
            File.WriteAllText(path, "test settings");
        }
        public static void SaveAssets() { }
        public static void Refresh() { }
        public static void ImportAsset(string path) { }
        public static UnityEngine.Object LoadMainAssetAtPath(string path) => File.Exists(path) ? new UnityEngine.Object() : null;
        public static bool DeleteAsset(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".meta")) File.Delete(path + ".meta");
            return true;
        }
        public static string MoveAsset(string previous, string next)
        {
            File.Move(previous, next);
            if (File.Exists(previous + ".meta")) File.Move(previous + ".meta", next + ".meta");
            return "";
        }
    }
}

namespace UnityEditor.ProjectWindowCallback
{
    public abstract class EndNameEditAction : UnityEngine.ScriptableObject
    {
        public abstract void Action(int instanceId, string pathName, string resourceFile);
    }
}

namespace UnityEditor.Callbacks
{
    public class OnOpenAssetAttribute : System.Attribute
    {
        public OnOpenAssetAttribute(int order = 0) { }
    }
}

namespace UnityEditorInternal
{
    public static class InternalEditorUtility
    {
        public static string LastFile;
        public static int LastLine;
        public static void OpenFileAtLineExternal(string file, int line)
        {
            LastFile = file;
            LastLine = line;
        }
    }
}

namespace Unity.CodeEditor
{
    public sealed class StubCodeEditor
    {
        public bool OpenProject(string path, int line, int column) => false;
    }
    public static class CodeEditor
    {
        public static readonly StubCodeEditor CurrentEditor = new();
    }
}
