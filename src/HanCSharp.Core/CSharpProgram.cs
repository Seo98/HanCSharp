using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HanCSharp;

/// <summary>
/// 현재 .NET 런타임의 기본 참조로 생성 코드를 검사하고 실행한다.
/// Unity나 외부 패키지 참조는 포함하지 않으며, 실행은 현재 사용자 권한으로 수행된다.
/// </summary>
public sealed class CSharpProgram
{
    private static readonly Lazy<MetadataReference[]> _references = new(() =>
    {
        string[] paths = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException(".NET 런타임 참조를 찾지 못했습니다."))
            .Split(Path.PathSeparator);
        return paths.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToArray();
    });

    private readonly CSharpCompilation _compilation;

    public CSharpProgram(string source, bool library = false, IEnumerable<string>? referencePaths = null)
    {
        var references = _references.Value.Concat((referencePaths ?? []).Select(path =>
            (MetadataReference)MetadataReference.CreateFromFile(Path.GetFullPath(path))));
        _compilation = CSharpCompilation.Create("HanCSharp_" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, path: "generated.cs")], references,
            new CSharpCompilationOptions(library ? OutputKind.DynamicallyLinkedLibrary : OutputKind.ConsoleApplication));
    }

    /// <summary>의미 검사 결과는 생성된 C#의 행 번호를 기준으로 반환한다.</summary>
    public IReadOnlyList<Diagnostic> Check() => _compilation.GetDiagnostics()
        .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning).ToArray();

    /// <summary>컴파일 오류가 없을 때 진입점을 실행하며, 정수 반환값을 종료 코드로 사용한다.</summary>
    public async Task<int> RunAsync(string[] args)
    {
        using var stream = new MemoryStream();
        var result = _compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine,
                result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        }
        stream.Position = 0;
        var context = new AssemblyLoadContext("HanCSharpRun", isCollectible: true);
        try
        {
            Assembly assembly = context.LoadFromStream(stream);
            MethodInfo entry = assembly.EntryPoint ?? throw new InvalidOperationException("실행할 Main이 없습니다.");
            object? value;
            try
            {
                value = entry.Invoke(null, entry.GetParameters().Length == 0 ? null : [args]);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
            if (value is Task<int> intTask)
            {
                return await intTask;
            }
            if (value is Task task)
            {
                await task;
                return 0;
            }
            return value is int exitCode ? exitCode : 0;
        }
        finally
        {
            context.Unload();
        }
    }
}
