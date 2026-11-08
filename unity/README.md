# Unity 연결

한씨는 Unity 게임 안에서 별도의 인터프리터를 돌리지 않습니다. **에디터에서 한국어 소스를 일반 C#으로 만들고, Unity가 해당 C#을 컴파일**합니다. 배포 게임에는 한씨 CLI나 .NET 9 런타임을 넣지 않습니다.

## 설치

Windows 실행 ZIP에서는 **START-HAN.cmd → ③ 기존 Unity 프로젝트에 설치**를 눌러 프로젝트 루트를 고릅니다. 이동 예제만 먼저 체험하려면 **② Unity 이동 예제 준비·실행**을 누르세요.

명령줄에서는 루트의 Install-Unity.ps1에 기존 Unity 프로젝트 경로를 전달합니다. 프로젝트는 자동 선택하지 않습니다. 소스 체크아웃은 `tools/Build-Launcher.ps1`로 시작 프로그램도 먼저 빌드해야 합니다.

```powershell
.\Install-Unity.ps1 -ProjectPath 'C:\내프로젝트\UnityGame' -IncludeExamples
```

- `Tools/HanCSharp/`: 외부 변환기. **Assets 밖**에 설치하여 .NET 9 DLL을 Unity가 직접 로드하지 않도록 합니다.
- `Assets/HanCSharp/Editor/`: 설정·변환·가져오기 콜백·Visual Studio 연결 코드.
- `Assets/HanCSharp/Examples/`: 선택한 경우 .han 예제 5개.

스크립트는 복사할 파일을 먼저 검사합니다. 내용이 다른 기존 파일이 있으면 복사하지 않고 중단합니다. 같은 버전의 재실행은 허용합니다. 기존 설치를 수정한 경우에는 변경점을 검토해 수동으로 갱신하세요. 런타임 기본 클래스나 프로젝트 입력 설정을 변경하지 않습니다.

## 첫 사용

Windows 실행 배포본에서는 `START-HAN.cmd`의 **Unity 프로젝트에 설치** 버튼으로 프로젝트 폴더를 고를 수 있습니다. 설치 후 Project 창에서 **우클릭 → Create → HanCSharp → 한씨 컴포넌트**를 선택하면 이름 입력과 기본 Start/Update 한씨 파일 생성을 제공합니다. 이름이 같은 사용자 C#은 덮어쓰지 않습니다. 자세한 절차는 [처음 사용자 가이드](../docs/QUICKSTART.md)를 보세요.

1. Unity 컴파일이 끝나면 **Tools → HanCSharp → Settings**를 선택합니다.
2. Inspector에서 Dot Net Path와 Compiler Path를 확인합니다. 기본값은 `dotnet`, `Tools/HanCSharp/HanCSharp.Cli.dll`입니다.
3. **Tools → HanCSharp → Generate All**로 처음 한 번 생성합니다.
4. **Tools → HanCSharp → Include Han Files In Solution**을 선택합니다. 기존 프로젝트 확장자를 유지하며 `han`을 추가합니다.
5. Unity의 **Assets → Open C# Project**로 솔루션을 다시 생성합니다. 외부 스크립트 편집기는 Visual Studio를 선택해 두세요.
6. `.han`을 편집하고 저장한 다음 Unity로 돌아옵니다. Unity가 파일을 가져올 때 같은 폴더의 `.cs`가 갱신됩니다.

Project 창에서 `.han`을 열면 설정된 외부 편집기로 전달하는 콜백도 포함했습니다. Unity 버전 및 Visual Studio 연동 패키지에 따른 실제 동작은 해당 프로젝트에서 확인해야 합니다.

## 컴포넌트 예제

`HanSpinner.han`:

```text
가져오기 UnityEngine.

공개 봉인 클래스 HanSpinner는 MonoBehaviour를 상속한다:
    특성 SerializeField.
    비공개 필드 단정도실수 _degreesPerSecond는 90f이다.

    특성 SerializeField.
    비공개 필드 벡터3 _rotationAxis는 Vector3.up이다.

    비공개 함수 Update()는 없음:
        실행 transform.Rotate(_rotationAxis, _degreesPerSecond * Time.deltaTime, Space.Self).
    끝.
끝.
```

생성한 HanSpinner.cs를 게임 오브젝트에 붙이면 Inspector에서 회전 속도와 축을 조정할 수 있는 구성입니다. **파일명과 MonoBehaviour 클래스명을 일치**시키세요. 샘플은 자동 회전이므로 레거시/새 Input System 설정에 의존하지 않습니다.

추가 예제:

- [키보드로 큐브 움직이기](MOVEMENT-DEMO.md): HanKeyboardInput·HanObjectMover, Inspector 참조 연결과 실행 방법.

- `HealthState.han`: Unity에 의존하지 않는 상태 클래스와 명시적 피해 메서드·변경 이벤트.
- `HanCoroutineSample.han`: Start의 IEnumerator 반환과 WaitForSeconds, yield return/yield break.

`MonoBehaviour`, `ScriptableObject`, `Start`, `Update`, `OnEnable`, `OnDisable`, `OnCollisionEnter` 같은 이름은 Unity가 인식해야 하므로 그대로 씁니다. `특성 SerializeField.`, `특성 Range(0f, 10f).`처럼 C# 특성도 지정할 수 있습니다. 배열·제네릭·C# 식과 호출을 함께 사용할 수 있으며, 아직 표현하지 못하는 구문은 C# 원문 블록으로 넣습니다.

## 생성 파일 관리

편집할 파일은 `.han`입니다. Unity에서 소유권 표식이 일치하는 생성 `.cs`를 일반 열기하면 원본 `.han`으로 연결합니다. 일반 C#과 특정 C# 오류 행 이동은 그대로 유지합니다. Visual Studio에 이미 열린 C# 탭에서는 이 Unity 콜백이 실행되지 않으므로 원본을 따로 열어야 합니다.

생성 C#의 줄마다 보이던 `#line`은 원본 오류 행을 연결하기 위한 표시입니다. 기본 설정은 표시를 생략하는 **Emit Source Map = 꺼짐**입니다. 원본 행 매핑을 원하면 Settings에서 켜고 Generate All을 실행하세요. 끄면 C# 컴파일 오류의 행은 생성 C# 기준이며 한씨 문법 자체의 오류는 기존 변환기 진단을 사용합니다.

생성 파일 첫 줄에 원본 경로를 기록합니다. 해당 표식과 경로가 일치하는 파일만 갱신합니다. 같은 이름의 직접 작성한 .cs는 보호합니다.

변환 오류가 나면 소유한 생성 파일에 `#error`를 넣어 이전 코드가 계속 실행되는 것을 막습니다. `.han`을 고쳐 저장하면 정상 코드로 복구합니다. 생성 파일을 직접 편집하지 마세요. 원본을 이동하면 생성 C#을 AssetDatabase로 이동하여 .meta를 유지하고, 원본을 삭제하면 대응하는 소유 파일도 삭제하는 흐름을 구현했습니다. 이동 후 클래스명과 파일명도 확인하세요.

일괄 변환은 에디터에서 외부 프로세스를 순서대로 실행하며 파일당 제한 시간은 15초입니다. 대규모 소스 프로젝트용 백그라운드 변환·의존성 추적·Undo 통합은 아직 없습니다.

## 검증 범위

기존 모형 검증에서 생성·오류 후 복구·변경 없는 파일 유지·사용자 C# 보호·이동/삭제·meta 유지·프로젝트 확장자 설정을 확인했습니다. 이 검증 자체는 실제 Unity 실행 증거와 구분합니다.

2026-11-08에는 별도 `HanCSharpMovementDemo`를 Unity 6000.3.25f1에서 실제로 열어 이동 예제를 컴파일했습니다. 한씨 생성 메뉴, Inspector 입력 연결, Play Mode의 W 입력 시뮬레이션에 따른 위치 변경, 키 해제, Play 종료 시 초기 위치 복원, Unity 복귀 후 원본 변경의 자동 재생성을 확인했습니다. 물리 키보드 장시간 조작·빌드·실기기는 확인하지 않았습니다. 상세 범위는 예제 프로젝트의 `VALIDATION.md`를 참고하세요.

참고: [Unity의 프로젝트 확장자 설정](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/editorsettings/projectgenerationuserextensions), [외부 편집기 열기 API의 Unity 참조 소스](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/InternalEditorUtility.bindings.cs).
