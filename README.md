# 아레나 서바이버 리소스

- 성명: 정우진
- 환경: Windows, .NET 9 SDK, C# / glc2d
- 구성: 기획서, PNG 7개, PCM16 WAV 8개, 출처·라이선스, WASD 플레이어 이동

`src/ActionRpg.sln`을 Visual Studio에서 열어 실행하거나, 프로젝트 루트에서 다음 명령을 사용합니다.

```powershell
dotnet run --project src/ActionRpg.csproj
```

필요한 NuGet dependencies는 `src/ActionRpg.csproj`에 명시돼 있습니다. 실행하면 GameMain이 열리며 게임 창에 focus를 두고 WASD로 플레이어를 이동합니다. 카메라는 고정이며 창을 닫으면 종료합니다.

파일별 용도와 규격은 `resource/resource_list.md`, 출처는 `resource/CREDITS.md`를 참조하세요.
