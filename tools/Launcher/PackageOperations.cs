using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HanCSharp.Launcher
{
    /// <summary>사용자 파일을 덮어쓰지 않고 설치 및 짧은 경로 예제 준비를 수행한다.</summary>
    public static class PackageOperations
    {
        public const int MaximumDemoPathLength = 80;

        public static string LongPath(string path)
        {
            string full = Path.GetFullPath(path);
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || full.StartsWith(@"\\?\", StringComparison.Ordinal))
                return full;
            return full.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + full.Substring(2) : @"\\?\" + full;
        }

        public static string DefaultDemoPath(string root)
        {
            string version = File.ReadAllText(LongPath(Path.Combine(root, "VERSION"))).Trim();
            string suffix = Path.Combine("HanCSharp", "MovementDemo-" + version);
            foreach (string folder in new[] { Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) })
            {
                if (!string.IsNullOrEmpty(folder) && Path.Combine(folder, suffix).Length <= MaximumDemoPathLength)
                    return Path.Combine(folder, suffix);
            }
            throw new IOException("기본 경로가 너무 깁니다. 예제 준비 시 짧은 폴더를 선택하세요.");
        }

        public static string PrepareDemo(string root, string destination)
        {
            string target = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar);
            if (target.Length > MaximumDemoPathLength)
                throw new IOException("Unity 예제 경로는 80자 이하로 선택하세요. 예: C:\\HanCSharp\\Demo");
            string version = File.ReadAllText(LongPath(Path.Combine(root, "VERSION"))).Trim();
            string owner = "HanCSharp Demo " + version;
            string marker = Path.Combine(target, ".hancsharp-demo");
            if (Directory.Exists(LongPath(target)))
            {
                if (!File.Exists(LongPath(marker)) || File.ReadAllText(LongPath(marker)).Trim() != owner
                    || !File.Exists(LongPath(Path.Combine(target, "ProjectSettings", "ProjectVersion.txt"))))
                    throw new IOException("이 폴더에는 기존 파일이 있습니다. 비어 있는 새 폴더를 선택하세요.");
                RequestDemoScene(target);
                return target;
            }
            string parent = Path.GetDirectoryName(target);
            Directory.CreateDirectory(LongPath(parent));
            string staging = Path.Combine(parent, Path.GetFileName(target) + ".preparing-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(LongPath(staging));
            using (var stream = File.OpenRead(LongPath(Path.Combine(root, "templates", "UnityMovementDemo.zip"))))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string file = Path.GetFullPath(Path.Combine(staging, entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
                    if (!file.StartsWith(staging + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new IOException("예제 압축 파일의 경로가 올바르지 않습니다.");
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(LongPath(file));
                        continue;
                    }
                    Directory.CreateDirectory(LongPath(Path.GetDirectoryName(file)));
                    using (var input = entry.Open())
                    using (var output = File.Create(LongPath(file)))
                        input.CopyTo(output);
                }
            }
            CopyTree(Path.Combine(root, "app"), Path.Combine(staging, "Tools", "HanCSharp"));
            File.WriteAllText(LongPath(Path.Combine(staging, ".hancsharp-demo")), owner, new UTF8Encoding(false));
            // 이름 바꾸기는 검증한 동일 부모의 두 경로 사이에서만 수행한다.
            if (Path.GetDirectoryName(staging) != parent || Path.GetDirectoryName(target) != parent)
                throw new IOException("예제 준비 경로가 지정한 부모 폴더를 벗어났습니다.");
            Directory.Move(LongPath(staging), LongPath(target));
            RequestDemoScene(target);
            return target;
        }

        private static void RequestDemoScene(string project)
        {
            string flag = Path.Combine(project, "UserSettings", "HanCSharp", "OpenDemo.txt");
            Directory.CreateDirectory(LongPath(Path.GetDirectoryName(flag)));
            File.WriteAllText(LongPath(flag), "open", Encoding.UTF8);
        }

        public static void InstallUnity(string root, string project, bool includeExamples)
        {
            string targetRoot = Path.GetFullPath(project);
            if (!Directory.Exists(LongPath(Path.Combine(targetRoot, "Assets"))) || !Directory.Exists(LongPath(Path.Combine(targetRoot, "ProjectSettings"))))
                throw new IOException("Assets와 ProjectSettings가 있는 Unity 프로젝트 루트를 선택하세요.");
            var copies = new List<KeyValuePair<string, string>>();
            AddCopies(copies, Path.Combine(root, "app"), Path.Combine(targetRoot, "Tools", "HanCSharp"), "*");
            AddCopies(copies, Path.Combine(root, "unity", "Editor"), Path.Combine(targetRoot, "Assets", "HanCSharp", "Editor"), "*.cs");
            if (includeExamples)
                AddCopies(copies, Path.Combine(root, "samples", "UnityScripts"), Path.Combine(targetRoot, "Assets", "HanCSharp", "Examples"), "*.han");
            foreach (var copy in copies)
            {
                if (File.Exists(LongPath(copy.Value)) && !SameFile(copy.Key, copy.Value))
                    throw new IOException("기존 사용자 파일을 덮어쓰지 않습니다. 비교·백업 후 갱신하세요: " + copy.Value);
            }
            foreach (var copy in copies)
            {
                Directory.CreateDirectory(LongPath(Path.GetDirectoryName(copy.Value)));
                if (!File.Exists(LongPath(copy.Value)))
                    File.Copy(LongPath(copy.Key), LongPath(copy.Value));
            }
        }

        private static void AddCopies(List<KeyValuePair<string, string>> copies, string source, string destination, string pattern)
        {
            string full = LongPath(source).TrimEnd(Path.DirectorySeparatorChar);
            foreach (string file in Directory.GetFiles(full, pattern, SearchOption.AllDirectories))
                copies.Add(new KeyValuePair<string, string>(file, Path.Combine(destination, file.Substring(full.Length + 1))));
        }

        private static bool SameFile(string first, string second)
        {
            using (var sha = SHA256.Create())
            using (var a = File.OpenRead(LongPath(first)))
            using (var b = File.OpenRead(LongPath(second)))
                return sha.ComputeHash(a).SequenceEqual(sha.ComputeHash(b));
        }

        private static void CopyTree(string source, string target)
        {
            var copies = new List<KeyValuePair<string, string>>();
            AddCopies(copies, source, target, "*");
            foreach (var copy in copies)
            {
                Directory.CreateDirectory(LongPath(Path.GetDirectoryName(copy.Value)));
                File.Copy(LongPath(copy.Key), LongPath(copy.Value));
            }
        }
    }
}
