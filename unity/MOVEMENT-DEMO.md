# 한씨로 큐브 움직이기

**[Unity 패키지 설치 가이드](../docs/UNITYPACKAGE-INSTALL.md)**에 따라 예제 포함으로 설치합니다. v0.8 패키지는 기존 씬이나 입력 설정을 바꾸지 않습니다.

1. 컴파일이 끝나면 새 씬에 Cube를 만듭니다.
2. Cube에 **HanKeyboardInput**, **HanObjectMover**를 붙입니다.
3. Input 컴포넌트를 Mover의 **Input** 필드로 드래그합니다. **Move Speed**는 4입니다.
4. 카메라가 Cube를 보도록 배치하고 Play를 누릅니다.
5. Game 화면을 클릭하고 WASD/방향키로 XZ 평면을 이동합니다.

입력과 이동은 별도 컴포넌트입니다. 속도와 입력 참조를 Inspector에서 조정할 수 있습니다. Transform 이동이라 물리 충돌은 처리하지 않습니다. 새 Input System 설정을 쓰는 프로젝트는 Package Manager에서 Input System이 설치되어 있어야 합니다. 기존 Input Manager도 지원합니다.

수정할 원본: `Assets/HanCSharp/Examples/HanObjectMover.han`, `HanKeyboardInput.han`. 청록색 **한** 아이콘을 선택하고 **한씨 원본 열기**를 누릅니다. 생성 C#을 직접 고치면 다음 변환 때 덮어써집니다. Auto Generate를 꺼뒀다면 **Tools → HanCSharp → Generate All**을 실행하세요.

원본을 저장하면 미리보기의 C#도 자동으로 갱신됩니다. 직접 작성한 별도 C#과 함께 사용해도 됩니다.
