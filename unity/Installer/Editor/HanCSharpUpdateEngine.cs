using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>관리 파일만 검사·백업·원자적 교체한다. 사용자 변경 충돌 시 전체 적용을 거부하고 실패 시 되돌린다.</summary>
public static class HanCSharpUpdateEngine
{
    public const string StateRelativePath = "ProjectSettings/HanCSharpInstallState.json";

    public static string Hash(string file)
    {
        if (!File.Exists(file)) return null;
        using (var stream = File.OpenRead(file))
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    public static string Normalize(string path)
    {
        if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path) || path.Contains(":"))
            throw new IOException("설치 경로가 올바르지 않습니다: " + path);
        string normalized = path.Replace('\\', '/');
        if (normalized.Split('/').Any(part => part.Length == 0 || part == "." || part == ".."))
            throw new IOException("상위 경로 또는 빈 경로는 허용하지 않습니다: " + path);
        bool allowed = normalized.StartsWith("Assets/HanCSharp/Editor/", StringComparison.Ordinal)
            || normalized.StartsWith("Assets/HanCSharp/Examples/", StringComparison.Ordinal)
            || normalized.StartsWith("Assets/HanCSharp/Documentation/", StringComparison.Ordinal)
            || normalized.StartsWith("Tools/HanCSharp/", StringComparison.Ordinal);
        if (!allowed || normalized.StartsWith("Assets/HanCSharp/Editor/HanCSharpSettings.asset", StringComparison.Ordinal)
            || new[] { ".exe", ".bat", ".cmd", ".ps1", ".msi", ".com" }.Contains(Path.GetExtension(normalized).ToLowerInvariant()))
            throw new IOException("관리 대상 밖의 파일은 설치하지 않습니다: " + path);
        return normalized;
    }

    public static void Extract(string archive, string stage)
    {
        if (Directory.Exists(stage)) throw new IOException("새 스테이징 폴더를 사용하세요.");
        Directory.CreateDirectory(stage);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long total = 0;
        using (var zip = ZipFile.OpenRead(archive))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;
                string path = entry.FullName.Replace('\\', '/');
                if (path != "manifest.json" && path != "baselines.json") path = Normalize(path);
                if (!seen.Add(path)) throw new IOException("중복 설치 경로입니다: " + path);
                total += entry.Length;
                if (total > 512L * 1024 * 1024) throw new IOException("설치 파일 크기 제한을 초과했습니다.");
                string target = Path.Combine(stage, path);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                using (var input = entry.Open())
                using (var output = File.Create(target)) input.CopyTo(output);
            }
        }
        if (!File.Exists(Path.Combine(stage, "manifest.json"))) throw new IOException("배포 목록이 없습니다.");
    }

    public static HanCSharpUpdatePlan Plan(string project, string stage, HanCSharpUpdateManifest incoming,
        HanCSharpUpdateManifest installed, HanCSharpBaselineCatalog baselines)
    {
        project = Path.GetFullPath(project);
        stage = Path.GetFullPath(stage);
        if (!Directory.Exists(Path.Combine(project, "Assets")) || !Directory.Exists(Path.Combine(project, "ProjectSettings")))
            throw new IOException("Unity 프로젝트 루트가 아닙니다.");
        Version next;
        if (incoming == null || incoming.schemaVersion != 1 || !Version.TryParse(incoming.version, out next) || incoming.files == null || incoming.files.Length == 0)
            throw new IOException("배포 버전 또는 파일 목록이 올바르지 않습니다.");
        Version previous;
        if (installed != null && Version.TryParse(installed.version, out previous) && previous > next)
            throw new IOException("이전 버전으로 자동 되돌리지는 않습니다.");
        GuardLinks(project, StateRelativePath);
        var plan = new HanCSharpUpdatePlan { ProjectRoot = project, StageRoot = stage, Incoming = incoming,
            Installed = installed, StateHash = Hash(Path.Combine(project, StateRelativePath)) };
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (HanCSharpUpdateFile file in incoming.files)
        {
            file.path = Normalize(file.path);
            if (!paths.Add(file.path) || string.IsNullOrEmpty(file.sha256) || file.sha256.Length != 64)
                throw new IOException("배포 파일 항목이 올바르지 않습니다.");
            GuardLinks(project, file.path);
            GuardLinks(stage, file.path);
            if (!Same(Hash(Path.Combine(stage, file.path)), file.sha256))
                throw new IOException("배포 파일 해시가 일치하지 않습니다: " + file.path);
            string target = Path.Combine(project, file.path);
            string current = Hash(target);
            bool keep = file.preserveExisting || file.path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)
                || file.path.StartsWith("Assets/HanCSharp/Examples/", StringComparison.Ordinal);
            HanCSharpUpdateAction action;
            if (Directory.Exists(target)) action = HanCSharpUpdateAction.Conflict;
            else if (current == null) action = HanCSharpUpdateAction.Add;
            else if (keep || Same(current, file.sha256)) action = HanCSharpUpdateAction.Keep;
            else if (IsKnown(file.path, current, installed, baselines)) action = HanCSharpUpdateAction.Replace;
            else action = HanCSharpUpdateAction.Conflict;
            plan.Changes.Add(new HanCSharpUpdateChange { File = file, Action = action, BeforeHash = current });
        }
        return plan;
    }

    public static string Apply(HanCSharpUpdatePlan plan, Func<HanCSharpUpdateManifest, string> serialize)
    {
        if (!plan.CanApply) throw new IOException("수정되거나 출처를 확인할 수 없는 파일이 있어 적용하지 않습니다.");
        GuardLinks(plan.ProjectRoot, StateRelativePath);
        string state = Path.Combine(plan.ProjectRoot, StateRelativePath);
        if (!Same(Hash(state), plan.StateHash)) throw new IOException("검사 후 설치 상태가 바뀌었습니다. 다시 검사하세요.");
        foreach (var change in plan.Changes)
        {
            GuardLinks(plan.ProjectRoot, change.File.path);
            GuardLinks(plan.StageRoot, change.File.path);
            if (!Same(Hash(Path.Combine(plan.ProjectRoot, change.File.path)), change.BeforeHash)
                || !Same(Hash(Path.Combine(plan.StageRoot, change.File.path)), change.File.sha256)
                || Directory.Exists(Path.Combine(plan.ProjectRoot, change.File.path)))
                throw new IOException("검사 후 파일이 바뀌었습니다. 다시 검사하세요: " + change.File.path);
        }
        string backup = Path.Combine(plan.ProjectRoot, "HanCSharpBackups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
        GuardLinks(plan.ProjectRoot, "HanCSharpBackups");
        Directory.CreateDirectory(backup);
        foreach (var change in plan.Changes.Where(c => c.Action == HanCSharpUpdateAction.Replace))
        {
            string target = Path.Combine(backup, change.File.path);
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.Copy(Path.Combine(plan.ProjectRoot, change.File.path), target);
            if (!Same(Hash(target), change.BeforeHash)) throw new IOException("백업 중 파일이 변경됐습니다. 다시 검사하세요.");
        }
        if (File.Exists(state)) File.Copy(state, Path.Combine(backup, "before-state.json"));
        var applied = new List<HanCSharpUpdateChange>();
        string writtenStateHash = null;
        try
        {
            foreach (var change in plan.Changes.Where(c => c.Action == HanCSharpUpdateAction.Add || c.Action == HanCSharpUpdateAction.Replace))
            {
                string target = Path.Combine(plan.ProjectRoot, change.File.path);
                GuardLinks(plan.ProjectRoot, Normalize(change.File.path));
                if (!Same(Hash(target), change.BeforeHash)) throw new IOException("적용 직전 파일이 변경됐습니다: " + change.File.path);
                byte[] content = File.ReadAllBytes(Path.Combine(plan.StageRoot, change.File.path));
                if (!Same(HashBytes(content), change.File.sha256)) throw new IOException("배포 파일이 변경됐습니다: " + change.File.path);
                AtomicWrite(target, content);
                applied.Add(change);
            }
            var files = new Dictionary<string, HanCSharpUpdateFile>(StringComparer.OrdinalIgnoreCase);
            if (plan.Installed != null && plan.Installed.files != null)
                foreach (var file in plan.Installed.files) files[Normalize(file.path)] = file;
            foreach (var change in plan.Changes)
                files[change.File.path] = new HanCSharpUpdateFile { path = change.File.path,
                    sha256 = Hash(Path.Combine(plan.ProjectRoot, change.File.path)), preserveExisting = change.File.preserveExisting };
            string json = serialize(new HanCSharpUpdateManifest { version = plan.Incoming.version, files = files.Values.ToArray() });
            if (!Same(Hash(state), plan.StateHash)) throw new IOException("설치 상태가 동시에 변경됐습니다.");
            AtomicWrite(state, new UTF8Encoding(false).GetBytes(json));
            writtenStateHash = HashBytes(new UTF8Encoding(false).GetBytes(json));
            File.WriteAllText(Path.Combine(backup, "README.txt"), "한씨 " + plan.Incoming.version + " 적용 전 교체 파일 백업입니다. 기존 예제·meta는 보존했습니다.", Encoding.UTF8);
            return backup;
        }
        catch (Exception failure)
        {
            var failures = new List<string>();
            foreach (var change in applied.AsEnumerable().Reverse())
            {
                string target = Path.Combine(plan.ProjectRoot, change.File.path);
                try
                {
                    if (!Same(Hash(target), change.File.sha256)) throw new IOException("동시에 수정된 파일");
                    if (change.BeforeHash == null) File.Delete(target);
                    else AtomicWrite(target, File.ReadAllBytes(Path.Combine(backup, change.File.path)));
                }
                catch (Exception error) { failures.Add(change.File.path + ": " + error.Message); }
            }
            // 상태 기록까지 끝난 뒤의 실패라면 이전 상태도 되돌린다.
            if (writtenStateHash != null)
            {
                try
                {
                    if (!Same(Hash(state), writtenStateHash)) throw new IOException("설치 상태가 동시에 수정됐습니다.");
                    if (plan.StateHash == null) File.Delete(state);
                    else AtomicWrite(state, File.ReadAllBytes(Path.Combine(backup, "before-state.json")));
                }
                catch (Exception error) { failures.Add("설치 상태: " + error.Message); }
            }
            throw new IOException("적용 실패. 백업: " + backup + "\n" + failure.Message
                + (failures.Count == 0 ? "\n적용 파일을 복구했습니다." : "\n직접 확인 필요: " + string.Join("; ", failures)), failure);
        }
    }

    private static void AtomicWrite(string target, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        string temporary = target + ".hancsharp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllBytes(temporary, bytes);
            if (File.Exists(target)) File.Replace(temporary, target, null);
            else File.Move(temporary, target);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static bool IsKnown(string path, string hash, HanCSharpUpdateManifest installed, HanCSharpBaselineCatalog catalog)
    {
        if (installed != null && installed.files != null && installed.files.Any(f => f.path == path && Same(f.sha256, hash))) return true;
        return catalog != null && catalog.entries != null && catalog.entries.Any(f => f.path == path && f.hashes != null && f.hashes.Any(h => Same(h, hash)));
    }

    private static bool Same(string first, string second) => string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

    private static string HashBytes(byte[] bytes)
    {
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }

    private static void GuardLinks(string root, string relative)
    {
        string current = Path.GetFullPath(root);
        CheckLink(current);
        foreach (string part in relative.Replace('\\', '/').Split('/'))
        {
            current = Path.Combine(current, part);
            CheckLink(current);
        }
    }

    private static void CheckLink(string path)
    {
        if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("링크 경로를 통해 설치하지 않습니다: " + path);
    }
}
