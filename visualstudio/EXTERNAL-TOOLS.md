# Visual Studio 외부 도구

솔루션의 F5 경로는 별도 등록 없이 작동합니다. 현재 열린 파일 하나를 변환하거나 실행하고 싶을 때 다음 외부 도구를 추가할 수 있습니다.

Visual Studio의 **도구 → 외부 도구 → 추가**에서 설정합니다. 아래 `패키지폴더`를 실제 압축을 푼 폴더로 바꾸세요.

| 항목 | 값 |
|---|---|
| 제목 | 한씨 - 현재 파일 실행 |
| 명령 | `C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe` |
| 인수 | `-NoProfile -File "패키지폴더\visualstudio\HanExternalTool.ps1" -Action Run -InputFile "$(ItemPath)"` |
| 초기 디렉터리 | `$(ItemDir)` |
| 출력 창 사용 | 선택 |

다른 명령은 같은 설정에서 제목과 `-Action` 값만 바꿉니다.

| Action | 동작 |
|---|---|
| Run | 현재 .han을 콘솔 프로그램으로 실행 |
| Check | 현재 .han을 콘솔 프로그램으로 컴파일 검사 |
| Preview | 현재 .han의 C#을 임시 폴더에 생성하고 경로 출력 |
| ToKorean | 현재 .cs 옆에 `.converted.han` 새 파일 생성 |

실행 전 현재 파일을 저장하세요. Preview는 Unity Assets 밖의 임시 폴더에 저장하므로 중복 컴포넌트 클래스를 만들지 않습니다. ToKorean은 기존 출력 파일이 있으면 실패하며 덮어쓰지 않습니다. Run의 출력 창에서는 콘솔 키보드 입력을 받지 못할 수 있으므로 입력이 필요한 프로그램은 솔루션의 F5/Ctrl+F5 경로를 사용하세요.

Unity 스크립트는 Unity 어셈블리가 필요하므로 위 Run/Check로 엔진 동작을 실행하지 않습니다. Unity에 설치된 자동 변환 도구와 Unity Console에서 검사하세요. 별도 참조 검사는 CLI `check --library --reference 경로`를 사용할 수 있습니다.

외부 도구 등록은 사용자의 전역 Visual Studio 설정을 바꾸므로 이 패키지는 자동으로 덮어쓰지 않습니다.

매크로와 설정 위치는 [Microsoft의 외부 도구 문서](https://learn.microsoft.com/en-us/visualstudio/ide/managing-external-tools)를 기준으로 작성했습니다.
