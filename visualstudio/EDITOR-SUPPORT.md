# 한씨 Visual Studio 확장 0.7

## 설치

Visual Studio 문서를 저장하고 모든 Visual Studio 창을 닫습니다. 다음 방법 중 하나를 사용합니다.

**일반 VSIX 설치:** 이 폴더의 `HanCSharp-0.7.vsix`를 열고 대상 Visual Studio를 선택합니다.

**사용자 폴더 배포:** VSIX 설치 관리자가 실행되지 않는 환경에서는 루트 폴더에서 다음 명령을 사용합니다.

```powershell
.\editor-support\Install-Extension.ps1 -Version 2022
# 설치된 2022와 2026 모두에 배포하려면:
.\editor-support\Install-Extension.ps1 -Version All
```

이 스크립트는 자신의 확장 ID를 확인한 폴더에만 파일을 배포합니다. 실행 중인 Visual Studio를 자동으로 닫지 않습니다. 기존 MEF 캐시는 같은 프로필의 백업 폴더로 옮기고 다시 생성합니다. 다른 확장과 편집기 설정은 삭제하지 않습니다. 확장 코드는 `.han` 문서에만 적용됩니다.

사용자 폴더에 처음 배포하면 Visual Studio가 확장을 비활성 상태로 둘 수 있습니다. **확장 → 확장 관리 → 설치됨 → 한씨 언어 도구 → … → 사용**을 선택하고 Visual Studio를 닫았다가 다시 여세요. 파일 복사나 목록 등록만으로 실제 활성화 성공을 판단하지 않습니다.

## 제공 기능

| 기능 | 범위 |
|---|---|
| 색상 강조 | 키워드, 자료형, 숫자, 문자열, 주석, 기호, C# 원문 구분자 |
| 일반 완성 | 한씨 키워드, 자료형, 수식어, C#/Unity 주요 이름 |
| 문서 이름 | 현재 문서에 선언한 변수·필드·속성·함수·클래스·매개변수 |
| Unity 멤버 | transform, Time, Vector2/3, Mathf, Debug, gameObject의 수동 목록 |
| 문장 템플릿 | 변수, 조건문, 반복, 함수, 클래스, 콘솔, Unity 컴포넌트·필드·코루틴, C# 원문 |
| 입력 보조 | Ctrl+Space, Tab/Enter 확정, Escape 닫기, 첫 이름 선택, 현재 줄 들여쓰기 적용 |
| 표시 설정 | Visual Studio의 기존 분류와 테마 색상 사용 |

예를 들어 `변수`를 입력하고 **변수 선언**을 선택하면 `정수 체력은 100이다.`를 삽입하고 `체력`을 선택합니다. `Time.` 뒤에서는 `deltaTime` 등을 제안합니다. 기본 완성이 바로 뜨지 않으면 **Ctrl+Space**를 사용하세요.

문자열과 주석에서는 완성을 억제합니다. WPF 텍스트 조합 이벤트로 IME 조합 중 확정을 피하도록 처리했지만, 모든 입력기·키보드 설정의 실제 조합을 검증한 것은 아닙니다.

## 한계

이 확장은 C# Roslyn 의미 분석 서버를 구현한 것이 아닙니다. 문서의 이름을 수집하되 실제 범위·타입을 완전히 추론하지 않습니다. 다른 파일의 선언, 임의 객체의 모든 멤버, 오버로드 서명 도움말·리팩터링·코드 진단은 포함하지 않습니다. 컴파일 진단은 기존 한씨/MSBuild 연결을 사용합니다.

색상은 표시용 토큰화 결과입니다. C# 원문 블록도 기본 문자열·주석·키워드를 구분하지만, C# 전체 편집 서비스가 해당 블록 안에 내장되는 것은 아닙니다. 생성 C# 파일에서는 Visual Studio의 C# 서비스를 사용할 수 있습니다.

## 확장 소스와 검증

- `editor-support/Shared/HanLexer.cs`: 표시용 토큰과 억제 위치.
- `CompletionCatalog.cs`: 항목·문장 템플릿·문서 이름·Unity 목록.
- `HanClassifier.cs`: Visual Studio 색상 분류에 연결.
- `HanCompletionSource.cs`: 현재 버퍼의 완성 목록.
- `HanCommandFilter.cs`: 편집기 명령과 세션, IME 이벤트와 구독 해제.

```powershell
dotnet run --project editor-support/EditorSupport.Tests
.\editor-support\Build-Extension.ps1
```

소스 빌드에는 Visual Studio 2022의 에디터 어셈블리, .NET Framework 4.7.2 개발용 참조와 .NET 9 SDK가 필요합니다. 확장 실행은 Visual Studio의 .NET Framework 환경을 사용하며 한씨 CLI 프로세스와 분리됩니다. SDK DLL은 VSIX 안에 복사하지 않습니다.

정확한 실제 검증 결과는 [VALIDATION.md](../VALIDATION.md)를 확인하세요. 문법·이름 규칙은 [개발 가이드](../docs/DEVELOPMENT-GUIDE.md)에 있습니다.

참고: [Microsoft의 완성 확장 구현](https://learn.microsoft.com/en-us/visualstudio/extensibility/walkthrough-displaying-statement-completion), [MEF 캐시 설명](https://github.com/microsoft/VSProjectSystem/blob/master/doc/overview/mef.md).
