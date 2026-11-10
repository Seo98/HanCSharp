# 한씨 Unity 패키지 설치·업데이트

v0.8부터 실행 EXE 대신 **HanCSharp.unitypackage**로 배포합니다. Unity에 설치 창과 DLL 변환기를 가져오는 방식입니다. 필수 프로그램을 자동 설치하거나 운영체제 설정을 변경하지 않습니다.

**v0.9 초급 문법 작업본을 받은 경우:** 제공받은 v0.9 패키지와 HanCSharp-0.9.vsix를 사용하세요. 공개 v0.8 다운로드에는 새 문법이 없습니다. [주어·목적어·동사 순서 입문 가이드](BEGINNER-GUIDE.md)

## 먼저 직접 설치할 것

| 항목 | 설치 방법 |
|---|---|
| Unity Editor | Unity Hub에서 Unity 6000.3 LTS를 설치합니다. 확인한 버전은 6000.3.25f1입니다. |
| .NET Runtime | [공식 .NET 10 다운로드](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)의 **Run apps → .NET Runtime**에서 운영체제·아키텍처에 맞게 설치합니다. Windows Unity x64는 x64를 선택합니다. SDK나 ASP.NET 전용 프로그램을 따로 요구하지 않습니다. SDK에 런타임이 이미 있으면 별도 설치는 필요 없습니다. |
| 코드 편집기(선택) | Visual Studio의 Unity 개발 환경을 설치하고 Unity Preferences → External Tools → External Script Editor에서 선택합니다. |
| 자동완성 확장(선택) | v0.9 작업본은 **HanCSharp-0.9.vsix**, 공개 v0.8은 **HanCSharp-0.7.vsix**를 사용합니다. Visual Studio 문서를 저장하고 닫은 뒤 직접 설치합니다. |

런타임 설치 뒤 Unity를 완전히 닫고 다시 여세요. `dotnet`이 PATH에서 실행 가능해야 합니다. 변환기는 net9.0 DLL이며 런타임 설정의 Major roll-forward로 .NET 10에서도 실행됩니다. 새 설치는 .NET 10 Runtime을 권장합니다. 소스 자체를 빌드할 개발자만 저장소 global.json에 맞는 .NET SDK가 필요합니다. [런타임 선택 정책](https://learn.microsoft.com/en-us/dotnet/core/versions/selection)

Unity 프로젝트는 짧은 경로를 권장합니다. 패키지가 기존 프로젝트를 자동 이동하지 않습니다. Windows x64의 Unity/.NET 실행을 확인했으며 macOS·Linux의 실제 실행은 별도 검증이 필요합니다.

## 처음 설치

1. [GitHub 최신 Release](https://github.com/Seo98/HanCSharp/releases/latest)에서 **HanCSharp.unitypackage**를 받습니다.
2. 프로젝트를 열고 **Assets → Import Package → Custom Package**에서 패키지를 선택합니다.
3. 목록의 **Assets/HanCSharpInstaller**를 가져옵니다. 실제 한씨 도구·예제를 즉시 덮어쓰는 패키지가 아니라 설치 창과 압축 데이터입니다.
4. **Tools → HanCSharp → 설치 및 안전한 업데이트**를 엽니다.
5. **패키지 파일 검사**를 누릅니다. .NET 실행 확인과 파일별 추가·교체·보존·충돌을 확인합니다.
6. 예제 원본도 설치할지 선택합니다. 선택을 바꾸면 다시 검사합니다.
7. **검사한 파일을 백업 후 적용**을 누르고 Unity 컴파일을 기다립니다.

변환기 DLL은 `Tools/HanCSharp`에 설치되어 Assets 밖에서 실행됩니다. 게임에는 CLI나 외부 .NET 런타임을 포함하지 않습니다. `.han`은 원본, `.cs`는 생성 결과입니다. 예제를 선택하면 최초 설치에서 새로 추가한 예제만 C#으로 생성합니다.

## 안전한 업데이트 버튼

미리보기의 **안전한 업데이트**, 또는 **Tools → HanCSharp → 설치 및 안전한 업데이트**를 엽니다.

1. **최신 버전 확인·받기**를 누릅니다. 고정한 공식 GitHub 저장소만 조회하고 `HanCSharp-UnityPayload.zip`과 첨부 SHA-256을 확인합니다.
2. 변경 목록을 검토합니다. 수정한 도구나 출처를 확인할 수 없는 충돌 파일이 있으면 전체 적용이 차단됩니다. 자동 강제 덮어쓰기 옵션은 없습니다.
3. **검사한 파일을 백업 후 적용**을 누릅니다. 검사 후 사용자가 다시 저장한 경우에도 재검사하도록 중단합니다.
4. 교체 전 파일은 프로젝트 루트의 `HanCSharpBackups/날짜-고유번호`에 보관됩니다. **백업 폴더 열기**로 확인합니다.

기존 예제 원본·`.meta`·Settings 자산·직접 작성한 일반 C#은 보존합니다. 기존 예제의 새 배포 내용이 필요하면 별도 프로젝트에 설치해 비교하세요. 실제 교체 도중 실패하면 적용한 파일을 복구합니다. 동시에 다른 프로그램이 다시 수정한 파일은 강제로 복구하지 않고 직접 확인할 경로를 안내합니다. 제거된 도구 파일을 자동 삭제하는 기능은 없습니다.

v0.5~v0.7은 설치 상태 파일이 없으므로 알려진 공개 배포본과 해시가 같은 도구 파일만 갱신할 수 있습니다. 개발용으로 다시 빌드했거나 수정한 파일은 충돌로 표시됩니다. 백업 후 변경점을 비교해 관리 폴더를 정리하고 다시 검사하세요. 이전 EXE·런타임 파일은 자동 삭제하지 않으며 새 변환은 DLL을 우선 사용합니다.

`Assets/HanCSharpInstaller`는 배포용 설치 도구 영역입니다. 새 unitypackage를 가져올 때 이 영역의 파일은 Unity Import 창의 선택에 따라 갱신됩니다. 사용자 확장 코드는 이 폴더에 두지 마세요. 실제 설치된 `Assets/HanCSharp/Editor`의 사용자 변경은 적용 버튼에서 보호합니다. SHA-256은 다운로드와 파일 무결성을 확인하는 값이며 별도 전자서명 체계는 아닙니다.

오프라인 업데이트는 공식 Release의 **HanCSharp-UnityPayload.zip**을 받아 **배포 ZIP 직접 선택**으로 검사합니다. DLL이나 Editor 파일만 서로 다른 버전으로 복사하지 마세요.

## 저장 후 미리보기 자동 갱신

한 아이콘 원본을 선택해 Inspector의 **변환 미리보기**를 엽니다. **저장 후 자동 갱신**은 기본 켜짐입니다. 외부 편집기에서 저장하면 짧은 지연 후 반영됩니다. 연속 저장을 합치고 변환 중의 저장은 완료 후 다시 반영합니다. Auto Generate를 꺼도 미리보기는 저장된 원본으로 갱신됩니다. 저장하지 않은 편집기 버퍼는 읽지 않습니다. 필요하면 토글을 끄고 수동 새로고침을 사용합니다.

## Unity 이동 예제 연결

1. 예제 설치 후 Console 컴파일 오류가 없는지 확인합니다. 자동 생성이 꺼져 있으면 **Tools → HanCSharp → Generate All**을 실행합니다.
2. Cube를 만들고 **Han Keyboard Input**, **Han Object Mover**를 붙입니다.
3. Input 컴포넌트를 Mover의 **Input** 칸으로 드래그하고 Move Speed를 4로 둡니다.
4. 카메라가 Cube를 보도록 배치한 뒤 Play → Game 화면 클릭 → WASD/방향키를 사용합니다.

입력은 프로젝트의 설정을 사용합니다. 새 Input System이 켜져 있으면 Unity Package Manager에서 Input System을 설치해야 합니다. 기존 Input Manager를 사용해도 됩니다. 설치 도구는 입력 설정·씬·렌더 파이프라인을 바꾸지 않습니다. 이동은 Transform 기반이며 충돌을 처리하는 Rigidbody 이동 예제가 아닙니다.

## 문제가 있을 때

| 증상 | 확인 |
|---|---|
| 설치 메뉴 없음 | 패키지의 Installer/Editor를 가져왔는지, Unity 컴파일 오류가 있는지 확인 |
| dotnet 실행 실패 | .NET 10 Runtime의 아키텍처와 PATH 확인 후 Unity 재시작 |
| 충돌로 중단 | 사용자 변경을 백업·비교하고 다시 검사. 설정과 예제는 덮어쓰지 않음 |
| 원본만 있고 C# 없음 | Auto Generate 설정과 Generate All 확인 |
| 최신 배포 조회 실패 | 네트워크 확인 또는 공식 Payload ZIP 직접 선택 |
| 링크 경로 설치 거부 | 관리 폴더에 심볼릭 링크/정션이 없는 일반 프로젝트 경로 사용 |

[문법](https://github.com/Seo98/HanCSharp/blob/main/docs/GRAMMAR-0.7.md) · [원본 열기·오류 위치](https://github.com/Seo98/HanCSharp/blob/main/docs/UNITY-EDITING.md)
