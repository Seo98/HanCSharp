# 한씨 개발 가이드

대상: 프로그래밍을 처음 배우거나, C#/Unity 코드를 문장형 한국어로 작성하려는 개발자.

v0.9 작업본에서 처음 시작한다면 **[주어·목적어·동사 순서 입문 가이드](BEGINNER-GUIDE.md)**를 먼저 보세요. 이 문서는 기존 호환 문법과 C# 연동까지 설명합니다.

이 문서는 기존 기본 문법을 설명하며, **v0.7 추가 문법과 호환 규칙은 [문법 개선 안내](GRAMMAR-0.7.md)**에 정리했습니다. C#으로 생성한 뒤 C# 컴파일러와 Unity가 실행합니다. 식과 C#/Unity API 이름은 원래 표기를 사용합니다.

## 1. 처음 알아둘 네 가지

```han
정수 체력은 100이다.
체력에서 25를 뺀다.
출력 체력.
```

첫 줄은 **정수를 저장하는 체력이라는 변수를 만들고 100을 넣는다**는 뜻입니다. 다음 줄은 체력에서 25를 빼고, 마지막 줄은 결과 75를 출력합니다.

| 구성 요소 | 위 예제 | 의미 |
|---|---|---|
| 자료형 | 정수 | 어떤 종류의 값을 저장할지 정한다. |
| 이름 | 체력 | 그 값을 나중에 다시 부르는 이름이다. |
| 초기값 | 100 | 만들 때 처음 저장하는 값이다. |
| 문장 끝 | 이다. 또는 . | 선언이나 실행 문장을 마친다. |

값을 바꾸는 대입 `=`과 같은지 검사하는 `==`는 다릅니다. 한씨의 `체력에 50을 넣는다.`는 대입이고, `만약 체력이 50과 같으면:`은 비교입니다.

한 문장은 한 줄에 씁니다. 여러 줄을 묶는 블록은 `:`로 시작하고 **끝.**으로 닫습니다. 들여쓰기는 4칸을 권장합니다. 조사 은/는·이/가·을/를·와/과는 양쪽을 허용합니다.

## 2. 자료형 고르기

| 한씨 | C# | 용도 | 초기값 예시 |
|---|---|---|---|
| 정수 | int | 개수, 레벨, 정수 체력 | `100` |
| 긴정수 | long | int 범위를 넘을 수 있는 누적 수치 | `10000000000L` |
| 단정도실수 | float | Unity의 속도, 시간, 거리 | `3.5f` |
| 실수 | double | 일반적인 실수 계산 | `3.5` |
| 십진수 | decimal | 십진 정밀도가 필요한 계산 | `3.5m` |
| 논리 | bool | 참/거짓 상태 | `true` 또는 `false` |
| 문자열 | string | 이름, 설명, 메시지 | `"플레이어"` |
| 문자 | char | 문자 하나 | `'가'` |
| 객체 | object | 여러 타입을 담는 공통 참조 | 타입이 명확하다면 구체적 타입을 우선 |
| 자동 | var | 초기값으로 지역 변수 타입 추론 | `new List<int>()` |
| 없음 | void | 함수가 값을 반환하지 않음 | 변수 자료형으로는 사용 불가 |
| 벡터2 / 벡터3 | UnityEngine.Vector2 / Vector3 | 위치·방향·크기 | `Vector3.up` |
| 회전 / 색상 | UnityEngine.Quaternion / Color | 회전·색상 | `Quaternion.identity` |
| 코루틴 | System.Collections.IEnumerator | Unity 코루틴 함수 반환형 | Start 등에서 사용 |

```han
문자열 playerName은 "플레이어"이다.
정수 level은 1이다.
단정도실수 moveSpeed는 3.5f이다.
논리 isAlive는 true이다.
출력 $"{playerName}: 레벨 {level}, 속도 {moveSpeed}, 생존 {isAlive}".
```

`3.5f`의 f는 float, `3.5m`의 m은 decimal 표기입니다. Unity의 `Time.deltaTime`, Transform 등과 계산할 속도에는 보통 float를 사용합니다. `실수`를 쓰면 double이므로 Unity API에 넘길 때 타입이 맞지 않을 수 있습니다.

`자동`은 아무 타입으로나 계속 바뀌는 변수가 아닙니다. 초기값을 기준으로 C# 타입이 정해집니다. 기본 자료형과 배열·구조체, 클래스 참조의 복사 방식도 C# 규칙을 따릅니다. 클래스 변수에 다른 객체 참조를 대입하는 것과 객체를 복제하는 것은 다릅니다.

## 3. 이름을 짓는 규칙

### 언어가 요구하는 이름 규칙

- 공백이나 하이픈 대신 문자·숫자·밑줄을 사용합니다. 첫 글자에는 숫자를 쓰지 않습니다.
- 대문자와 소문자를 구분합니다. `health`와 `Health`는 다른 이름입니다.
- 한국어 이름도 사용할 수 있습니다. `남은체력`, `_이동속도`, `피해계산`처럼 씁니다.
- C# 예약어가 꼭 필요하면 `@class`처럼 이스케이프할 수 있지만, 다른 의미 있는 이름을 고르는 편이 낫습니다.
- 한씨 키워드·자료형과 같은 이름은 피합니다. 특히 이름이 `정수`인 사용자 타입은 자료형 별칭과 충돌할 수 있습니다.

### 이 가이드의 권장 명명법

기존 프로젝트 규칙이 있다면 그것을 우선합니다. 다음은 새 코드를 시작할 때 쓸 수 있는 기본 규칙입니다.

| 대상 | 권장 형태 | 예시 |
|---|---|---|
| 클래스 | 역할이 드러나는 PascalCase | `HealthState`, `PlayerController` |
| 함수·메서드 | 동작을 나타내는 PascalCase | `TakeDamage`, `TryUseItem`, `Refresh` |
| 속성 | 의미 있는 PascalCase 명사 | `CurrentHealth`, `IsAlive` |
| private 필드 | _camelCase | `_currentHealth`, `_moveSpeed` |
| 지역 변수·매개변수 | camelCase | `remainingTime`, `damageAmount` |
| 이벤트 | 발생한 변화를 나타내는 이름 | `HealthChanged`, `ItemUsed` |
| 한국어 private 필드 | 밑줄 + 구체적 명사 | `_현재체력`, `_이동속도` |
| 한국어 지역 변수 | 구체적 명사 | `남은시간`, `피해량` |

한국어 이름에는 대소문자 구분을 억지로 적용하지 않습니다. 범위를 구분하는 밑줄과 역할이 분명한 단어로 일관성을 유지하세요. 함수 이름이 길어져도 `Do`, `Process`, `Manager`만으로 역할을 숨기지 않는 편이 좋습니다.

### 게임에서 자주 쓰는 이름 사전

| 이름 | 의미 | 타입 예시 |
|---|---|---|
| `_currentHealth` | 현재 체력 | 정수 |
| `_maxHealth` | 최대 체력 설정값 | 정수 |
| `damageAmount` | 이번에 적용할 피해량 | 정수 |
| `_moveSpeed` | 이동 속도 설정값 | 단정도실수 |
| `moveDirection` | 이동 방향 | 벡터3 |
| `_target` | 명시적으로 연결한 대상 | Transform |
| `_cooldownSeconds` | 쿨다운 길이 | 단정도실수 |
| `remainingSeconds` | 남은 시간 | 단정도실수 |
| `itemCount` | 아이템 개수 | 정수 |
| `isAlive` | 살아 있는지 | 논리 |
| `canAttack` | 공격 가능한지 | 논리 |
| `hasTarget` | 대상이 있는지 | 논리 |
| `selectedIndex` | 선택 항목의 위치 | 정수 |

시간 단위는 `Seconds`, 거리나 각도는 필요에 따라 `Meters`, `Degrees`처럼 이름으로 밝힙니다. `time`, `value`, `data`만 반복하는 것보다 `remainingSeconds`, `damageAmount`처럼 의미를 드러내세요. `a`, `b`, `i`는 짧은 계산·반복문에서만 사용해도 충분합니다.

## 4. 지역 변수·필드·속성의 차이

지역 변수는 함수가 실행되는 동안 사용할 값을 담습니다. 필드는 객체가 유지할 상태나 참조를 보관합니다. 속성은 외부에 보여줄 값을 제공합니다.

```han
공개 봉인 클래스 HealthState:
    비공개 필드 정수 _currentHealth는 100이다.
    공개 속성 정수 CurrentHealth는 _currentHealth이다.

    공개 함수 TakeDamage(정수 damageAmount)는 없음:
        만약 damageAmount가 0보다 작으면:
            실행 throw new System.ArgumentOutOfRangeException(nameof(damageAmount)).
        끝.
        정수 remainingHealth는 System.Math.Max(0, _currentHealth - damageAmount)이다.
        _currentHealth에 remainingHealth를 넣는다.
    끝.
끝.
```

여기서 `_currentHealth`는 필드, `CurrentHealth`는 읽기 속성, `remainingHealth`는 지역 변수, `damageAmount`는 매개변수입니다. 현재 한씨의 위 속성 문법은 C#의 `=>` 읽기 속성을 생성합니다. 외부에서 `CurrentHealth`에 직접 대입할 수 없습니다.

상태를 바꾸는 메서드에 비용·범위·조건 검사를 모아 두면 여러 곳에서 같은 규칙을 재사용할 수 있습니다. 인벤토리 View가 개수를 직접 줄이거나 저장 파일을 직접 쓰게 만들지 않습니다.

## 5. 조건·반복·함수

```han
함수 피해계산(정수 attack, 정수 defense)는 정수:
    반환 System.Math.Max(0, attack - defense).
끝.

정수 health는 100이다.
정수 damage는 피해계산(35, 10)이다.

반복 turn은 4회:
    health에서 damage를 뺀다.
    출력 $"{turn + 1}턴: {health}".
    만약 health가 0과 같으면:
        중단.
    끝.
끝.
```

- `반복 turn은 4회:`는 turn을 **0, 1, 2, 3**으로 사용합니다.
- `동안 조건이면:`은 조건이 참인 동안 반복합니다. 조건이 바뀌지 않으면 끝나지 않을 수 있습니다.
- `계속.`은 이번 반복의 남은 문장을 건너뜁니다. `중단.`은 반복을 종료합니다.
- 반환형이 정수라면 정수를 반환해야 합니다. 아무 값도 반환하지 않으면 `없음`을 사용합니다.
- 복합 조건은 `만약 health > 0 && canAttack이면:`처럼 C# 식으로 씁니다.

`공개 함수`처럼 접근 수식어가 있는 함수는 클래스 멤버로 작성합니다. 수식어 없는 `함수`는 지역 함수 예제에 사용할 수 있습니다. C#이 허용하지 않는 위치에 선언하면 컴파일 오류가 납니다.

## 6. 배열·목록·사전·enum

```han
가져오기 System.Collections.Generic.

정수[] scores는 new int[] { 10, 20, 30 }이다.
List<정수> scoreList는 new List<int> { 10, 20 }이다.
실행 scoreList.Add(30).
Dictionary<string,정수> counts는 new Dictionary<string,int> { ["coin"] = 5 }이다.
출력 scores[0].
출력 scoreList.Count.
출력 counts["coin"].
```

배열·목록의 인덱스는 0부터 시작합니다. 크기가 3이면 사용할 수 있는 위치는 0, 1, 2입니다. 유효 범위 밖에 접근하면 C# 예외가 발생합니다.

자료형 위치의 `List<정수>`는 `List<int>`로 변환하지만, **식 내부의 new List<int>()는 C# 문법 그대로** 씁니다. `new List<정수>()`가 자동으로 int로 바뀌는 것은 아닙니다.

enum은 현재 C# 원문 블록으로 정의합니다.

````han
```csharp
enum WeaponKind { Sword, Bow }
```
````

이 enum을 사용할 변수는 해당 선언이 C#에서 보이는 위치에 작성합니다. 예를 들어 클래스의 필드를 `비공개 필드 WeaponKind _weapon는 WeaponKind.Sword이다.`처럼 선언할 수 있습니다. 문자열과 리플렉션으로 게임 행동을 선택하는 대신 이런 타입을 사용할 수 있습니다.

## 7. Unity에서 작성하기

Unity 연결 도구를 설치한 뒤 `.han`을 저장하고 Unity로 돌아오면 대응하는 C#을 생성합니다. 실행 게임은 일반 Unity/C# 코드입니다. .NET 9 도구는 에디터의 외부 변환에만 사용합니다.

```han
가져오기 UnityEngine.

공개 봉인 클래스 HanSpinner는 MonoBehaviour를 상속한다:
    특성 SerializeField.
    특성 Range(0f, 360f).
    비공개 필드 단정도실수 _degreesPerSecond는 90f이다.

    특성 SerializeField.
    비공개 필드 벡터3 _rotationAxis는 Vector3.up이다.

    비공개 함수 Update()는 없음:
        실행 transform.Rotate(_rotationAxis, _degreesPerSecond * Time.deltaTime, Space.Self).
    끝.
끝.
```

이 코드는 **HanSpinner.han** 파일에 둡니다. 생성할 HanSpinner.cs와 MonoBehaviour 클래스명을 일치시키세요. `_degreesPerSecond`, `_rotationAxis`는 생성된 컴포넌트의 Inspector에서 조정하도록 선언했습니다. 실제 프로젝트에서는 생성 코드의 컴파일과 컴포넌트 연결을 확인해야 합니다.

| 이름 | 역할 | 유지할 점 |
|---|---|---|
| Awake | 컴포넌트 초기화 시점 | Unity 이름과 인수 형식을 유지 |
| OnEnable / OnDisable | 활성화·비활성화 | 이벤트 구독/해제를 짝지어 처리 |
| Start | 시작 시 초기화 | 필요하면 IEnumerator 코루틴으로 작성 |
| Update | 프레임마다 호출 | 일반 이동 등에는 deltaTime 고려 |
| FixedUpdate | 고정 시간 간격 호출 | 물리 처리는 프로젝트 규칙에 맞춰 배치 |
| OnCollisionEnter | 3D 충돌 콜백 | Collision 인수 등 Unity 시그니처 유지 |
| SerializeField | private 필드의 직렬화 표시 | Inspector에서 설정할 필드에 적용 |

`Start`를 `시작`으로, `Update`를 `갱신`으로 임의 번역하면 Unity 콜백이 되지 않습니다. 일반 메서드 이름은 자유롭게 정할 수 있지만 Unity가 찾는 이름은 그대로 둡니다. 입력 API도 프로젝트의 기존 Input System을 사용합니다.

배열·List를 선언할 수 있다는 것과 모든 자료구조가 Unity Inspector에 기본 표시된다는 것은 다릅니다. Unity의 기본 직렬화 규칙을 따라야 하며 Dictionary의 기본 Inspector 편집을 전제로 설계하지 않습니다.

## 8. 책임과 참조를 나누기

- **일반 C# 상태·규칙 클래스**: 체력·비용·보상 등 계산과 검증. Unity 생명주기가 필요하지 않으면 MonoBehaviour를 상속하지 않습니다.
- **MonoBehaviour Controller/Presenter**: Unity 콜백, 씬 참조와 기능 간 연결.
- **View**: 값 표시와 입력 전달. 프로젝트 UI는 UGUI/TMP와 저장된 씬·프리팹을 사용합니다.
- **설정 ScriptableObject**: 여러 객체가 공유하는 조정값. 플레이 중 상태 데이터와 분리합니다.
- **저장 담당 코드**: 상태를 읽고 저장 형식으로 변환. UI 표시나 입력 처리와 분리합니다.

참조는 SerializeField 또는 명시적인 초기화로 전달합니다. 예제의 편의를 이유로 모든 객체를 이름으로 검색하거나 전역 싱글톤으로 연결하지 않습니다. 인터페이스는 교체할 경계나 테스트 격리가 필요할 때 추가합니다.

이벤트 구독 예시의 뼈대:

```text
비공개 함수 OnEnable()는 없음:
    실행 _state.Changed += HandleHealthChanged.
끝.

비공개 함수 OnDisable()는 없음:
    실행 _state.Changed -= HandleHealthChanged.
끝.
```

위 코드는 `_state`와 `HandleHealthChanged`가 해당 클래스에 선언되어 있고, `_state`가 사용 전에 초기화되어 있다는 전제의 일부 코드입니다. 이벤트 인수 타입에 맞는 메서드를 연결해야 합니다.

## 9. Visual Studio 자동 완성 사용법

**한씨 언어 도구** 확장을 설치하고 Visual Studio를 다시 연 뒤 .han을 엽니다.

| 조작 | 동작 |
|---|---|
| 글자 입력 | 일치하는 키워드·이름 제안 |
| Ctrl+Space | 현재 위치의 완성 목록 열기 |
| 위/아래 화살표 | 항목 선택 |
| Tab 또는 Enter | 선택 항목 삽입 |
| Escape | 완성 목록 닫기 |
| 변수 선언 / 조건문 / 클래스 틀 | 완성 목록에서 문장 템플릿 선택 |
| 유니티 컴포넌트 / 직렬화 필드 | Unity용 템플릿 선택 |

템플릿을 삽입하면 첫 번째 바꿀 이름을 선택합니다. 예를 들어 변수 선언 템플릿의 `체력`을 새 이름으로 바꿀 수 있습니다. 템플릿 본문에서 같은 이름을 다시 사용하는 부분은 직접 함께 수정하세요. 리팩터링이나 다중 위치 이름 변경 기능은 아닙니다.

현재 문서의 변수·필드·속성·함수·클래스·매개변수 이름을 제안합니다. `Time.`, `transform.`, `Vector3.`, `Mathf.`, `Debug.` 등에는 자주 쓰는 Unity 멤버 목록을 제공합니다. 문자열·주석에서는 완성을 억제합니다.

**타입·범위를 완전히 추론하는 C# IntelliSense와는 범위가 다릅니다.** 현재 문서의 이름이 아직 사용할 수 없는 범위에 있거나, 같은 이름의 다른 객체가 있을 수 있습니다. Unity 멤버 목록도 수동 카탈로그이므로 모든 오버로드와 패키지 API를 포함하지 않습니다. 최종 오류 판정은 빌드와 Unity 컴파일 결과를 기준으로 합니다.

## 10. 작성·검증 순서

1. 관련 기존 코드 2~3개를 읽고 폴더·이름·책임·참조 방식을 확인합니다.
2. 상태, 규칙, 씬 연결, UI, 저장 중 어디에 속하는 코드인지 정합니다.
3. 조정값과 참조는 Inspector 또는 명시적 초기화로 연결합니다.
4. `.han`을 수정하고 저장한 뒤 솔루션을 빌드합니다.
5. 원본 행에 연결된 HCS001 또는 C# 오류를 수정합니다.
6. 콘솔 출력 또는 Unity의 실제 입력·핵심 흐름을 확인합니다.
7. 테스트 통과, 설계 평가, 실제 플레이 검증을 구분해 기록합니다.

자동 완성 목록에 있다는 이유만으로 코드가 올바른 것은 아닙니다. 컴파일 통과도 Inspector 연결이나 실제 게임 입력의 성공을 보장하지 않습니다.

## 11. 자주 만나는 문제

| 증상 | 확인할 것 |
|---|---|
| HCS001 | 마침표·조사·콜론·끝. 누락, 지원되지 않는 문장 |
| double을 float에 넘길 수 없음 | Unity용 값이면 단정도실수와 f 표기 확인 |
| 이름을 찾을 수 없음 | 오타·대소문자·범위·using·참조 어셈블리 확인 |
| 속성에 대입할 수 없음 | 읽기 속성인지 확인하고 담당 메서드로 상태 변경 |
| MonoBehaviour를 찾을 수 없음 | UnityEngine 참조가 있는 Unity 프로젝트에서 컴파일 |
| Update가 실행되지 않음 | 콜백 이름·인수·컴포넌트 부착·활성 상태 확인 |
| Inspector 필드가 없음 | 필드의 SerializeField, 지원 타입, 실제 생성/컴파일 여부 확인 |
| .han이 검은 글자만 보임 | 확장 설치·재시작·.han 재열기 확인 |
| 완성 목록이 없음 | Ctrl+Space, 문자열/주석 위치인지 확인 |

## 12. 언어 도구를 확장할 때

- 문법의 근거는 Core의 `LanguageDefinition`, `DeclarationSyntax`, `KoreanToCSharp`입니다.
- 편집기 색상은 `HanLexer`, 제안·설명·템플릿은 `CompletionCatalog`에 있습니다.
- 문법을 추가하면 변환, 역변환, 색상, 자동 완성, 문서를 함께 갱신합니다.
- 템플릿이 실제로 변환되는지 편집 지원 테스트로 확인합니다.
- 생성 C#을 고치는 대신 .han 원본이나 변환 규칙을 수정합니다.

빌드·설치 절차는 [편집기 확장 안내](../visualstudio/EDITOR-SUPPORT.md), 기본 문법 요약은 [루트 README](../README.md), 엔진 연결은 [Unity 안내](../unity/README.md)를 참고하세요.
