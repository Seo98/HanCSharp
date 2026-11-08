# Visual Studio에서 한씨 작성하기

## 처음 실행

1. Visual Studio 2022 17.14 이상 또는 Visual Studio 2026에서 루트의 **HanCSharp.sln**을 엽니다. .NET 9 SDK가 필요합니다.
2. 솔루션 탐색기에서 **samples → ConsoleGame**을 우클릭해 **시작 프로젝트로 설정**합니다.
3. [한씨 편집 확장](EDITOR-SUPPORT.md)을 설치하고 **Main.han**을 열어 한국어 코드를 수정합니다.
4. 저장하고 **F5**를 누릅니다. 디버깅 없이 실행하려면 **Ctrl+F5**를 사용합니다.
5. `.han → obj/Debug/net9.0/HanGenerated/Main.g.cs → C# 컴파일 → 실행` 순서로 처리됩니다.

언어 변환기 소스를 수정한 경우에도 ConsoleGame의 프로젝트 참조가 변환기를 먼저 빌드합니다. 예제에서 확인할 때는 app 폴더를 수동으로 갱신할 필요가 없습니다. 독립 CLI나 Unity에 전달할 빌드는 다음 명령으로 갱신합니다.

```powershell
dotnet publish src/HanCSharp.Cli -c Release -o app --self-contained false
```

## 첫 코드

```text
공개 클래스 Program:
    공개 정적 함수 Main()은 없음:
        정수 체력은 100이다.
        체력에서 25를 뺀다.
        출력 $"남은 체력: {체력}".
    끝.
끝.
```

새 `.han` 파일은 ConsoleGame 프로젝트 폴더에 추가합니다. 하위 폴더도 지원합니다. Main 진입점은 프로그램에 하나만 남기세요. C# 파일도 같은 프로젝트에서 함께 사용할 수 있습니다.

일반 C# 프로젝트에 연결하려면 프로젝트 파일에서 `build/HanCSharp.targets`를 Import하고 `HanCompilerPath`에 배포된 `app/HanCSharp.Cli.dll`의 경로를 지정하면 됩니다. 예제 프로젝트를 복사해 시작하는 편이 쉽습니다.

## 오류 확인

Visual Studio 오류 목록에 한국어 원본 경로와 행을 표시하도록 `#line` 매핑을 넣습니다. 문장 자체가 틀리면 `HCS001`, 자료형 등 C# 오류는 `CS0029` 같은 원래 진단을 사용합니다. 저장한 `.han` 변경을 빌드에서 놓치지 않도록 Visual Studio의 빠른 최신 상태 검사를 이 프로젝트에서 비활성화했습니다.

## 현재 편집 기능의 범위

- Visual Studio 솔루션에서 `.han` 열기·편집, 저장 후 빌드·실행 경로를 제공합니다.
- 편집 확장 설치 후 색상 강조와 자동완성·문장 템플릿을 제공합니다. Ctrl+Space로 호출하고 Tab/Enter로 확정합니다.
- 완성은 키워드·현재 문서 이름·Unity 주요 멤버 목록을 사용합니다. 전체 타입 추론·리팩터링·한국어 소스 중단점 지원은 포함하지 않습니다.
- 생성 C#은 실제 C# 코드이며, 파일을 열면 C# 편집 기능을 사용할 수 있습니다. 생성 파일 수정은 다음 빌드에서 교체되므로 원본 `.han`을 수정하세요.
- 실제 Visual Studio 2022의 `/Build Debug`에서 빌드를 검증했습니다. F5 버튼 클릭이나 디버거의 한국어 중단점은 검증하지 않았습니다.

파일 하나를 별도로 변환·검사하려면 [외부 도구 등록 안내](EXTERNAL-TOOLS.md)를 참고하세요.

자료형, 변수명 정의와 명명 규칙, 조건·함수, Unity 작성법은 [개발 가이드](../docs/DEVELOPMENT-GUIDE.md)에 있습니다.
