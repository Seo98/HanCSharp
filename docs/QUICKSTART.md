# 처음 사용하는 한씨

1. Unity Hub에서 Unity 6000.3 LTS를 설치합니다. 검증 버전은 6000.3.25f1입니다.
2. [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)을 운영체제·아키텍처에 맞게 직접 설치하고 Unity를 다시 엽니다. 소스 빌드가 아니면 SDK는 필요 없습니다.
3. [최신 Release](https://github.com/Seo98/HanCSharp/releases/latest)에서 **HanCSharp.unitypackage**를 받습니다.
4. **Assets → Import Package → Custom Package**에서 가져옵니다.
5. **Tools → HanCSharp → 설치 및 안전한 업데이트**에서 **패키지 파일 검사 → 검사한 파일을 백업 후 적용**을 누릅니다. 컴파일을 기다립니다.
6. Cube에 **Han Keyboard Input**, **Han Object Mover**를 추가합니다. Input 컴포넌트를 Mover의 Input 칸에 연결합니다.
7. Play 후 Game 화면을 클릭하고 WASD/방향키로 움직입니다. 씬과 카메라는 직접 배치합니다.

새 파일은 Project 창 **우클릭 → Create → HanCSharp → 한씨 컴포넌트**로 만듭니다. 청록색 한 아이콘이 원본 `.han`입니다. 생성 `.cs`를 일반 열기해도 원본으로 연결됩니다. 미리보기에서 저장된 원본과 C#을 비교할 수 있고 저장 후 자동 갱신됩니다.

Visual Studio 자동완성은 Release의 **HanCSharp-0.7.vsix**를 직접 설치합니다. 문법 버전은 0.7 그대로입니다. 편집기는 Unity Preferences → External Tools에서 지정합니다.

**[전체 설치·안전한 업데이트·입력 설정·문제 해결](UNITYPACKAGE-INSTALL.md)** · [문법과 변수명 가이드](DEVELOPMENT-GUIDE.md)

v0.7 이하의 START-HAN.cmd/Windows ZIP 안내는 과거 배포용입니다. v0.8부터 기본 배포는 Unity 패키지입니다.
