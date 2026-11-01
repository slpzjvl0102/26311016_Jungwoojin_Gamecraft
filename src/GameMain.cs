// -------------------------------------------------------------------------------------------------------------------------------------------------------------
// Author: 3dapi (https://github.com/3dapi)
// -------------------------------------------------------------------------------------------------------------------------------------------------------------

using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

class GameMain : G2AppBase
{
	private const float PlayerSize = 48.0f;
	private const float PlayerSpeed = 240.0f;
	private Vector2 _playerPosition;
	private readonly Dictionary<string, G2Texture> _textures = new();
	private G2Font? _heading;
	private G2Audio? _backgroundMusic;

	public override System.Drawing.Size ScreenSize => GameGlobal.ScreenSize;
	public override string GameName => GameGlobal.GameName;

	protected override void Initialize()
	{
		//---------------------------------------
		// 게임 관련 객체를 생성합니다.
		//---------------------------------------
		ClearColor = new Color4(.055f, .07f, .10f, 1);
		foreach (string name in new[] { "player", "chaser", "charger", "boss", "sword", "floor", "wall" })
		{
			_textures.Add(name, new G2Texture($"resource/graphics/{name}.png"));
		}
		_heading = new G2Font("Malgun Gothic", 26);
		_playerPosition = new Vector2(220, 330);
		_backgroundMusic = new G2Audio("resource/audio/battle.wav");
		_backgroundMusic.Play(isLooping: true);
	}

	protected override void Update()
	{
		//---------------------------------------
		// 게임 관련 객체를 갱신합니다.
		//---------------------------------------
		Vector2 direction = Vector2.Zero;
		if (IsMoveKeyHeld(Keys.W)) direction.Y -= 1.0f;
		if (IsMoveKeyHeld(Keys.S)) direction.Y += 1.0f;
		if (IsMoveKeyHeld(Keys.A)) direction.X -= 1.0f;
		if (IsMoveKeyHeld(Keys.D)) direction.X += 1.0f;

		// 대각선 이동 속도를 보정하고 frame 간 경과 시간을 반영합니다.
		if (direction.LengthSquared() > 0.0f)
		{
			_playerPosition += Vector2.Normalize(direction) * PlayerSpeed * (float)DeltaTime;
		}

		// 기존 arena의 벽 안쪽으로 이동 범위를 제한합니다.
		_playerPosition.X = Math.Clamp(_playerPosition.X, 96.0f, 864.0f - PlayerSize);
		_playerPosition.Y = Math.Clamp(_playerPosition.Y, 160.0f, 496.0f - PlayerSize);
	}

	private bool IsMoveKeyHeld(Keys key)
	{
		// glc2d의 Press에는 처음 누른 frame의 Down이 포함되지 않습니다.
		return Input.IsKeyDown(key) || Input.IsKeyPress(key);
	}

	protected override void Render()
	{
		//---------------------------------------
		// 게임 관련 객체를 렌더링 합니다.
		//---------------------------------------
		_heading?.DrawText("ARENA SURVIVOR", new Rect(28, 18, 920, 52), new Color4(.92f, .95f, 1, 1));
		for (int y = 112; y < 544; y += 48)
		{
			for (int x = 48; x < 912; x += 48)
			{
				DrawSprite(y == 112 || y == 496 || x == 48 || x == 864 ? "wall" : "floor", x, y, 48);
			}
		}

		DrawSprite("chaser", 420, 250, 48);
		DrawSprite("charger", 600, 250, 48);
		DrawSprite("boss", 704, 380, 80);
		DrawSprite("player", _playerPosition.X, _playerPosition.Y, PlayerSize);
		DrawSprite("sword", _playerPosition.X + 50, _playerPosition.Y + 8, 32);
	}

	private void DrawSprite(string name, float x, float y, float size)
	{
		_textures[name].Draw(
			new Rect(x, y, size, size),
			new Rect(0.0f, 0.0f, 16.0f, 16.0f),
			interpolationMode: BitmapInterpolationMode.NearestNeighbor);
	}

	public override void Dispose()
	{
		//---------------------------------------
		// 게임 관련 객체를 해제합니다.
		//---------------------------------------
		_backgroundMusic?.Stop();
		_backgroundMusic?.Dispose();
		_backgroundMusic = null;
		foreach (G2Texture texture in _textures.Values)
		{
			texture.Dispose();
		}
		_textures.Clear();
		_heading?.Dispose();
		_heading = null;
		base.Dispose();
	}
}
