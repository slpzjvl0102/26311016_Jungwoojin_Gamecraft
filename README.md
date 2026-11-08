# 아레나 서바이버

- 성명: 정우진
- 환경: Windows, .NET 9 SDK, C# / glc2d
- 구성: 단일 Stage (3개 Wave), 기획서, PNG 7개, PCM16 WAV 8개, 리소스 preview와 출처·라이선스

`src/ActionRpg.sln`을 Visual Studio에서 열어 실행하거나, 프로젝트 루트에서 다음 명령을 사용합니다.

```powershell
dotnet run --project src/ActionRpg.csproj
# 기존 정적 화면
dotnet run --project src/ActionRpg.csproj -- --preview
# 원본 크기와 표시 크기 비교
dotnet run --project src/ActionRpg.csproj -- --gallery
```

기존 NuGet dependencies와 버전은 `src/ActionRpg.csproj` 그대로 유지합니다. 기본 실행은 게임이며 `--original`도 게임을 실행합니다.

## 플레이

- 시작 화면에서 시작 버튼 또는 Enter를 누르세요.
- WASD 이동, 마우스 조준, 왼쪽 클릭 공격(유지 시 반복), Space 회피, Esc 일시정지/재개.
- 회피 중에는 공격할 수 없습니다. 금색은 적의 공격 예고, 빨간색은 공격, 청록색 테두리는 적의 회복 구간입니다.
- Wave 1: 추격병 3마리 → Wave 2: 추격병 3마리 + 돌진병 2마리 → Wave 3: 보스 1마리.
- Wave 사이에 5초 준비와 체력 1 회복(최대 5), 각 Wave 시작 전 1초 등장 예고가 있습니다.
- 결과 화면의 다시하기 버튼 또는 Enter/R로 재시작합니다. 창이 비활성화되면 자동으로 일시정지합니다.

## 검증과 구조

```powershell
dotnet build src/ActionRpg.csproj
dotnet run --project tests/ArenaStage.Tests.csproj
```

`ArenaStage.cs`는 전투·Wave·승패 규칙, `GameMain.cs`는 기존 glc2d의 Input·Graphics·Audio와 화면 흐름을 담당합니다. 검증 프로젝트는 별도 test package 없이 실제 Stage 코드를 참조합니다. 자동 검증은 고정된 상황에서 규칙과 상태 전이를 확인하며, 일반 플레이의 난이도나 3~5분 목표 달성을 보증하지는 않습니다.

파일별 용도와 규격은 `resource/resource_list.md`, 출처는 `resource/CREDITS.md`를 참조하세요.
