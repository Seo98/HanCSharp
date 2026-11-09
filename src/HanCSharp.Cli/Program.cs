using System.Text;
using HanCSharp;
using Microsoft.CodeAnalysis;

Console.OutputEncoding = Encoding.UTF8;
if (args.Length == 0 || args[0] is "help" or "--help")
{
    Console.WriteLine("""
        한씨(HanCSharp) — 문장형 한국어 ↔ C# 프로토타입
        to-cs 입력.han 출력.cs [--source-map | --write-line-map] [--force]   한국어를 C#으로 변환
        to-ko 입력.cs 출력.han   C#을 한국어로 변환
        check 입력.han [--library] [--reference 파일이나폴더]   C# 컴파일 검사
        run 입력.han [프로그램 인수...]   변환하고 실행
        파일 덮어쓰기는 --force를 마지막에 붙여 명시합니다.
        """);
    return 0;
}

try
{
    string command = args[0];
    if (command is not ("to-cs" or "to-ko" or "check" or "run"))
    {
        throw new ArgumentException($"알 수 없는 명령: {command}");
    }
    if (args.Length < 2)
    {
        throw new ArgumentException("입력 파일 경로가 필요합니다.");
    }
    string source = await File.ReadAllTextAsync(args[1], Encoding.UTF8);
    if (command is "to-cs" or "to-ko")
    {
        if (args.Length < 3 || args.Skip(3).Any(a => a != "--force" && ((a != "--source-map" && a != "--write-line-map") || command != "to-cs"))
            || (args.Contains("--source-map") && args.Contains("--write-line-map")))
        {
            throw new ArgumentException("입력 경로와 출력 경로를 지정하세요. 선택 사항: --force");
        }
        if (Path.GetFullPath(args[1]).Equals(Path.GetFullPath(args[2]), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("입력과 출력은 서로 다른 파일이어야 합니다.");
        }
        bool writeLineMap = args.Contains("--write-line-map");
        if (writeLineMap && Path.GetFullPath(args[2] + ".hanmap").Equals(Path.GetFullPath(args[1]), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("행 매핑 출력과 입력은 서로 다른 파일이어야 합니다.");
        if (writeLineMap && File.Exists(args[2] + ".hanmap") && !args.Contains("--force"))
            throw new IOException("행 매핑 파일이 이미 있습니다. --force를 사용하세요.");
        var lineMap = writeLineMap ? new List<int>() : null;
        string result = command == "to-cs" ? new KoreanToCSharp().Convert(source, args.Contains("--source-map") ? args[1] : null, lineMap) : new CSharpToKorean().Convert(source);
        FileMode mode = args.Contains("--force") ? FileMode.Create : FileMode.CreateNew;
        await using var stream = new FileStream(args[2], mode, FileAccess.Write);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        await writer.WriteAsync(result);
        if (lineMap != null)
            await File.WriteAllLinesAsync(args[2] + ".hanmap", lineMap.Select(line => line.ToString(System.Globalization.CultureInfo.InvariantCulture)), new UTF8Encoding(false));
        Console.WriteLine($"변환 완료: {Path.GetFullPath(args[2])}");
        return 0;
    }
    var references = new List<string>();
    if (command == "check")
    {
        for (int index = 2; index < args.Length; index++)
        {
            if (args[index] == "--library") continue;
            if (args[index] == "--reference" && index + 1 < args.Length)
            {
                string path = args[++index];
                if (Directory.Exists(path)) references.AddRange(Directory.EnumerateFiles(path, "*.dll"));
                else references.Add(path);
                continue;
            }
            throw new ArgumentException("check의 선택 사항은 --library 및 --reference 경로입니다.");
        }
    }
    string generated = new KoreanToCSharp().Convert(source, args[1]);
    var program = new CSharpProgram(generated, library: command == "check" && args.Contains("--library"), referencePaths: references);
    var diagnostics = program.Check();
    foreach (var diagnostic in diagnostics)
    {
        Console.Error.WriteLine(diagnostic);
    }
    if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
    {
        return 1;
    }
    if (command == "check")
    {
        Console.WriteLine("컴파일 검사 통과 (진단 경로와 행 번호는 한씨 원본 기준)");
        return 0;
    }
    return await program.RunAsync(args.Skip(2).ToArray());
}
catch (LanguageException exception)
{
    Console.Error.WriteLine($"{Path.GetFullPath(args[1])}({exception.LineNumber},1): error HCS001: {exception.Message}");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
