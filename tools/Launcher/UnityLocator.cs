using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace HanCSharp.Launcher
{
    /// <summary>설치된 정확한 Unity 버전을 찾는다. 없으면 UI에서 사용자가 경로를 고른다.</summary>
    internal static class UnityLocator
    {
        public static string Find(string version)
        {
            var candidates = new List<string>();
            foreach (string root in new[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Hub", "Editor"),
                Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "UnityEditors") })
            {
                if (Directory.Exists(root))
                    candidates.AddRange(Directory.GetDirectories(root).Select(folder => Path.Combine(folder, "Editor", "Unity.exe")));
            }
            string metadata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnityHub", "installedEditorBinaryMetadata.json");
            if (File.Exists(metadata))
            {
                try
                {
                    var entries = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(metadata));
                    candidates.AddRange(entries.Keys);
                }
                catch (ArgumentException) { }
            }
            foreach (string path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (File.Exists(path) && (FileVersionInfo.GetVersionInfo(path).ProductVersion ?? "").StartsWith(version, StringComparison.OrdinalIgnoreCase))
                    return path;
            }
            return null;
        }
    }
}
