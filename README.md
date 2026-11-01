# 아레나 서바이버 리소스

- 성명: 정우진
- 환경: Windows, .NET 9 SDK, C# / glc2d
- 구성: 기획서, PNG 7개, PCM16 WAV 8개, 출처·라이선스, 정적 화면 출력 프로젝트

`src/ActionRpg.sln`을 Visual Studio에서 열어 실행하거나, 프로젝트 루트에서 다음 명령을 사용합니다.

```powershell
dotnet run --project src/ActionRpg.csproj
# 원본 크기와 표시 크기 비교
dotnet run --project src/ActionRpg.csproj -- --gallery
```

필요한 NuGet dependencies는 `src/ActionRpg.csproj`에 명시돼 있습니다. 기본 화면은 정적 리소스 preview이며 창을 닫으면 종료합니다. 기존 GameMain은 `--original` 인자로 실행할 수 있습니다.

파일별 용도와 규격은 `resource/resource_list.md`, 출처는 `resource/CREDITS.md`를 참조하세요.
