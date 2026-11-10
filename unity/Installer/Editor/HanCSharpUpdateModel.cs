using System;
using System.Collections.Generic;

/// <summary>Unity JsonUtility와 독립 테스트에서 공유하는 배포·설치 상태 형식이다.</summary>
[Serializable]
public sealed class HanCSharpUpdateManifest
{
    public int schemaVersion = 1;
    public string version;
    public HanCSharpUpdateFile[] files = new HanCSharpUpdateFile[0];
}

[Serializable]
public sealed class HanCSharpUpdateFile
{
    public string path;
    public string sha256;
    public bool preserveExisting;
}

[Serializable]
public sealed class HanCSharpBaselineCatalog
{
    public HanCSharpKnownFile[] entries = new HanCSharpKnownFile[0];
}

[Serializable]
public sealed class HanCSharpKnownFile
{
    public string path;
    public string[] hashes = new string[0];
}

public enum HanCSharpUpdateAction { Add, Replace, Keep, Conflict }

public sealed class HanCSharpUpdateChange
{
    public HanCSharpUpdateFile File { get; internal set; }
    public HanCSharpUpdateAction Action { get; internal set; }
    public string BeforeHash { get; internal set; }
}

/// <summary>검사 시점의 파일 해시를 고정한다. 적용 직전에 같은 상태인지 다시 검사한다.</summary>
public sealed class HanCSharpUpdatePlan
{
    public string ProjectRoot { get; internal set; }
    public string StageRoot { get; internal set; }
    public HanCSharpUpdateManifest Incoming { get; internal set; }
    public HanCSharpUpdateManifest Installed { get; internal set; }
    public string StateHash { get; internal set; }
    public List<HanCSharpUpdateChange> Changes { get; } = new List<HanCSharpUpdateChange>();
    public bool CanApply => !Changes.Exists(change => change.Action == HanCSharpUpdateAction.Conflict);
}
