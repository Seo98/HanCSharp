# GitHub에 올릴 때

## 저장소에 넣을 것

`HanCSharp` 폴더 자체를 저장소 루트로 사용합니다. src, tests, unity, editor-support, tools, build, samples, docs, .github, 솔루션과 루트 스크립트를 올립니다. `.gitignore`가 bin/obj와 로컬 배포 산출물을 제외합니다. Unity 프로젝트의 Library/Temp/Logs/UserSettings, 인증 정보·개인 로그는 올리지 마세요.

한씨 소스는 루트 LICENSE의 MIT 라이선스로 배포합니다. THIRD-PARTY-NOTICES.md와 runtime 라이선스 고지는 타사 구성 요소에 대한 별도 고지입니다. 배포 스크립트가 runtime 고지를 함께 복사합니다.

## 실행 ZIP 만들기

Windows에서 .NET 9 SDK, Visual Studio 2022와 .NET Framework 4.7.2 개발용 참조가 필요합니다. 새 VS 확장 빌드가 필요하면 먼저 `editor-support/Build-Extension.ps1`을 실행합니다.

```powershell
.\tools\Build-Release.ps1
```

`artifacts/HanCSharp-Windows-x64.zip`이 만들어집니다. .NET 런타임과 CLI, START-HAN 시작 창, Unity 생성 메뉴, VSIX, 예제·가이드·소스를 포함합니다. 처음 빌드할 때 공식 NuGet에서 Windows 런타임을 받으므로 인터넷 연결이 필요합니다. Roslyn을 사용하므로 trimming/AOT는 적용하지 않습니다.

## GitHub에서 배포하기

1. GitHub에 저장소를 만들고 이 폴더를 push합니다.
2. **Releases → Draft a new release**를 선택합니다.
3. 태그·버전과 변경 내용을 작성합니다.
4. 위에서 만든 ZIP을 첨부합니다. 소스 ZIP만 올리면 처음 사용자는 실행 파일을 받지 못합니다.
5. [처음 사용자 가이드](QUICKSTART.md)를 릴리스 설명에 링크하고 Publish release를 누릅니다.
6. 다른 폴더에 ZIP을 새로 풀어 START-HAN.cmd → 예제 실행을 직접 확인합니다.

현재 공개 저장소는 [Seo98/HanCSharp](https://github.com/Seo98/HanCSharp)입니다. README와 사용자 가이드의 다운로드 링크를 다른 저장소로 옮길 때 함께 수정하세요.

## 자동화

포함된 `.github/workflows/release.yml`은 **수동 Actions 실행** 시 Windows ZIP을 빌드해 Actions artifact로 제공합니다. 자동 공개 업로드는 하지 않습니다. artifact를 내려받아 테스트한 뒤 위 절차로 Release에 첨부하세요. [첫 Windows 빌드](https://github.com/Seo98/HanCSharp/actions/runs/42473219816)에서 언어 검증·완성 검증·VS 확장 빌드·배포 ZIP 생성이 모두 성공했습니다.

현재 도구는 언어 프로토타입입니다. README/VALIDATION에 실제 확인한 버전과 아직 지원하지 않는 문법·IDE 기능을 함께 유지하세요.
