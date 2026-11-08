using System.Text;
using HanCSharp;
using Microsoft.CodeAnalysis;

Console.OutputEncoding = Encoding.UTF8;
int passed = 0;
int failed = 0;

await Test("변수/계산/문자열/조건/아니면", async () =>
{
    await ExpectProgram("""
        정수 a는 0이다.
        a에 3을 더한다.
        a에 2를 곱한다.
        만약 a가 5보다 크면:
            출력 "정수 a는 0이다.".
        아니면:
            출력 "실패".
        끝.
        a에서 1을 뺀다.
        출력 a.
        """, "정수 a는 0이다.\n5\n");
});

await Test("함수/중첩 블록/반환", async () =>
{
    await ExpectProgram("""
        함수 더하기(정수 x, 정수 y)는 정수:
            반환 x + y.
        끝.
        정수 합계는 더하기(2, 3)이다.
        동안 합계가 7보다 작으면:
            합계에 1을 더한다.
        끝.
        출력 합계.
        """, "7\n");
});

await Test("반복/계속/중단", async () =>
{
    await ExpectProgram("""
        반복 i는 6회:
            만약 i가 1과 같으면:
                계속.
            끝.
            만약 i가 4와 같으면:
                중단.
            끝.
            출력 i.
        끝.
        """, "0\n2\n3\n");
});

await Test("문자열 보간/원문/외부 C# API", async () =>
{
    await ExpectProgram(""""
        ```csharp
        using System.Collections.Generic;
        ```
        자동 값들은 new List<int> { 1, 2, 3 }이다.
        정수 합계는 0이다.
        ```csharp
        foreach (var 값 in 값들) { 합계 += 값; }
        ```
        출력 $"결과:{합계}".
        """", "결과:6\n");
});

await Test("C# 주석과 문자열 보존", async () =>
{
    await ExpectCSharp("""
        // 정수 a는 0이다.
        int a = 2; /* a에 3을 더한다. */
        System.Console.WriteLine("끝. 만약");
        System.Console.WriteLine(a);
        """, "끝. 만약\n2\n");
});

await Test("클래스/메서드 원문 보존", async () =>
{
    await ExpectCSharp("""
        class 정수
        {
            static void Main() { System.Console.WriteLine(42); }
        }
        """, "42\n");
});

await Test("전처리기/비활성 코드 보존", async () =>
{
    await ExpectCSharp("""
        #define DEMO
        #if DEMO
        System.Console.WriteLine(1);
        #else
        invalid text here
        #endif
        """, "1\n");
});

await Test("raw 문자열 내부 fence/줄바꿈 보존", async () =>
{
    await ExpectCSharp(""""
        var text = """
        ```
        정수 a는 0이다.
        """;
        System.Console.WriteLine(text);
        """", "```\n정수 a는 0이다.\n");
});

await Test("static 함수/다중 변수/using 원문 보존", async () =>
{
    await ExpectCSharp("""
        using System;
        int x = 1, y = 2;
        static int Add(int a, int b) => a + b;
        Console.WriteLine(Add(x, y));
        """, "3\n");
});

await Test("C# 식의 null/논리/비교/연산 유지", async () =>
{
    await ExpectProgram("""
        논리 활성은 true이다.
        문자열 이름은 null이다.
        만약 활성 && 이름 == null이면:
            출력 2 + 3 * 4.
        끝.
        """, "14\n");
});

await Test("반환 없는 함수", async () =>
{
    await ExpectProgram("""
        함수 인사()는 없음:
            출력 "안녕".
            반환.
        끝.
        실행 인사().
        """, "안녕\n");
});

await Test("한국어 문법 오류의 줄 번호", () =>
{
    ExpectError("정수 a는 0이다.\n알수없는 문장", 2);
    ExpectError("끝.", 1);
    ExpectError("아니면:", 1);
    ExpectError("만약 true이면:\n출력 1.", 1);
    ExpectError("정수 a는 1 + 이다.", 1);
    ExpectError("```csharp\nint a = 0;", 1);
    ExpectError("만약 true이면:\n아니면:\n아니면:\n끝.", 3);
    return Task.CompletedTask;
});

await Test("생성 문장의 받침 조사", () =>
{
    string korean = new CSharpToKorean().Convert("int 체력 = 3; 체력 -= 1; if (체력 == 0) { global::System.Console.WriteLine(체력); }");
    foreach (string phrase in new[] { "체력은 3이다.", "체력에서 1을 뺀다.", "체력이 0과 같으면:" })
    {
        if (!korean.Contains(phrase, StringComparison.Ordinal))
        {
            throw new Exception($"생성 문장이 다릅니다: {phrase}\n{korean}");
        }
    }
    return Task.CompletedTask;
});

await Test("의미 오류는 컴파일 검사에서 검출", () =>
{
    string source = new KoreanToCSharp().Convert("정수 a는 \"문자열\"이다.\n출력 a.");
    if (!new CSharpProgram(source).Check().Any(d => d.Severity == DiagnosticSeverity.Error))
    {
        throw new Exception("자료형 오류를 놓쳤습니다.");
    }
    return Task.CompletedTask;
});

await Test("한국어 자료형 이름과 같은 사용자 타입 보존", async () =>
{
    await ExpectCSharp("""
        정수 value = new 정수();
        System.Console.WriteLine(value.GetType().Name);
        class 정수 { }
        """, "정수\n");
});

await Test("조건 속 문자열/예약어 이스케이프", async () =>
{
    await ExpectCSharp("""
        int @if = 1;
        if ("a가 0보다 크면" == "a가 0보다 크면")
        {
            System.Console.WriteLine(@if);
        }
        """, "1\n");
});

await Test("Main 정수 종료 코드", async () =>
{
    string source = "class Program { static int Main() { return 7; } }";
    string restored = new KoreanToCSharp().Convert(new CSharpToKorean().Convert(source));
    if (await new CSharpProgram(restored).RunAsync([]) != 7)
    {
        throw new Exception("종료 코드가 다릅니다.");
    }
});

await Test("비동기 Main과 프로그램 인수", async () =>
{
    await ExpectCSharp("""
        using System.Threading.Tasks;
        class Program
        {
            static async Task Main(string[] args)
            {
                await Task.Yield();
                System.Console.WriteLine(args[0]);
            }
        }
        """, "한씨\n", ["한씨"]);
});

await Test("이름공간/클래스/필드/메서드/속성/이벤트", async () =>
{
    await ExpectProgram("""
        가져오기 System.
        이름공간 Demo:
            공개 봉인 클래스 Health:
                비공개 필드 정수 _current는 100이다.
                공개 속성 정수 Current는 _current이다.
                공개 이벤트 Action<int>? Changed.
                공개 함수 Damage(정수 amount)는 없음:
                    _current에서 amount를 뺀다.
                    실행 Changed?.Invoke(_current).
                끝.
            끝.
            공개 클래스 Program:
                공개 정적 함수 Main()은 없음:
                    자동 health는 new Health()이다.
                    실행 health.Damage(25).
                    출력 health.Current.
                끝.
            끝.
        끝.
        """, "75\n");
});

await Test("제네릭/배열 자료형과 매개변수", async () =>
{
    await ExpectProgram("""
        가져오기 System.Collections.Generic.
        함수 합계(Dictionary<string,정수> 값들)는 정수:
            반환 값들["a"].
        끝.
        정수[] 값들은 new int[] { 2, 3 }이다.
        자동 사전은 new Dictionary<string,int> { ["a"] = 값들[0] + 값들[1] }이다.
        출력 합계(사전).
        """, "5\n");
});

await Test("상속/재정의/특성의 왕복 실행", async () =>
{
    await ExpectCSharp("""
        using System;
        class Base { public virtual int Read() { return 1; } }
        [Serializable]
        sealed class Child : Base { public override int Read() { return 2; } }
        class Program { static void Main() { Console.WriteLine(new Child().Read()); } }
        """, "2\n");
});

await Test("클래스 내부 주석/생성자 원문 보존", async () =>
{
    await ExpectCSharp("""
        class Counter
        {
            // 초기 상태는 명시적으로 보관한다.
            private int _count;
            public Counter(int count) { _count = count; }
            public int Read() { return _count; }
        }
        class Program { static void Main() { System.Console.WriteLine(new Counter(3).Read()); } }
        """, "3\n");
});

await Test("Unity 스크립트 구조 컴파일 (엔진 스텁)", () =>
{
    string korean = """
        가져오기 UnityEngine.
        공개 봉인 클래스 HanSpinner는 MonoBehaviour를 상속한다:
            특성 SerializeField.
            비공개 필드 벡터3 _axis는 Vector3.up이다.
            특성 SerializeField.
            비공개 필드 단정도실수 _speed는 90f이다.
            공개 함수 Tick()는 없음:
                실행 transform.Rotate(_axis, _speed * Time.deltaTime, Space.Self).
            끝.
            공개 함수 Start()는 코루틴:
                기록 "코루틴".
                기다린다 new WaitForSeconds(1f).
                양보종료.
            끝.
        끝.
        """;
    string stub = """
        namespace UnityEngine
        {
            public class MonoBehaviour { public Transform transform = new Transform(); }
            public class SerializeField : System.Attribute { }
            public struct Vector3 { public static Vector3 up => new Vector3(); }
            public class Transform { public void Rotate(Vector3 axis, float angle, Space space) { } }
            public enum Space { Self }
            public static class Time { public static float deltaTime => 0.02f; }
            public class WaitForSeconds { public WaitForSeconds(float seconds) { } }
            public static class Debug { public static void Log(object value) { } }
        }
        """;
    string generated = new KoreanToCSharp().Convert(korean);
    CheckLibrary(generated + "\n" + stub);
    string restored = new KoreanToCSharp().Convert(new CSharpToKorean().Convert(generated));
    CheckLibrary(restored + "\n" + stub);
    return Task.CompletedTask;
});

await Test("원본 행으로 C# 진단 연결", () =>
{
    string path = Path.GetFullPath("sample.han");
    string generated = new KoreanToCSharp().Convert("정수 a는 0이다.\n정수 b는 \"문자열\"이다.", path);
    Diagnostic error = new CSharpProgram(generated).Check().First(d => d.Id == "CS0029");
    var mapped = error.Location.GetMappedLineSpan();
    if (mapped.Path != path || mapped.StartLinePosition.Line != 1)
    {
        throw new Exception("원본 파일/2행 연결 실패");
    }
    return Task.CompletedTask;
});

await Test("알 수 없는 수식어와 선언 문법 오류", () =>
{
    ExpectError("알수없는 클래스 Foo:", 1);
    ExpectError("공개 클래스 123:", 1);
    ExpectError("특성 Broken(.", 1);
    return Task.CompletedTask;
});

await Test("생성된 클래스도 한국어로 분해", () =>
{
    string korean = new CSharpToKorean().Convert("public class Sample { private int _value = 1; public int Value => _value; public void Add(int amount) { _value += amount; } }");
    foreach (string phrase in new[] { "공개 클래스 Sample:", "비공개 필드 정수 _value", "공개 속성 정수 Value", "공개 함수 Add" })
    {
        if (!korean.Contains(phrase, StringComparison.Ordinal)) throw new Exception(korean);
    }
    return Task.CompletedTask;
});

Console.WriteLine($"검증 결과: {passed} 통과 / {failed} 실패");
return failed == 0 ? 0 : 1;

async Task Test(string name, Func<Task> test)
{
    try
    {
        await test();
        passed++;
        Console.WriteLine($"통과: {name}");
    }
    catch (Exception exception)
    {
        failed++;
        Console.Error.WriteLine($"실패: {name}: {exception.Message}");
    }
}

static async Task ExpectProgram(string korean, string expected)
{
    string source = new KoreanToCSharp().Convert(korean);
    string actual = await Capture(source, []);
    Equal(expected, actual);
    string roundTrip = new KoreanToCSharp().Convert(new CSharpToKorean().Convert(source));
    Equal(expected, await Capture(roundTrip, []));
}

static async Task ExpectCSharp(string source, string expected, string[]? args = null)
{
    Equal(expected, await Capture(source, args ?? []));
    string korean = new CSharpToKorean().Convert(source);
    string restored = new KoreanToCSharp().Convert(korean);
    Equal(expected, await Capture(restored, args ?? []));
}

static async Task<string> Capture(string source, string[] args)
{
    var program = new CSharpProgram(source);
    var errors = program.Check().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0)
    {
        throw new Exception(string.Join("\n", errors.Select(d => d.ToString())) + "\n" + source);
    }
    TextWriter previous = Console.Out;
    using var writer = new StringWriter();
    try
    {
        Console.SetOut(writer);
        await program.RunAsync(args);
    }
    finally
    {
        Console.SetOut(previous);
    }
    return writer.ToString().Replace("\r\n", "\n");
}

static void Equal(string expected, string actual)
{
    if (actual != expected)
    {
        throw new Exception($"출력이 다릅니다. 기대: [{expected}], 실제: [{actual}]");
    }
}

static void ExpectError(string source, int line)
{
    try
    {
        new KoreanToCSharp().Convert(source);
    }
    catch (LanguageException exception) when (exception.LineNumber == line)
    {
        return;
    }
    throw new Exception($"{line}행 문법 오류가 필요합니다.");
}

static void CheckLibrary(string source)
{
    var errors = new CSharpProgram(source, library: true).Check().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0) throw new Exception(string.Join("\n", errors.Select(d => d.ToString())) + "\n" + source);
}
