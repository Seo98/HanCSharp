# 처음 쓰는 한씨 — 다운로드부터 Unity까지

한씨는 한국어 문장으로 작성한 `.han`을 C#으로 바꾸는 도구입니다. Unity에서는 생성된 C#을 실행합니다.

## 1. 설치 없이 먼저 실행해 보기 (Windows x64)

1. [GitHub Releases](https://github.com/Seo98/HanCSharp/releases/latest)에서 **HanCSharp-Windows-x64.zip**을 받습니다.
2. ZIP을 **모두 압축 풀기**로 풉니다. ZIP 안에서 바로 실행하지 마세요.
3. **START-HAN.cmd**를 더블클릭합니다.
4. 열린 창에 예제 결과가 자동으로 표시됩니다. **① 한씨 예제 바로 실행**을 누르면 다시 실행할 수 있습니다.

체력이 `75 → 50 → 25 → 0`으로 줄어드는 결과가 표시되면 성공입니다. 이 단계는 Unity·Visual Studio·별도 .NET 설치 없이 작동하도록 Windows x64 런타임을 포함합니다. Windows 10/11 x64용이며 macOS/Linux/ARM은 이 실행 ZIP의 대상이 아닙니다.

GitHub의 **Code → Download ZIP**은 개발용 소스입니다. 실행 파일을 포함한 **Releases ZIP**과 다릅니다. 소스 ZIP만 받았다면 아래 개발자 절을 참고하세요.

## 2. Unity에 설치하기

전제: Unity 프로젝트가 이미 있어야 합니다. 실제 확인한 버전은 Unity 6000.3.25f1입니다. Unity 자체의 다운로드·설치나 라이선스 활성화는 한씨 도구가 대신하지 않습니다.

1. 시작 창의 **② Unity 프로젝트에 설치**를 누릅니다.
2. `Assets`, `Packages`, `ProjectSettings`가 있는 **프로젝트 맨 위 폴더**를 선택합니다. `Assets` 폴더 자체를 고르지 마세요.
3. 설치 완료 후 Unity로 돌아가 컴파일을 기다립니다.
4. Project 창의 Assets 안에서 **우클릭 → Create → HanCSharp → 한씨 컴포넌트**를 선택합니다.
5. 파일 이름을 `MyFirstHan`으로 입력하고 Enter를 누릅니다.
6. 잠시 후 `MyFirstHan.han`과 생성된 `MyFirstHan.cs`가 생깁니다.
7. Hierarchy에서 Cube를 만들고 **생성된 MyFirstHan.cs**를 Cube에 드래그합니다.
8. Play를 누르면 Console에 `한씨 컴포넌트가 시작되었습니다!`가 표시됩니다.

원본은 `.han`, 실행에 사용되는 결과는 `.cs`입니다. 앞으로는 **.han을 수정하고 저장 → Unity로 돌아오기 → 컴파일 기다리기** 순서로 사용하세요. 같은 이름의 사용자 C#이 있으면 보호를 위해 생성을 거부합니다. 자동 생성이 꺼져 있으면 **Tools → HanCSharp → Generate All**을 사용하세요.

Unity에서 한씨가 생성한 `.cs`를 더블클릭하거나 컴포넌트의 Script를 열면 대응하는 `.han` 원본으로 연결합니다. 일반 사용자 작성 C#에는 적용하지 않습니다. Visual Studio에 이미 열려 있는 `.cs` 탭은 자동 교체되지 않으므로 해당 탭을 닫고 `.han`을 여세요.

생성 C#은 기본적으로 `#line` 표시 없이 만듭니다. **Tools → HanCSharp → Settings → Emit Source Map**을 켜고 **Generate All**을 실행하면 원본 오류 행 연결을 다시 사용할 수 있습니다. 끈 상태의 C# 컴파일 오류는 생성 C# 기준입니다. 오류의 구체적인 C# 행을 여는 동작은 정확한 위치를 유지하도록 원본으로 가로채지 않습니다.

클래스명은 파일명과 같습니다. `MyFirstHan`, `플레이어이동`, `Player2`처럼 공백 없이 정하고, 숫자로 시작하거나 `class` 같은 C# 예약어를 사용하지 마세요. **생성 이후 파일명을 바꾸면 클래스명도 직접 맞춰야 합니다.** 자동 리팩터링 기능은 없습니다.

## 3. Visual Studio 자동완성 켜기

1. Visual Studio 문서를 저장하고 창을 닫습니다.
2. 시작 창의 **③ Visual Studio 확장 설치 열기**를 누릅니다.
3. VSIX 설치 관리자의 안내를 따르고 Visual Studio를 다시 엽니다.
4. Unity에서 **Tools → HanCSharp → Include Han Files In Solution**, 이어서 **Assets → Open C# Project**를 선택합니다.
5. `.han`을 열어 `변수` 입력 → **Ctrl+Space → Tab**을 눌러 봅니다.

자동완성은 키워드·현재 문서 이름·주요 Unity API 목록을 제공합니다. 완전한 C# 타입 추론은 포함하지 않습니다. 실제 확인한 IDE는 Visual Studio 2022이며 2026 대상은 미검증입니다. VSIX 설치 관리자가 실행되지 않으면 [사용자 폴더 배포 안내](../visualstudio/EDITOR-SUPPORT.md)를 이용하세요. 확장이 사용 안 함 상태면 확장 관리에서 활성화 후 재시작합니다.

## 4. 큐브를 움직이고 싶다면

실행 ZIP의 `samples/UnityMovementDemo`를 Unity Hub에 등록하고 `Assets/HanCSharp/Scenes/MovementDemo.unity`를 여세요. 이 프로젝트에는 입력 참조와 카메라·바닥·큐브 프리팹이 준비되어 있습니다.

[키보드 이동 예제](../unity/MOVEMENT-DEMO.md)를 따라 `HanKeyboardInput`, `HanObjectMover`를 추가하고 Input 참조를 연결합니다. **Game 화면 클릭 → WASD/방향키**로 조작합니다. Move Speed는 Inspector에서 조절합니다. [.han 문법·변수명 가이드](DEVELOPMENT-GUIDE.md)에서 타입과 이름 규칙을 볼 수 있습니다.

## 문제가 생겼을 때

| 증상 | 확인 |
|---|---|
| START-HAN 실행 파일 없음 | Releases ZIP을 받고 모두 압축을 풀었는지 확인 |
| Unity 프로젝트 폴더가 아니라고 나옴 | Assets가 들어 있는 상위 폴더 선택 |
| 기존 파일과 다르다고 설치 중단 | 사용자 수정 보호 기능. 백업·비교 후 직접 갱신. 자동 덮어쓰기하지 않음 |
| Create 메뉴가 없음 | Console 컴파일 오류 확인. Editor 스크립트가 Assets/HanCSharp/Editor에 있는지 확인 |
| .cs가 생성되지 않음 | Unity로 돌아오기, Auto Generate 설정, Generate All, Console 오류 확인 |
| 편집기에 한씨 파일이 없음 | Include Han Files In Solution 후 프로젝트 다시 열기 |
| 색상/완성이 없음 | VS 확장 설치 및 사용 상태 확인. .han 확장자 확인 |
| 큐브가 안 움직임 | Play·Game 포커스·Input 참조·Move Speed 확인 |

배포본에는 관리자 권한이나 시스템 설정 변경이 필요하지 않습니다. 조직의 실행 정책으로 스크립트가 차단되면 담당 관리자에게 문의하세요. Windows 보안 경고를 자동으로 우회하는 기능은 제공하지 않습니다.

## 개발용 소스에서 시작하기

.NET 9 SDK와 Visual Studio 2022를 준비하고 `HanCSharp.sln`을 엽니다. ConsoleGame을 시작 프로젝트로 설정하고 F5로 실행합니다. CLI만 빌드하려면:

```powershell
dotnet publish src/HanCSharp.Cli -c Release -o app --self-contained false
```

GitHub용 실행 ZIP 만들기는 [배포 가이드](GITHUB-PUBLISH.md)를 참고하세요.
