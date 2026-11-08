# 리소스 목록

## Graphics

모두 Kenney Tiny Dungeon의 투명 PNG 또는 지형 타일이며 원본 크기는 16×16, 정적 1 frame입니다. 정적 preview는 960×640 화면을 기준으로 NearestNeighbor를 사용합니다.

| 파일 (`graphics/`) | 기획상 용도 | 표시 크기 | 실제 보이는 범위 |
| --- | --- | --- | --- |
| player.png | 플레이어 | 48×48px | 48×45px |
| chaser.png | 추격병 외형 후보(유령) | 48×48px | 48×45px |
| charger.png | 돌진병 외형 후보(박쥐) | 48×48px | 48×36px |
| boss.png | 아레나 가디언 | 80×80px | 80×80px |
| sword.png | 정적 preview용 검 (실제 전투는 창) | 32×32px | 16×32px |
| floor.png | 전투장 바닥 | 48×48px | 48×48px |
| wall.png | 외곽 벽 | 48×48px | 48×48px |

Source crop은 모두 `(0,0,16,16)`입니다. 캐릭터는 공통 cell을 유지하고, 검의 좌우 투명 여백은 위 실제 표시 범위에 반영했습니다. 전용 방향별 Animation sheet는 포함하지 않습니다. 실제 게임의 창은 기존 Direct2D API로 자루와 창날을 그리며, 조준 회전과 전진·회수 Animation을 코드로 처리합니다. 추가 PNG나 package는 사용하지 않습니다.

## Audio

모두 실제 Ogg/Vorbis decode 후 변환한 PCM signed 16-bit WAV이며 stereo입니다. 현재 G2Audio/G2AudioSound API에서 사용할 수 있습니다.

| 파일 (`audio/`) | 기획상 용도 | Sample rate |
| --- | --- | --- |
| attack.wav | 창 찌르기 SFX (기존 파일 재사용) | 48 kHz |
| dodge.wav | 회피 SFX 후보 | 48 kHz |
| hurt.wav | 피격 SFX 후보 | 44.1 kHz |
| enemy_hit.wav | 보스/금속 타격 후보 | 44.1 kHz |
| step.wav | 돌바닥 발소리 | 44.1 kHz |
| victory.wav | Wave/결과 Jingle 후보 NES00 | 44.1 kHz |
| defeat.wav | Wave/결과 Jingle 후보 NES01 | 44.1 kHz |
| battle.wav | 전투 BGM, 26.65초 loop | 44.1 kHz |

`victory`/`defeat`는 각각 승리/패배 시 재생하며, `battle`은 전투 BGM으로 반복 재생합니다. OGG가 모든 환경에서 불가능한 것은 아니지만 현재 G2Audio wrapper에 Vorbis 경로가 없어 호환 WAV를 준비했습니다.

캐릭터·적 4종, 무기, 전투장과 원본 리스트의 RPG Audio / Impact Sounds / Music Jingles / Battle Loop 범주를 포함했습니다. 현재 단일 Stage의 3개 Wave와 보스 전투에 사용합니다.

출처와 라이선스: [CREDITS.md](CREDITS.md)
