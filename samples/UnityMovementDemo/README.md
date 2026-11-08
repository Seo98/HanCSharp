# 한씨로 큐브 움직이기

Windows 배포본은 **START-HAN.cmd → ② Unity 이동 예제 준비·실행**을 사용합니다. 짧은 폴더에 압축된 템플릿과 변환기를 준비한 뒤 Unity 6000.3.25f1로 이동 씬을 엽니다. 배포 ZIP에는 이 폴더의 프로젝트 대신 `templates/UnityMovementDemo.zip`을 넣었습니다. 직접 압축을 풀어 쓰기보다 시작 창을 이용하세요.

GitHub 소스 체크아웃에서는 이 폴더가 Unity 프로젝트 루트입니다. .NET 9 SDK로 도구를 먼저 준비합니다.

```powershell
dotnet publish src/HanCSharp.Cli -c Release -o app --self-contained false
.\tools\Build-Launcher.ps1
.\Install-Unity.ps1 -ProjectPath '.\samples\UnityMovementDemo'
```

소스 프로젝트 경로도 짧게 유지하세요. [처음 사용자 가이드](../../docs/QUICKSTART.md)를 참고하세요.

## 바로 실행

Play를 누른 다음 Game 화면을 클릭하세요. 씬이 열리지 않았다면 `Assets/HanCSharp/Scenes/MovementDemo.unity`를 여세요.

- W / ↑: 앞쪽(+Z)
- S / ↓: 뒤쪽(-Z)
- A / ←: 왼쪽(-X)
- D / →: 오른쪽(+X)
- Play를 끄면 편집 시점의 위치로 돌아갑니다.

Hierarchy의 `HanPlayer`를 선택하면 Inspector의 **Han Object Mover → Move Speed**로 초당 이동 거리를 바꿀 수 있습니다. 기본값은 4입니다. Play 중 조정한 값은 Play 종료 시 되돌아가므로 계속 사용할 값은 편집 모드에서 변경하고 씬을 저장하세요.

이미 씬/프리팹에 저장된 수치는 한씨 필드의 초기값보다 우선합니다. 원본의 `_moveSpeed는 4f`를 바꿔도 기존 컴포넌트의 저장된 값이 자동 교체되지는 않습니다. 기존 오브젝트의 속도는 Inspector에서 바꾸세요.

## 한씨 코드는 어디에 있나요?

`Assets/HanCSharp/Examples/`의 두 원본을 Visual Studio에서 엽니다.

| 원본 | 역할 |
|---|---|
| HanKeyboardInput.han | WASD·방향키를 읽어 MoveValue를 제공. 위치는 수정하지 않음 |
| HanObjectMover.han | 연결된 입력으로 자신의 Transform을 월드 XZ 평면에서 이동 |

한씨 파일을 저장하고 Unity로 돌아오면 같은 폴더의 C# 파일이 갱신됩니다. 처음에는 **Tools → HanCSharp → Generate All**로 생성할 수도 있습니다. `.cs`는 생성 결과이므로 `.han`을 수정하세요.

이동의 핵심은 다음과 같습니다.

```text
벡터2 입력값은 _input.MoveValue이다.
벡터3 이동방향은 new Vector3(입력값.x, 0f, 입력값.y)이다.
단정도실수 이동거리는 Mathf.Max(0f, _moveSpeed) * Time.deltaTime이다.
실행 transform.Translate(이동방향 * 이동거리, Space.World).
```

`Time.deltaTime`을 곱해 프레임마다 이동하는 거리를 시간에 맞춥니다. 입력 컴포넌트에서 대각선 입력의 길이를 1로 제한합니다. 새 Input System과 기존 Input Manager의 차이는 입력 파일의 C# 원문 구간에서 처리하고, 이동 코드는 공통으로 사용합니다. Both에서는 새 입력만 읽어 중복 이동을 피합니다.

## 기존 프로젝트에 직접 넣기

1. 한씨 도구 루트에서 설치 명령을 실행합니다.

   ```powershell
   .\Install-Unity.ps1 -ProjectPath 'C:\내프로젝트\UnityGame' -IncludeExamples
   ```

2. Unity로 돌아와 **Tools → HanCSharp → Generate All**을 실행하고 Console 오류가 없는지 확인합니다.
3. **GameObject → 3D Object → Cube**를 만들고 위치를 `(0, 0.5, 0)`으로 둡니다.
4. Cube에 **Han Keyboard Input**, **Han Object Mover** 두 컴포넌트를 추가합니다.
5. Cube의 Han Keyboard Input 컴포넌트를 Han Object Mover의 **Input** 칸으로 드래그합니다. 자동 이름 검색이나 싱글톤 연결은 사용하지 않습니다.
6. Move Speed를 `4`로 설정합니다.
7. Plane을 만들고 Camera가 Cube를 보도록 배치합니다. 위에서 보는 카메라의 예시는 위치 `(0, 12, -8)`, 회전 `(56, 0, 0)`입니다.
8. 씬을 저장하고 Play → Game 화면 클릭 → WASD/방향키로 이동합니다.

입력 설정은 기존 프로젝트의 것을 유지합니다. 새 Input System이 활성화된 프로젝트에서는 해당 패키지가 설치되어 있어야 합니다. 두 입력 백엔드가 모두 꺼져 있으면 입력은 0입니다. 관련 설정은 **Edit → Project Settings → Player → Other Settings → Active Input Handling**에서 확인합니다. [Unity 입력 백엔드 설명](https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/Installation.md)

## Visual Studio 자동완성

Unity에서 **Tools → HanCSharp → Include Han Files In Solution**, 이어서 **Assets → Open C# Project**를 사용합니다. 한씨 VS 확장이 설치된 Visual Studio에서 `.han`을 엽니다.

- `단정도` 입력 후 Ctrl+Space → `단정도실수` 선택 → Tab.
- `Time.` 입력 → `deltaTime` 선택 → Tab.
- 코드를 고친 뒤 Ctrl+S → Unity로 돌아오기 → 컴파일 완료 후 Play.

## 움직이지 않을 때

| 확인할 것 | 조치 |
|---|---|
| Play 중인가? | 상단 Play 버튼 확인 |
| Game 화면에 포커스가 있는가? | Game 화면을 한 번 클릭 |
| Input 참조가 비어 있는가? | Inspector에서 HanKeyboardInput 연결 |
| 컴포넌트 체크가 꺼져 있는가? | 두 컴포넌트 활성화 |
| Move Speed가 0인가? | 4 등 양수 입력 |
| Console에 빨간 오류가 있는가? | 원본 한씨 또는 변환기 경로 확인 |
| 큐브가 카메라 밖으로 나갔는가? | Play를 종료해 초기 위치로 복귀 |

이 예제는 Transform 이동을 배우는 예제입니다. Rigidbody 이동, 벽 충돌, 중력, 점프는 포함하지 않습니다. 물리 이동으로 확장할 때는 이동 컴포넌트를 해당 물리 방식에 맞게 바꾸세요.
