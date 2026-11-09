# 처음 쓰는 한씨 — v0.6.0

한씨는 `.han` 한국어 문장을 일반 C#으로 변환합니다. Unity 게임에는 별도 언어 해석기를 넣지 않습니다.

## 1. 다운로드와 첫 실행

1. [GitHub Releases](https://github.com/Seo98/HanCSharp/releases/latest)의 **HanCSharp-Windows-x64.zip**을 받습니다.
2. ZIP을 **모두 압축 풀기**로 풉니다. ZIP 내부에서 바로 실행하지 마세요.
3. **START-HAN.cmd**를 더블클릭합니다. 또는 `launcher/HanCSharp.Start.exe`를 실행합니다.
4. 시작 창에 `체력 75 → 50 → 25 → 0` 결과가 자동 표시되면 성공입니다.

Windows x64용입니다. 콘솔 변환기에는 .NET 런타임이 포함됩니다. 시작 창은 Windows의 .NET Framework 4.7.2 이상을 사용합니다(업데이트된 Windows 10/11에 포함). 별도 .NET SDK는 필요하지 않습니다. Unity와 Visual Studio 자체는 별도 설치 대상입니다.

GitHub **Code → Download ZIP**은 개발용 소스이며 실행 파일을 포함하지 않습니다. 처음 사용자는 Releases ZIP을 선택하세요.

## 2. Unity 이동 예제를 가장 쉽게 실행하기

전제: Unity 6000.3.25f1이 설치되어 있어야 합니다. Unity 설치·로그인·라이선스 활성화는 한씨 도구가 대신하지 않습니다.

1. 시작 창에서 **② Unity 이동 예제 준비·실행**을 누릅니다.
2. 도구가 `문서/HanCSharp/MovementDemo-0.6.0` 같은 짧은 경로에 프로젝트와 변환기를 준비합니다. 다운로드 ZIP이 깊은 경로에 있어도 실제 Unity 프로젝트는 이 별도 경로에 만들어집니다.
3. 설치된 해당 Unity를 찾으면 자동으로 엽니다. 찾지 못하면 `Editor/Unity.exe` 선택 창이 뜹니다. Unity Hub에서 원하는 에디터를 먼저 설치하거나 준비된 프로젝트를 Hub에 등록할 수도 있습니다.
4. 최초 패키지 다운로드와 컴파일을 기다립니다. 이동 씬이 자동으로 열립니다.
5. **Play → Game 화면 클릭 → WASD 또는 방향키**로 청록색 큐브를 움직입니다.
6. Hierarchy의 `HanPlayer`를 선택하고 Inspector의 **Han Object Mover → Move Speed**로 속도를 바꿉니다.

예제 저장 위치를 바꾸려면 **예제 저장 위치 선택** 버튼을 사용하세요. 도구는 80자 이하의 프로젝트 경로를 사용합니다. 기존 폴더를 덮어쓰지 않으며, 같은 버전의 도구가 이미 준비한 예제를 다시 열 때는 사용자 코드 변경을 유지합니다.

v0.5부터 Unity 프로젝트는 실행 ZIP 안의 `templates/UnityMovementDemo.zip`에 압축 보관됩니다. 이 템플릿을 다운로드 폴더에 직접 풀어 Unity에서 열기보다 시작 창의 준비 버튼을 사용하세요. `Library/Temp/Logs`를 옮기거나 Windows 시스템 설정을 바꿀 필요는 없습니다.

**범위:** Unity 엔진 자체의 긴 경로 제약을 변경하는 기능은 아닙니다. 새 예제의 실제 경로를 짧게 준비해 재현된 URP 경로 문제를 피합니다. 이미 사용 중인 긴 경로의 기존 프로젝트는 자동 이동하지 않습니다.

## 3. 기존 Unity 프로젝트에 설치하기

1. **③ 기존 Unity 프로젝트에 설치**를 누릅니다.
2. `Assets`, `Packages`, `ProjectSettings`가 있는 프로젝트 맨 위 폴더를 고릅니다. Assets 자체를 선택하지 마세요.
3. Unity로 돌아가 컴파일을 기다립니다.
4. Project 창에서 **우클릭 → Create → HanCSharp → 한씨 컴포넌트**를 선택합니다.
5. `MyFirstHan`처럼 공백 없이 이름을 입력하고 Enter를 누릅니다. 같은 이름이 이미 있다면 다른 이름을 사용하세요.
6. `.han`과 생성된 `.cs`가 생기면 `.cs`를 Cube 같은 GameObject에 연결합니다.
7. Play 시 Console에 `한씨 컴포넌트가 시작되었습니다!`가 표시됩니다.

기존 파일의 내용이 설치 파일과 다르면 사용자 변경 보호를 위해 중단합니다. v0.4에서 직접 관리하던 프로젝트를 갱신할 때는 먼저 백업하고 `unity/Editor`의 변경 파일을 비교해 교체하세요. 자동으로 사용자 코드나 기존 프로젝트 위치를 바꾸지 않습니다.

## 4. 수정과 자동완성

Project 창의 청록색 **한** 아이콘이 원본입니다. 선택 후 Inspector의 **한씨 원본 열기** 또는 **변환 미리보기**를 누르세요. 미리보기의 왼쪽은 저장된 원본, 오른쪽은 생성 C#입니다. 문법 오류가 있으면 **오류 행 열기**를 누릅니다.

원본 **.han을 수정 → 저장 → Unity로 돌아오기 → 컴파일 기다리기** 순서로 사용합니다. 생성된 C#을 직접 수정하면 다음 변환에서 바뀝니다. 직접 작성한 별도 C#은 함께 사용할 수 있습니다.

Unity에서 한씨 생성 C#을 일반 열기하면 원본 한씨로 연결합니다. Visual Studio에 이미 열려 있는 `.cs` 탭은 자동 교체되지 않습니다. 생성 C#은 기본적으로 `#line` 표시를 생략합니다. 원본 오류 행 연결이 필요하면 **Tools → HanCSharp → Settings → Emit Source Map**을 켜고 **Generate All**을 실행하세요. v0.6부터 미리보기의 **Unity C# 오류 목록 → 한씨 N행 열기**로 원본 오류 행을 엽니다. 기본 설정에서 Unity Console 자체는 생성 C#을 열 수 있습니다. [원본 열기·미리보기·오류 위치 안내](UNITY-EDITING.md)를 참고하세요.

Visual Studio 자동완성 설치:

1. Visual Studio 문서를 저장하고 창을 닫습니다.
2. 시작 창의 **Visual Studio 확장 설치**를 누르고 VSIX 설치 안내를 따릅니다.
3. Unity에서 **Tools → HanCSharp → Include Han Files In Solution**, 이어서 **Assets → Open C# Project**를 사용합니다.
4. `.han`에서 `변수` 입력 → **Ctrl+Space → Tab**으로 문장 템플릿을 넣어 보세요.

VSIX 설치 관리자가 실행되지 않으면 [사용자 폴더 배포 안내](../visualstudio/EDITOR-SUPPORT.md)를 사용합니다. 실제 확인한 IDE는 Visual Studio 2022입니다. 전체 C# 타입 추론·리팩터링은 포함하지 않습니다.

## 문제 확인

| 증상 | 확인 |
|---|---|
| 시작 실행 파일 없음 | Releases ZIP을 모두 풀었는지 확인 |
| Unity를 찾지 못함 | 설치한 해당 버전의 Editor/Unity.exe를 선택하거나 Hub에서 프로젝트 추가 |
| 저장 위치 거부 | 짧은 새 경로 선택. 다른 사용자 폴더는 덮어쓰지 않음 |
| 한씨 생성 메뉴 없음 | Unity 컴파일 완료 및 Console 오류 확인 |
| C# 자동 생성 안 됨 | Unity 복귀, Auto Generate 설정, Tools → HanCSharp → Generate All |
| 큐브가 안 움직임 | Play, Game 포커스, Input 참조, 양수 Move Speed 확인 |
| 다른 경로에서 URP 파일 오류 | 시작 창으로 예제를 새로 준비. 기존 프로젝트는 짧은 경로로 별도 복사·검증 |

## 더 보기

- [개발·변수명 가이드](DEVELOPMENT-GUIDE.md)
- [C# 혼용](CSHARP-INTEROP.md)
- [이동 코드 설명](../unity/MOVEMENT-DEMO.md)
- [GitHub 배포 가이드](GITHUB-PUBLISH.md)
- [검증 기록](RELEASE-CHECK.md)

소스 빌드는 .NET 9 SDK 및 Windows용 .NET Framework 4.7.2 참조가 필요합니다. `tools/Build-Launcher.ps1`로 시작 도구를, `tools/Build-Release.ps1`로 전체 Windows 배포본을 만듭니다.
