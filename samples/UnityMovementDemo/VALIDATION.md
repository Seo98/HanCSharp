# 한씨 이동 예제 검증

검증일: 2026-11-08. Unity 6000.3.25f1 / Windows / URP / Input System 1.20.0.

## 실제 Unity에서 확인한 것

- 한씨 Generate All 메뉴로 두 이동 원본을 C#으로 생성하고 실제 Unity 컴파일 성공.
- MovementDemo.unity와 HanPlayer.prefab을 Unity API로 생성·저장. 카메라·바닥·재질과 이동 컴포넌트 포함.
- HanObjectMover의 Input 참조가 HanPlayer의 HanKeyboardInput에 연결됨. 저장된 Move Speed = 4.
- 실제 에디터의 Game 화면과 Inspector에서 청록색 큐브 확인.
- Play Mode에서 Input System의 W 누름을 CLI로 시뮬레이션. 입력 (0, 1), 위치가 (0, 0.5, 0)에서 (0, 0.5, 3.0234)로 바뀜. 해제 후 입력 (0, 0), 최종 위치 (0, 0.5, 5.6165)에서 정지. 값은 CLI 왕복 중 경과한 시간에 따른 관측값이며 정확한 이동 거리 성능 테스트가 아님.
- Play 종료 후 위치 (0, 0.5, 0) 복원 및 저장된 참조 유지.
- 한씨 원본에 주석을 추가해 저장한 후 Unity로 복귀하자 import 콜백이 대응 C#을 자동 재생성함. 원본 변경과 생성된 주석을 비교해 확인.
- Unity 솔루션을 생성하고 Assembly-CSharp.csproj에 HanObjectMover.han이 포함됨. 외부 편집기 열기 콜백 성공 반환.

## 검증의 한계

- W 이동은 Input System 이벤트 시뮬레이션으로 확인했다. UI 자동화의 짧은 W 키 입력에서는 위치 변화가 관측되지 않았으므로 이를 물리 키보드 조작 성공으로 주장하지 않는다.
- WASD 전체·방향키·대각선·기존 Input Manager 분기·장시간 조작·실기기·플레이어 빌드는 실제 검증하지 않았다.
- 이동 예제는 Transform 기반이다. 충돌·중력·Rigidbody 이동은 제공하지 않는다.
- 최초 전체 예제 설치 중 다른 예제 HealthState의 nullable 경고와 기존 편집기 연결 API의 obsolete 경고를 관측했다. 이동 전용 프로젝트에서는 불필요한 기존 샘플을 제거했고 원본 전체 샘플은 형제 HanCSharp 폴더에 유지한다. 컴파일 오류는 0개였다.

## 설계 평가

입력 컴포넌트는 이동 입력만 제공하고, 이동 컴포넌트는 연결된 입력과 Inspector 속도로 자신의 Transform을 제어한다. 원본·생성 코드·에디터 변환기를 분리하고 씬/프리팹에 참조를 저장했다. 이 구조와 실제 실행 검증은 별도 평가이며 완전한 게임 이동 시스템의 검증을 의미하지 않는다.
