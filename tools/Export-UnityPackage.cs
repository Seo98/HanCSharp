using System.IO;
using UnityEditor;

/// <summary>Unity가 관리하는 GUID와 자산으로 설치 패키지를 내보낸다. 실행 프로젝트의 Library 포인터만 사용한다.</summary>
public static class ExportHanCSharpUnityPackage
{
    public static object Main()
    {
        string flag = "Library/HanCSharp/package-output.txt";
        string output = File.ReadAllText(flag).Trim();
        if (File.Exists(output)) throw new IOException("기존 패키지는 덮어쓰지 않습니다: " + output);
        AssetDatabase.Refresh();
        AssetDatabase.ExportPackage(new[] { "Assets/HanCSharpInstaller" }, output, ExportPackageOptions.Recurse);
        File.Delete(flag);
        return new { output, bytes = new FileInfo(output).Length };
    }
}
