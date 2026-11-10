using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>고정한 공식 저장소의 최신 배포만 GET으로 조회하고 첨부 SHA-256을 검증한다.</summary>
public static class HanCSharpReleaseDownload
{
    [Serializable] private sealed class Release { public string tag_name = ""; public Asset[] assets = new Asset[0]; }
    [Serializable] private sealed class Asset { public string name = ""; public string browser_download_url = ""; }
    public const string PayloadName = "HanCSharp-UnityPayload.zip";
    private const string Prefix = "https://github.com/Seo98/HanCSharp/releases/download/";

    public static async Task<string> Latest(string destination)
    {
        string json = await GetText("https://api.github.com/repos/Seo98/HanCSharp/releases/latest");
        var release = JsonUtility.FromJson<Release>(json);
        string payload = null;
        string sums = null;
        foreach (var asset in release.assets ?? new Asset[0])
        {
            if (asset.name == PayloadName) payload = asset.browser_download_url;
            if (asset.name == "SHA256SUMS.txt") sums = asset.browser_download_url;
        }
        if (payload == null || sums == null) throw new IOException("최신 배포에 Unity 업데이트 파일이 없습니다. Unity 패키지 배포를 확인하세요.");
        if (!payload.StartsWith(Prefix, StringComparison.Ordinal) || !sums.StartsWith(Prefix, StringComparison.Ordinal))
            throw new IOException("공식 배포 다운로드 경로가 아닙니다.");
        string checksums = await GetText(sums);
        Match checksum = Regex.Match(checksums, @"(?m)^([0-9a-fA-F]{64})\s+\*?HanCSharp-UnityPayload\.zip\s*$");
        if (!checksum.Success) throw new IOException("배포 해시 항목이 없습니다.");
        Directory.CreateDirectory(destination);
        string archive = Path.Combine(destination, PayloadName);
        using (var request = UnityWebRequest.Get(payload))
        {
            request.timeout = 60;
            request.downloadHandler = new DownloadHandlerFile(archive);
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success) throw new IOException(request.error);
        }
        if (!string.Equals(HanCSharpUpdateEngine.Hash(archive), checksum.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
            throw new IOException("다운로드 해시가 일치하지 않습니다. 적용하지 않습니다.");
        return archive;
    }

    private static async Task<string> GetText(string url)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = 30;
            request.SetRequestHeader("User-Agent", "HanCSharp-Unity-Installer");
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success) throw new IOException(request.error);
            return request.downloadHandler.text;
        }
    }
}
