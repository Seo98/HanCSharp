# 한씨 가이드 목차

한씨는 `.han` 한국어 문장을 일반 C#으로 변환합니다. Unity 런타임에 별도 언어 해석기를 넣지 않습니다.

## 처음 사용하는 분

1. [처음 사용자 가이드](QUICKSTART.md) — 실행 ZIP 다운로드, 시작 창, Unity 설치, 새 한씨 파일 만들기.
2. [Unity에서 큐브 움직이기](../unity/MOVEMENT-DEMO.md) — WASD 입력, 컴포넌트 참조, Inspector 속도 조절.
3. [개발 가이드](DEVELOPMENT-GUIDE.md) — 자료형, 변수명 정의, 필드·속성·매개변수, 조건·반복·함수.
4. [C#과 함께 사용하기](CSHARP-INTEROP.md) — 직접 작성한 C#과 생성 C#의 차이, 서로 호출하기.

## 편집 환경

- [Visual Studio 시작](../visualstudio/START-HERE.md)
- [자동완성·색상 강조 설치](../visualstudio/EDITOR-SUPPORT.md)
- [Unity 설치와 생성 파일 관리](../unity/README.md)
- [현재 파일 변환용 외부 도구](../visualstudio/EXTERNAL-TOOLS.md)

## 개발·배포

- [GitHub 배포 가이드](GITHUB-PUBLISH.md) — 소스 빌드, Windows 실행 ZIP, Release 업데이트.
- [검증 결과](../VALIDATION.md) — 실제 실행과 모형 테스트를 구분한 기록.
- [v0.5.0 배포 검증](RELEASE-CHECK-0.5.md) — 짧은 경로 자동 준비·경고 수정·네이티브 시작 창.
- [v0.4.0 첫 다운로드 검증](RELEASE-CHECK.md) — 이전 버전에서 발견한 문제와 당시 기록.
- [MIT 라이선스](../LICENSE), [타사 고지](../THIRD-PARTY-NOTICES.md).

## 가장 자주 헷갈리는 점

| 파일 | 어떻게 사용하나요? |
|---|---|
| 직접 작성한 `.han` | 수정하고 저장합니다. Unity 복귀 후 C#이 생성됩니다. |
| 한씨가 생성한 `.cs` | Unity가 컴파일하는 결과입니다. 직접 수정하면 다음 변환에 덮어써집니다. |
| 직접 작성한 별도의 `.cs` | 일반 C#처럼 수정합니다. 한씨가 덮어쓰지 않습니다. |

Unity에서 생성 C#을 일반 열기하면 원본 한씨로 연결합니다. Visual Studio에 이미 열린 C# 탭이 자동으로 바뀌는 것은 아닙니다. 생성 C#의 `#line`을 보려면 Settings의 Emit Source Map을 켜고 Generate All을 실행합니다. 기본값은 꺼짐이며 C# 컴파일 오류의 행은 생성 C# 기준입니다.
