# Unity에서 한씨 사용

v0.8부터 **HanCSharp.unitypackage**로 설치합니다. **[설치·필수 프로그램·안전한 업데이트 전체 가이드](../docs/UNITYPACKAGE-INSTALL.md)**를 먼저 확인하세요.

필수: Unity 6000.3 LTS와 별도 .NET 10 Runtime. 선택: Visual Studio와 HanCSharp-0.7.vsix. EXE·런타임을 패키지에 포함하지 않습니다.

Unity Import 후 **Tools → HanCSharp → 설치 및 안전한 업데이트**에서 검사하고 적용합니다. 실제 도구는 Assets/HanCSharp/Editor, 예제는 Assets/HanCSharp/Examples, DLL 변환기는 Assets 밖 Tools/HanCSharp에 설치됩니다. 설치 창은 Assets/HanCSharpInstaller에 있습니다.

Project 창 **Create → HanCSharp → 한씨 컴포넌트**로 원본을 만듭니다. 저장 후 Unity로 돌아오면 C#이 생성됩니다. Settings의 Auto Generate가 꺼졌다면 Generate All을 누릅니다. 직접 작성한 일반 C#도 함께 사용할 수 있습니다.

미리보기의 **저장 후 자동 갱신**은 미리보기만 갱신하며 실제 생성 설정과 독립적입니다. **안전한 업데이트**는 다운로드·검사·목록 확인 후 명시적으로 적용합니다. 사용자 수정과 기존 예제는 보호됩니다.

[큐브 이동 예제](MOVEMENT-DEMO.md) · [원본 열기·오류 위치](../docs/UNITY-EDITING.md)
