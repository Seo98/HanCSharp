# 한씨(HanCSharp) — C#과 Unity에서 쓰는 문장형 한국어 언어

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**[Windows 실행 ZIP 다운로드](https://github.com/Seo98/HanCSharp/releases/latest/download/HanCSharp-Windows-x64.zip)** · **[Markdown 가이드 목차](docs/README.md)** · **[Unity 이동 예제](samples/UnityMovementDemo/README.md)**

한국어 문장을 C#으로 변환하고, C# 코드도 다시 한국어 문장으로 표현하는 개인 언어 프로토타입입니다. **Visual Studio 솔루션·빌드 연결·Unity 연결 코드·한씨 편집기 확장**을 포함합니다.

```text
공개 클래스 Program:
    공개 정적 함수 Main():
        정수 체력은 100이다.
        체력에서 25를 뺀다.
        출력 $"남은 체력: {체력}".
    끝.
끝.
```

## 시작

**처음이라면:** [실행 ZIP 받기](https://github.com/Seo98/HanCSharp/releases/latest) → 모두 압축 풀기 → **START-HAN.cmd** 더블클릭. 콘솔 예제가 자동으로 실행됩니다. 이어서 **② Unity 이동 예제 준비·실행**을 누르면 짧은 폴더에 프로젝트를 준비하고 이동 씬을 엽니다. Unity 6000.3.25f1은 별도 설치해야 합니다.

Windows x64 실행 ZIP에는 .NET 런타임이 포함됩니다. Unity에 설치한 뒤 Project 창에서 **우클릭 → Create → HanCSharp → 한씨 컴포넌트**로 새 원본을 만들 수 있습니다.

- [처음 사용자 단계별 가이드](docs/QUICKSTART.md)
- [GitHub 공개·배포 가이드](docs/GITHUB-PUBLISH.md)

다음은 개발용 소스를 빌드해 사용하는 방법입니다.

1. .NET 9 SDK가 있는 환경에서 **HanCSharp.sln**을 Visual Studio 2022 17.14 이상/2026으로 엽니다.
2. **ConsoleGame**을 시작 프로젝트로 설정합니다.
3. **samples/ConsoleGame/Main.han**을 수정하고 저장한 다음 **F5** 또는 **Ctrl+F5**로 실행합니다.

변환기 프로젝트를 먼저 빌드한 뒤 `.han → C# → 프로그램`으로 처리합니다. 원본 행과 C# 오류를 연결합니다. 생성 C#은 obj 폴더에 저장합니다.

- [Visual Studio 시작 안내](visualstudio/START-HERE.md)
- [자동완성·색상 강조 확장 설치](visualstudio/EDITOR-SUPPORT.md)
- [개발 가이드: 자료형·변수명·함수·Unity](docs/DEVELOPMENT-GUIDE.md)
- [Unity 원본 열기·미리보기·오류 위치](docs/UNITY-EDITING.md)
- [현재 파일용 외부 도구 등록](visualstudio/EXTERNAL-TOOLS.md)
- [Unity 설치와 사용](unity/README.md)
- [한씨로 큐브 움직이기](unity/MOVEMENT-DEMO.md)
- [실제 검증 결과와 한계](VALIDATION.md)

`visualstudio/HanCSharp-0.7.vsix`를 설치하면 `.han` 색상 강조, 키워드·현재 문서 이름·Unity 주요 멤버 완성, 문장 템플릿을 사용할 수 있습니다. **Ctrl+Space**로 목록을 열고 **Tab/Enter**로 확정합니다. 전체 C# 타입 추론과 한국어 원본 중단점은 포함하지 않습니다.

## v0.7 문법 개선

반환형 생략 함수, `체력이 0보다 큰 동안:`, `3번 반복:`, `참·거짓`, `실수32·실수64`, `조건 끝.` 등을 추가했습니다. 이전 문법도 계속 지원합니다. [새 문법과 호환 안내](docs/GRAMMAR-0.7.md) · [실행 예제](samples/LanguageTour/Main.han)

## 문장 문법

한 줄에 한 문장을 씁니다. 들여쓰기는 가독성을 위한 것이며 블록은 `끝.`으로 닫습니다. 조사 양쪽 형태를 허용합니다.

| 기능 | 예시 |
|---|---|
| 변수 | `정수 체력은 100이다.` |
| 대입/증가/감소/곱하기 | `체력에 80을 넣는다.` / `체력에 10을 더한다.` / `체력에서 5를 뺀다.` / `체력에 2를 곱한다.` |
| 출력 | `출력 체력.` |
| 조건 | `만약 체력이 0보다 크면:` → 본문 → `아니면:` → 본문 → `끝.` |
| 비교 | 크면 / 작으면 / 크거나 같으면 / 작거나 같으면 / `0과 같으면` / `0과 다르면` |
| C# 복합 조건 | `만약 체력 > 0 && 활성 == true이면:` |
| while | `체력이 0보다 큰 동안:` → 본문 → `반복 끝.` — 기존 `동안 체력이 0보다 크면:`도 허용 |
| 간단 반복 | `3번 반복:` → 본문 → `반복 끝.` — 번호가 필요 없는 정수 횟수 반복 |
| 횟수 반복 | `반복 차례는 4회:` → 본문 → `끝.` — 차례는 0부터 3까지 |
| 중단/건너뛰기 | `중단.` / `계속.` |
| 지역 함수 | `함수 더하기(정수 x, 정수 y)는 정수:` → 본문 → `끝.` |
| 반환 | `반환 x + y.` / `반환.` |
| C# 식 실행 | `실행 인사().` |
| 독립된 한 줄 주석 | `// 설명` |

## C# 선언 문법

| 기능 | 예시 |
|---|---|
| using | `가져오기 UnityEngine.` |
| 이름공간 | `이름공간 MyGame:` → 내용 → `끝.` |
| 클래스 | `공개 봉인 클래스 Player:` → 내용 → `끝.` |
| 상속 | `공개 클래스 Player는 MonoBehaviour를 상속한다:` |
| 필드 | `비공개 필드 정수 _health는 100이다.` / `공개 필드 Transform Target.` |
| 표현식 속성 | `공개 속성 정수 Health는 _health이다.` |
| 이벤트 | `공개 이벤트 Action<int> Changed.` |
| 멤버 함수 | `공개 함수 TakeDamage(정수 amount):` → 본문 → `함수 끝.` — 반환형 생략은 void |
| 정적 함수 | `공개 정적 함수 Main():` |
| 특성 | `특성 SerializeField.` / `특성 Range(0f, 10f).` — 다음 선언 바로 앞에 작성 |
| 코루틴 | `비공개 함수 Start()는 코루틴:` / `기다린다 new WaitForSeconds(1f).` / `양보종료.` |
| Unity 기록 | `기록 "시작".` → `UnityEngine.Debug.Log(...)` |

수식어: 공개, 비공개, 보호, 내부, 정적, 읽기전용, 상수, 가상, 재정의, 추상, 봉인, 부분, 비동기.

자료형:

| 한국어 | C# |
|---|---|
| 정수 / 긴정수 | int / long |
| 실수64 또는 실수 / 실수32 또는 단정도실수 / 십진수 | double / float / decimal |
| 문자열 / 문자 / 논리 / 객체 | string / char / bool / object |
| 자동 / 없음 | var / void |
| 벡터2 / 벡터3 / 회전 / 색상 | UnityEngine.Vector2 / Vector3 / Quaternion / Color |
| 코루틴 | System.Collections.IEnumerator |

배열·제네릭 안의 기본 자료형 별칭도 변환합니다. 예: `정수[]`, `Dictionary<string,정수>`. 복잡한 타입 표기는 공백 없이 적거나 원문 블록을 사용하세요. 매개변수의 ref/out·기본값, 생성자, 자동 속성, 인터페이스 선언, 예외 처리 블록 등은 아직 한국어 전용 문법이 없으며 C# 원문으로 사용할 수 있습니다.

식 내부는 C# 표기이며 한씨 식에서는 `참·거짓`도 사용할 수 있습니다. `true`, `false`, `null`, `new`, `Math.Max(...)`, `transform.Rotate(...)`, 보간 문자열 등을 그대로 사용합니다. Unity 콜백과 API 이름도 그대로 씁니다. 한국어 별칭과 같은 사용자 타입 이름은 `@정수`처럼 이스케이프하거나 원문 블록을 사용하세요.

## C# 원문 블록

한국어와 C#을 현재 위치에서 함께 사용할 수 있습니다.

````text
정수 합계는 0이다.
```csharp
foreach (int value in new[] { 1, 2, 3 })
{
    합계 += value;
}
```
출력 합계.
````

C# → 한국어는 지원하는 클래스·멤버·실행문을 문장으로 분해합니다. 주석을 포함하거나 표현할 수 없는 구문은 원문 블록으로 남깁니다. 전처리기가 있는 파일은 파일 전체를 보존합니다. 공백·괄호·조사의 원래 표기까지 복원하지는 않습니다. 모든 임의의 C# 프로그램에 대한 동등성을 보장하지 않습니다.

## 독립 CLI

Windows 실행 ZIP의 app에는 .NET 런타임이 포함됩니다. 소스에서 `--self-contained false`로 게시한 app에는 별도 .NET 9 런타임이 필요합니다. Visual Studio 예제와 변환기 소스 빌드에는 .NET 9 SDK가 필요하며 global.json에 9.0.307을 기준으로 지정했습니다. 같은 .NET 9 계열의 최신 feature SDK를 허용합니다. 기본 소스 빌드는 외부 NuGet 패키지를 요구하지 않지만 런타임 포함 배포 빌드는 공식 NuGet에서 런타임을 내려받습니다.

```powershell
.\app\HanCSharp.Cli.exe run .\examples\game.han
.\app\HanCSharp.Cli.exe to-cs 입력.han 출력.cs
.\app\HanCSharp.Cli.exe to-ko 입력.cs 출력.han
.\app\HanCSharp.Cli.exe check 입력.han --library
.\app\HanCSharp.Cli.exe check 입력.han --library --reference '어셈블리 폴더'
```

출력은 기본적으로 기존 파일을 덮어쓰지 않습니다. 의도적으로 교체할 때만 `--force`를 붙입니다. `to-cs --source-map`은 원본 행 정보를 추가합니다. check와 run의 진단은 원본 한국어 경로로 연결합니다. Unity 스크립트는 독립 콘솔 실행기로 실행하지 않고 Unity에서 컴파일·실행합니다.

## 소스 구조

- LanguageDefinition / SyntaxRules: 자료형, 이름, 식과 인수 검증.
- DeclarationSyntax: 클래스·멤버 등 선언 문법.
- KoreanToCSharp / CSharpToKorean: 양방향 소스 변환.
- CSharpProgram: C# 컴파일 검사 및 콘솔 실행.
- CLI: 명령과 파일 입출력.
- build/HanCSharp.targets: Visual Studio 및 MSBuild 생성 단계.
- unity/Editor: Inspector 설정, 외부 프로세스 실행, 생성 파일 관리, import 콜백, 편집기 연결.

모든 코드는 프로젝트 안에 있어 직접 확장할 수 있습니다. 원본 C# 분석은 Roslyn을 사용합니다. 참조는 Directory.Build.targets에서 실제 .NET SDK 경로로 연결하여 dotnet CLI와 Visual Studio의 경로 차이를 처리합니다. 한국어 문장 파서는 아직 패턴 기반이며, 문법이 더 커지면 별도의 언어 구문 트리로 분리하는 것이 다음 설계 과제입니다.

참고한 공개 자료: [엄랭 사양·실행기 저장소](https://github.com/rycont/umjunsik-lang), [Microsoft Roslyn 구문 모델](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/work-with-syntax). 엄랭 문법이나 구현 소스를 가져온 것은 아니며, 한씨는 별도 문장형 언어입니다.
