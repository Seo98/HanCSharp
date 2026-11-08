# 공개 배포본 첫 다운로드 검증

검증일: 2026-11-08. 대상: GitHub v0.4.0의 `HanCSharp-Windows-x64.zip`.

**판정: 짧은 경로에서 설치 → 한씨 생성 → Unity 예제 Play의 핵심 흐름은 통과했다. 긴 경로에서 Unity 패키지 가져오기 오류가 재현되어 설치 위치 안내가 필요하다.**

## 배포본과 환경

- GitHub 공개 다운로드 URL에서 인증 없이 ZIP과 SHA256SUMS.txt를 새로 받았다. 로컬 기존 배포 폴더를 대신 사용하지 않았다.
- ZIP SHA-256: `bac70d1b9ee6131402c40be7cb248f7b9869e7be1bdd766d0592d9ced126ea38`. 공개 체크섬과 일치했다.
- Windows x64, Unity 6000.3.25f1, URP 17.3.0, Input System 1.20.0.
- 새 압축 해제 폴더, 한씨가 없는 새 3D URP 프로젝트, 배포 ZIP에 포함된 Unity 프로젝트를 각각 사용했다.
- 이 PC에 Unity와 SDK는 이미 설치되어 있다. 새 PC/새 Windows 사용자/깨끗한 가상 머신의 검증은 아니다.

## 통과한 항목

| 항목 | 확인 결과 |
|---|---|
| 다운로드 무결성 | 공개 SHA-256과 일치 |
| 독립 예제 실행 | 한글·공백 경로에서 체력 75/50/25/0과 완료 메시지, 종료 코드 0 |
| 별도 .NET 실행 경로 | PATH에서 dotnet을 제외하고 DOTNET_ROOT를 없는 경로로 둔 실행 통과 |
| 시작 창 | '한씨 시작하기' 창 생성, 초기 stderr 오류 없음 |
| 새 Unity 설치 | Editor 코드·예제·런타임 포함 변환기 복사 성공 |
| 재설치 | 같은 버전 재실행 성공 |
| 입력 설정 보존 | ProjectSettings.asset 설치 전후 해시 동일 |
| 폴더 선택 오류 | Assets 자체를 선택하면 명확히 거부 |
| 사용자 수정 보호 | 기존 Editor 파일에 변경을 넣었을 때 덮어쓰지 않고 설치 거부 |
| Unity 자동 생성 | 새 프로젝트에서 한씨 예제의 C# 생성과 컴파일 성공 |
| 런타임 포함 변환기 | Unity 설정의 dotnet 경로를 존재하지 않는 이름으로 바꿔도 EXE로 실제 재생성 성공 |
| Create 메뉴 | 실제 이름 입력 UI에서 FirstDownloadComponent.han 생성, 대응 C# 자동 생성·컴파일 |
| 컴포넌트 실행 | 생성 타입을 큐브에 연결하고 Play에서 '한씨 컴포넌트가 시작되었습니다!' 출력 |
| 이동 | 새 설치 프로젝트와 배포본 Unity 프로젝트 양쪽에서 아래 입력 검증 통과 |

이동은 Input System 이벤트를 실제 Play Mode에 전달해 검증했다. WASD 4방향, 방향키 4방향, W+D 대각선의 방향·이동, 각 키 해제 후 정지, 입력 컴포넌트를 비활성화했을 때 정지를 확인했다. 대각선 입력은 `(0.707, 0.707)`로 길이가 1이었다. 실제 물리 키보드 장시간 조작과는 구분한다.

## 발견한 문제와 대응

### 1. 깊은 경로에서 Unity URP 가져오기 오류

배포 ZIP을 깊은 하위 폴더에 풀어 포함된 프로젝트를 열자, 전체 경로 길이가 **263자**인 URP `RestirCandidateTemporal.urtshader`에서 `DirectoryNotFoundException`이 발생했다. 파일 자체는 존재했다. 동일한 Assets/Packages/ProjectSettings/Tools를 더 짧은 경로로 복사해 다시 열자 이 오류 없이 가져오기·컴파일·Play 검증을 통과했다.

권장: ZIP은 `C:\HanCSharp` 같은 짧은 경로에 풀고, 예제 프로젝트는 그 안의 `samples\UnityMovementDemo`를 연다. 길게 중첩된 다운로드 폴더나 프로젝트 이름을 피한다. 다른 패키지/Unity 버전까지 안전한 정확한 최대 경로 길이를 보장하는 검증은 아니다.

### 2. 실행을 막지 않는 컴파일 경고

- 새 프로젝트에 모든 예제를 설치하면 HealthState의 nullable 문맥 경고 CS8669가 발생했다.
- Unity 6.3에서 EditorUtility.InstanceIDToObject 사용에 대한 CS0618 경고가 발생했다.

두 경고 모두 한씨 컴파일 및 이동 실행을 막지 않았다. 이번 검증에서는 공개 v0.4.0 자체를 확인하기 위해 런타임 소스를 고치지 않았다.

### 3. Unity 및 검증 도구의 초기화 진단

첫 프로젝트 시작에 URP Material upgrade 안내가 표시됐다. 확인 후 가져오기를 기다려야 한다. 컴파일/도메인 재로딩 중에는 검증용 Unity Pipeline 연결이 잠시 끊겼다.

짧은 경로 예제에서 첫 Play 무렵 `UnityEditor.Search.SearchDatabase`의 초기 검색 인덱스 생성 중 ArgumentOutOfRangeException을 한 번 관측했다. 스택에 한씨 코드는 없었으며 이후 이동 검증은 통과했고 오류가 재발하지 않았다. 이를 한씨 런타임 오류로 분류하지 않았지만, '처음부터 Console에 어떠한 오류도 없었다'고 주장하지 않는다.

한글 경로에서는 검증용 Pipeline eval의 Assembly.CodeBase 읽기에서도 Illegal byte sequence 오류가 발생했다. 이 오류는 제어 도구 경로에 있으며 한씨 변환기/게임 입력의 성공 증거와 분리한다.

## 아직 확인하지 못한 것

- GUI 시작 창의 버튼을 컴퓨터 사용 도구로 직접 클릭하는 전체 흐름. PowerShell 소유 창이 도구 목록에 노출되지 않았다. 실제 창 생성은 확인했고, 예제 실행·설치는 그 버튼이 호출하는 동일 스크립트/CLI 경로로 검증했다.
- 새 Windows 설치에서의 SmartScreen/조직 정책, Unity 설치·로그인·라이선스 활성화.
- Visual Studio 확장을 처음 설치하는 새 사용자 환경. 기존 확장을 지워 새로 설치하지 않았다.
- 다른 Unity 버전, macOS/Linux/ARM, 실제 기기·플레이어 빌드, 장시간 물리 키보드 조작.

상세 시작 절차는 [처음 사용자 가이드](QUICKSTART.md)를 참고한다. 검증 대상 v0.4.0 ZIP의 바이트와 체크섬은 변경하지 않고, 발견 사항은 이 문서와 온라인 가이드에 반영했다.
