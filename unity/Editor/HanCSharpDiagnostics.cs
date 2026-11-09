using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>실제 Unity 컴파일 오류만 모아 원본 행으로 연결한다. Console 내부 구현에는 의존하지 않는다.</summary>
[InitializeOnLoad]
public static class HanCSharpDiagnostics
{
    public sealed class Entry
    {
        public string Source { get; }
        public int Line { get; }
        public string Message { get; }
        public Entry(string source, int line, string message)
        {
            Source = source;
            Line = line;
            Message = message;
        }
    }

    private static readonly Dictionary<string, List<Entry>> _assemblies = new Dictionary<string, List<Entry>>();
    public static event Action Changed;

    static HanCSharpDiagnostics()
    {
        CompilationPipeline.assemblyCompilationFinished += OnCompiled;
    }

    public static List<Entry> ForSource(string source)
    {
        var result = new List<Entry>();
        foreach (var entries in _assemblies.Values)
        {
            foreach (Entry entry in entries)
            {
                if (string.Equals(entry.Source, source, StringComparison.OrdinalIgnoreCase))
                    result.Add(entry);
            }
        }
        return result;
    }

    private static void OnCompiled(string assembly, CompilerMessage[] messages)
    {
        var entries = new List<Entry>();
        foreach (CompilerMessage message in messages)
        {
            if (message.type != CompilerMessageType.Error || string.IsNullOrEmpty(message.file))
                continue;
            string path = AssetPath(message.file);
            string source = HanCSharpProjectIntegration.ResolveHanSource(path);
            if (source == null)
                continue;
            int line = message.line;
            if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                && !HanCSharpSourceMap.TryResolve(path, message.line, source, out line))
                continue;
            entries.Add(new Entry(source, Math.Max(1, line), message.message));
        }
        _assemblies[assembly] = entries;
        EditorApplication.delayCall += Notify;
    }

    private static void Notify()
    {
        Changed?.Invoke();
    }

    private static string AssetPath(string path)
    {
        string full = Path.GetFullPath(path).Replace('\\', '/');
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace('\\', '/').TrimEnd('/') + "/";
        return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full.Substring(root.Length) : path.Replace('\\', '/');
    }
}
