using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

sealed class GameMain : G2AppBase
{
    public override System.Drawing.Size ScreenSize => GameGlobal.ScreenSize;
    public override string GameName => GameGlobal.GameName;
    private readonly ArenaStage stage = new();
    private readonly Dictionary<string, G2Texture> textures = new();
    private readonly Dictionary<string, G2Audio> sounds = new();
    private G2Font title = null!, body = null!, small = null!;
    private ID2D1SolidColorBrush brush = null!;
    private static readonly Color4 Ink = new(.035f, .05f, .08f, 1), White = new(.94f, .96f, 1, 1);
    private static readonly Color4 Gold = new(1, .77f, .35f, 1), Teal = new(.25f, .9f, .8f, 1), Red = new(1, .3f, .3f, 1);
    private static readonly Color4 GoldText = new(.80f, .65f, .36f, 1), TealText = new(.31f, .73f, .66f, 1);
    private Vector2 mouse;
    private float stepTimer;

    protected override void Initialize()
    {
        ClearColor = Ink;
        foreach (string name in ResourcePreview.SpriteNames) textures.Add(name, new G2Texture($"resource/graphics/{name}.png"));
        foreach (string name in new[] { "attack", "dodge", "hurt", "enemy_hit", "step", "victory", "defeat", "battle" })
            sounds.Add(name, new G2Audio($"resource/audio/{name}.wav"));
        title = new G2Font("Malgun Gothic", 32);
        body = new G2Font("Malgun Gothic", 19);
        small = new G2Font("Malgun Gothic", 14);
        brush = RenderTarget.CreateSolidColorBrush(White);
        stage.Sound += PlaySound;
    }
    private void PlaySound(string name) => sounds[name].Play();
    private bool Held(Keys key) => Input.IsKeyDown(key) || Input.IsKeyPress(key);
    private static bool Inside(Vector2 p, float x, float y, float w, float h) => p.X >= x && p.X <= x + w && p.Y >= y && p.Y <= y + h;
    private bool Click(float y) => Input.IsButtonDown(MouseButtons.Left) && Inside(mouse, 330, y, 300, 48);
    private void StartStage()
    {
        StopSounds(); stage.Start(); stepTimer = 0; sounds["battle"].Play(true);
    }
    protected override void Update()
    {
        mouse = new(Input.MousePosition.X, Input.MousePosition.Y);
        if (Form.ActiveForm == null)
        {
            if (stage.IsRunning) { stage.TogglePause(); StopSounds(); }
            return;
        }
        if (stage.State == StageState.Title)
        {
            if (Input.IsKeyDown(Keys.Enter) || Click(366)) StartStage();
            else if (Click(426)) Close();
            return;
        }
        if (Input.IsKeyDown(Keys.Escape) && (stage.IsRunning || stage.State == StageState.Paused))
        {
            stage.TogglePause();
            if (stage.State == StageState.Paused) StopSounds();
            else sounds["battle"].Play(true);
            return;
        }
        if (stage.State == StageState.Paused)
        {
            if (Input.IsKeyDown(Keys.Enter) || Click(366)) { stage.TogglePause(); sounds["battle"].Play(true); }
            else if (Click(426)) { stage.ReturnToTitle(); StopSounds(); }
            return;
        }
        if (stage.State is StageState.Victory or StageState.Defeat)
        {
            if (Input.IsKeyDown(Keys.Enter) || Input.IsKeyDown(Keys.R) || Click(366)) StartStage();
            else if (Click(426)) { stage.ReturnToTitle(); StopSounds(); }
            return;
        }
        Vector2 move = new((Held(Keys.D) ? 1 : 0) - (Held(Keys.A) ? 1 : 0), (Held(Keys.S) ? 1 : 0) - (Held(Keys.W) ? 1 : 0));
        stage.Tick((float)DeltaTime, new StageInput(move, mouse,
            Input.IsButtonDown(MouseButtons.Left) || Input.IsButtonPress(MouseButtons.Left), Input.IsKeyDown(Keys.Space)));
        if (!stage.IsRunning) { sounds["battle"].Stop(); return; }
        stepTimer -= Math.Min((float)DeltaTime, .1f);
        if (move != Vector2.Zero && stage.DodgeTime <= 0 && stepTimer <= 0) { sounds["step"].Play(); stepTimer = .32f; }
    }
    private void StopSounds() { foreach (var sound in sounds.Values) sound.Stop(); }
    private void Box(float x, float y, float w, float h, Color4 color)
    { brush.Color = color; RenderTarget.FillRectangle(new Rect(x, y, w, h), brush); }
    private void Line(Vector2 a, Vector2 b, Color4 color, float width = 2)
    { brush.Color = color; RenderTarget.DrawLine(a, b, brush, width); }
    private void Ring(Vector2 p, float radius, Color4 color, float width = 2)
    { brush.Color = color; RenderTarget.DrawEllipse(new Ellipse(p, radius, radius), brush, width); }
    private void Text(string text, float x, float y, float width, Color4 color, G2Font? font = null)
    {
        Color4 textColor = color.Equals(Gold) ? GoldText : color.Equals(Teal) ? TealText : color;
        (font ?? body).DrawText(text, new Rect(x, y, width, 70), textColor);
    }
    private void Sprite(string name, Vector2 center, int size, float opacity = 1)
    { textures[name].Draw(new Rect(center.X - size / 2, center.Y - size / 2, size, size), new Rect(0, 0, 16, 16), opacity, BitmapInterpolationMode.NearestNeighbor); }
    private void Sector(Vector2 center, Vector2 direction, float radius, float halfAngle, Color4 color)
    {
        float angle = MathF.Atan2(direction.Y, direction.X);
        Vector2 previous = center + new Vector2(MathF.Cos(angle - halfAngle), MathF.Sin(angle - halfAngle)) * radius;
        Line(center, previous, color);
        for (int i = 1; i <= 24; i++)
        {
            float a = angle - halfAngle + halfAngle * 2 * i / 24;
            Vector2 next = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
            Line(previous, next, color, 3); previous = next;
        }
        Line(center, previous, color);
    }
    protected override void Render()
    {
        Text("ARENA SURVIVOR", 40, 18, 560, White, title);
        Text("STAGE 01  /  아레나 가디언", 655, 32, 280, Gold, small);
        for (int y = 112; y < 592; y += 48)
            for (int x = 48; x < 912; x += 48)
                Sprite(y == 112 || y == 544 || x == 48 || x == 864 ? "wall" : "floor", new(x + 24, y + 24), 48);
        foreach (var enemy in stage.Enemies)
        {
            if (stage.State == StageState.Spawning)
            {
                Ring(enemy.Position, enemy.Radius + 10, Gold, 3);
                Line(enemy.Position - new Vector2(12, 0), enemy.Position + new Vector2(12, 0), Gold);
                Line(enemy.Position - new Vector2(0, 12), enemy.Position + new Vector2(0, 12), Gold);
                continue;
            }
            if (enemy.Action is EnemyAction.Telegraph or EnemyAction.Attack)
            {
                var color = enemy.Action == EnemyAction.Attack ? Red : Gold;
                if (enemy.AreaAttack) Ring(enemy.Position, 120, color, 4);
                else if (enemy.Kind == EnemyKind.Chaser) Sector(enemy.Position, enemy.Direction, 72, 1.21f, color);
                else
                {
                    Vector2 side = new(-enemy.Direction.Y * enemy.Radius, enemy.Direction.X * enemy.Radius);
                    Vector2 end = enemy.Position + enemy.Direction * 269;
                    Line(enemy.Position + side, end + side, color);
                    Line(enemy.Position - side, end - side, color);
                    Line(end - side, end + side, color, 3);
                }
            }
            Sprite(enemy.Kind.ToString().ToLowerInvariant(), enemy.Position, enemy.Kind == EnemyKind.Boss ? 80 : 48, enemy.HitFlash > 0 ? .45f : 1);
            if (enemy.Action == EnemyAction.Recover) Ring(enemy.Position, enemy.Radius + 5, Teal);
            float w = enemy.Kind == EnemyKind.Boss ? 80 : 40;
            Box(enemy.Position.X - w / 2, enemy.Position.Y - enemy.Radius - 12, w, 5, Ink);
            Box(enemy.Position.X - w / 2, enemy.Position.Y - enemy.Radius - 12, w * enemy.Health / enemy.MaxHealth, 5, Red);
        }
        Sprite("player", stage.Player, 48, stage.Invulnerability > 0 && (int)(stage.Elapsed * 18) % 2 == 0 ? .4f : 1);
        if (stage.DodgeTime > 0) Ring(stage.Player, 30, Teal, 3);
        DrawSpear();
        DrawHud();
        Text("WASD 이동   |   마우스 조준 / 왼쪽 클릭 창 찌르기   |   Space 회피   |   Esc 일시정지", 70, 602, 840, White, small);
        if (stage.State == StageState.Spawning) Banner($"WAVE {stage.Wave}  ·  등장 위치를 확인하세요", $"{Math.Ceiling(stage.PhaseTime)}초 후 전투 시작");
        if (stage.State == StageState.Intermission) Banner($"WAVE {stage.Wave} CLEAR  ·  체력 +1 (최대 5)", $"다음 Wave까지 {Math.Ceiling(stage.PhaseTime)}초");
        if (stage.State == StageState.Title) Panel("아레나 서바이버", "3개의 Wave를 돌파하고 가디언을 처치하세요.", "시작  [Enter]", "종료");
        if (stage.State == StageState.Paused) Panel("일시정지", "공격 예고를 피하고, 청록색 회복 구간에 반격하세요.", "계속하기  [Enter / Esc]", "시작 화면으로");
        if (stage.State is StageState.Victory or StageState.Defeat)
            Panel(stage.State == StageState.Victory ? "승리 · STAGE CLEAR" : "패배 · 다시 도전하세요", $"플레이 시간 {TimeSpan.FromSeconds(stage.Elapsed):mm\\:ss}   /   처치 {stage.Defeated}마리", "다시하기  [Enter / R]", "시작 화면으로");
    }
    private void DrawSpear()
    {
        bool thrusting = stage.ThrustTime > 0;
        Vector2 direction = thrusting ? stage.ThrustDirection : stage.Facing;
        Vector2 side = new(-direction.Y, direction.X);
        float age = ArenaStage.ThrustDuration - stage.ThrustTime;
        float extension = !thrusting ? 0 : age < ArenaStage.ThrustWindup
            ? age / ArenaStage.ThrustWindup
            : stage.ThrustTime / (ArenaStage.ThrustDuration - ArenaStage.ThrustWindup);
        float reach = 46 + (ArenaStage.AttackReach - 46) * Math.Clamp(extension, 0, 1);
        Vector2 tip = stage.Player + direction * reach;
        Vector2 neck = tip - direction * 15;
        Vector2 tail = tip - direction * 60;
        Line(tail, neck, Ink, 7);
        Line(tail, neck, new Color4(.55f, .38f, .22f, 1), 4);
        // A simple shaft and metal arrowhead, drawn using the existing Direct2D API.
        Line(neck - side * 5, tip, White, 3);
        Line(neck + side * 5, tip, White, 3);
        Line(neck - side * 5, neck + side * 5, White, 3);
        Line(neck - direction * 4 - side * 3, neck - direction * 4 + side * 3, GoldText, 3);
    }
    private void DrawHud()
    {
        Text("HP", 50, 77, 45, White, small);
        for (int i = 0; i < 5; i++) Box(86 + i * 28, 78, 21, 17, i < stage.Health ? Red : new Color4(.25f, .25f, .3f, 1));
        Text($"WAVE {stage.Wave}/3   ·   남은 적 {stage.Enemies.Count}", 285, 75, 320, White, small);
        Text(stage.DodgeCooldownLeft <= 0 ? "회피 READY" : $"회피 {stage.DodgeCooldownLeft:0.0}s", 625, 75, 180, Teal, small);
        Text($"{TimeSpan.FromSeconds(stage.Elapsed):mm\\:ss}", 838, 75, 90, Gold, small);
    }
    private void Banner(string heading, string detail)
    {
        Box(210, 263, 540, 93, new Color4(.025f, .04f, .07f, .9f));
        Text(heading, 235, 277, 510, Gold);
        Text(detail, 235, 313, 510, White, small);
    }
    private void Panel(string heading, string detail, string primary, string secondary)
    {
        Box(0, 112, 960, 480, new Color4(.015f, .025f, .045f, .80f));
        Box(185, 204, 590, 304, Ink); Box(185, 204, 590, 3, Gold);
        Text(heading, 222, 235, 530, White, title);
        Text(detail, 222, 294, 530, Gold, small);
        Button(366, primary); Button(426, secondary);
    }
    private void Button(float y, string label)
    {
        Box(330, y, 300, 48, Inside(mouse, 330, y, 300, 48) ? new Color4(.18f, .35f, .4f, 1) : new Color4(.1f, .18f, .25f, 1));
        Text(label, 350, y + 10, 270, White);
    }
    public override void Dispose()
    {
        stage.Sound -= PlaySound;
        foreach (var sound in sounds.Values) sound.Dispose();
        foreach (var texture in textures.Values) texture.Dispose();
        title?.Dispose(); body?.Dispose(); small?.Dispose(); brush?.Dispose();
        base.Dispose();
    }
}
