# GitHub Unity 패키지 배포

공개 저장소는 [Seo98/HanCSharp](https://github.com/Seo98/HanCSharp), 라이선스는 MIT입니다. v0.8부터 사용자 배포물은 EXE 대신 Unity 패키지입니다.

## 배포 파일

- **HanCSharp.unitypackage**: Unity가 직접 ExportPackage한 설치 도구와 압축 DLL 데이터.
- **HanCSharp-UnityPayload.zip**: 안전한 업데이트·오프라인 검사에 쓰는 동일 버전 파일·manifest·기존 배포 해시 목록.
- **HanCSharp-0.7.vsix**: 선택 설치 Visual Studio 확장. 문법이 같으므로 버전 유지.
- **INSTALL.md**: 필수 프로그램을 직접 설치하는 한국어 안내.
- **SHA256SUMS.txt**: 위 파일의 SHA-256. 업데이트 버튼은 payload 해시를 확인합니다.

Unity/.NET Runtime을 배포에 묶거나 자동 설치하지 않습니다. [사용자 설치 안내](UNITYPACKAGE-INSTALL.md)의 공식 링크를 사용합니다.

## 유지보수자 빌드

저장소 global.json에 맞는 SDK, Unity 6000.3.25f1 및 QA 전용 프로젝트와 Pipeline 연결이 필요합니다. 사용자 게임 프로젝트에서 패키지 빌드를 하지 마세요.

```powershell
dotnet run --project tests/HanCSharp.Tests
dotnet run --project editor-support/EditorSupport.Tests
dotnet run --project tests/Updater.Tests -- C:/han-update-test
./tools/Build-UnityPayload.ps1 -OutputDirectory artifacts/UnityRelease
./tools/Build-UnityPackage.ps1 -UnityProjectPath C:/HanPackageBuild -PayloadDirectory artifacts/UnityRelease -OutputPath artifacts/HanCSharp.unitypackage
```

Payload 빌드는 apphost를 끄고 DLL·runtimeconfig를 만듭니다. `.NET 10 Runtime`에서 net9.0 DLL을 실행하도록 Major roll-forward를 기록합니다. Unity 패키지는 실제 `AssetDatabase.ExportPackage`를 사용하며 Assets/HanCSharpInstaller만 포함합니다.

Unity 없는 CI는 언어·연동·업데이트 검사와 payload 빌드까지만 수행합니다. Unity 패키지 추출과 새 프로젝트 Import·설치는 로컬 Unity에서 별도로 검증합니다. SDK 테스트와 실제 Play 검증은 구분해 기록하세요.

## 공개 전 확인

1. VERSION과 사용자 문서, 검증 기록을 갱신합니다.
2. 새 Unity 프로젝트에 실제 패키지를 가져와 검사·적용·예제 생성을 확인합니다.
3. 사용자 변경 충돌·백업·저장 후 미리보기 갱신을 확인합니다.
4. 파일별 SHA256SUMS.txt를 만들고 위 5개 파일을 Release에 첨부합니다.
5. 태그·커밋을 확인하고 Latest로 공개합니다. 기존 릴리스 파일은 덮어쓰지 않습니다.
6. 공개 주소에서 다시 다운로드해 해시·설치·업데이트 조회를 확인합니다.

`tools/Build-Release.ps1`, Launcher 소스와 v0.7 이하 Windows ZIP 문서는 과거 배포용으로 남아 있습니다. 최신 릴리스에는 그 EXE ZIP을 첨부하지 않습니다.
