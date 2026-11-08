using System.Numerics;

// Rendering-independent rules for the single arena in docs/기획서.md.
internal enum StageState { Title, Spawning, Fighting, Intermission, Paused, Victory, Defeat }
internal enum EnemyKind { Chaser, Charger, Boss }
internal enum EnemyAction { Approach, Telegraph, Attack, Recover }
internal readonly record struct StageInput(Vector2 Move, Vector2 Aim, bool Attack = false, bool Dodge = false);

internal sealed class ArenaEnemy(EnemyKind kind, Vector2 position)
{
    public EnemyKind Kind { get; } = kind;
    public Vector2 Position = position;
    public Vector2 Direction = Vector2.UnitY;
    public int MaxHealth => Kind switch { EnemyKind.Chaser => 2, EnemyKind.Charger => 3, _ => 15 };
    public int Health = kind switch { EnemyKind.Chaser => 2, EnemyKind.Charger => 3, _ => 15 };
    public float Radius => Kind == EnemyKind.Boss ? 34 : 20;
    public EnemyAction Action;
    public float Timer;
    public float HitFlash;
    public bool AreaAttack;
    public bool NextAreaAttack;
}

internal sealed class ArenaStage
{
    public const float Left = 96, Right = 864, Top = 160, Bottom = 544;
    public const float PlayerRadius = 20, AttackReach = 76, AttackInterval = .4f;
    public const float DodgeDuration = .2f, DodgeCooldown = 1, HurtInvulnerability = .7f;
    public StageState State { get; private set; } = StageState.Title;
    private StageState beforePause;
    public Vector2 Player { get; private set; } = new(480, 352);
    public Vector2 Facing { get; private set; } = Vector2.UnitX;
    public Vector2 SwingDirection { get; private set; } = Vector2.UnitX;
    private Vector2 dodgeDirection;
    public int Health { get; private set; } = 5;
    public int Wave { get; private set; }
    public int Defeated { get; private set; }
    public float Elapsed { get; private set; }
    public float PhaseTime { get; private set; }
    public float AttackCooldownLeft { get; private set; }
    public float SwingTime { get; private set; }
    public float DodgeTime { get; private set; }
    public float DodgeCooldownLeft { get; private set; }
    public float Invulnerability { get; private set; }
    private readonly List<ArenaEnemy> enemies = new();
    public IReadOnlyList<ArenaEnemy> Enemies => enemies;
    public event Action<string>? Sound;
    public bool IsRunning => State is StageState.Spawning or StageState.Fighting or StageState.Intermission;

    public void Start()
    {
        Player = new(480, 352); Facing = SwingDirection = Vector2.UnitX;
        dodgeDirection = Vector2.Zero;
        Health = 5; Wave = 1; Defeated = 0; Elapsed = 0;
        AttackCooldownLeft = SwingTime = DodgeTime = DodgeCooldownLeft = Invulnerability = 0;
        PrepareWave();
    }

    public void ReturnToTitle() { enemies.Clear(); State = StageState.Title; }
    public void TogglePause()
    {
        if (State == StageState.Paused) State = beforePause;
        else if (IsRunning) { beforePause = State; State = StageState.Paused; }
    }

    private void PrepareWave()
    {
        enemies.Clear();
        if (Wave == 3) enemies.Add(new(EnemyKind.Boss, new(480, 220)));
        else
        {
            enemies.Add(new(EnemyKind.Chaser, new(152, 220)));
            enemies.Add(new(EnemyKind.Chaser, new(808, 220)));
            enemies.Add(new(EnemyKind.Chaser, new(480, 496)));
            if (Wave == 2)
            {
                enemies.Add(new(EnemyKind.Charger, new(152, 484)));
                enemies.Add(new(EnemyKind.Charger, new(808, 484)));
            }
        }
        PhaseTime = 1; State = StageState.Spawning;
    }

    public static Vector2 ClampToArena(Vector2 p, float radius) => new(
        Math.Clamp(p.X, Left + radius, Right - radius), Math.Clamp(p.Y, Top + radius, Bottom - radius));
    private static Vector2 Direction(Vector2 delta, Vector2 fallback) => delta.LengthSquared() > .001f ? Vector2.Normalize(delta) : fallback;
    private static float Countdown(float value, float dt) => Math.Max(0, value - dt);

    public void Tick(float dt, StageInput input)
    {
        if (!IsRunning || dt <= 0 || !float.IsFinite(dt)) return;
        // Small steps prevent fast charges tunnelling through the player after a slow frame.
        dt = Math.Min(dt, .1f);
        while (dt > .00001f && IsRunning)
        {
            float step = Math.Min(dt, 1f / 120);
            Step(step, input);
            input = input with { Dodge = false };
            dt -= step;
        }
    }

    private void Step(float dt, StageInput input)
    {
        Elapsed += dt;
        AttackCooldownLeft = Countdown(AttackCooldownLeft, dt);
        SwingTime = Countdown(SwingTime, dt);
        DodgeCooldownLeft = Countdown(DodgeCooldownLeft, dt);
        Invulnerability = Countdown(Invulnerability, dt);
        Facing = Direction(input.Aim - Player, Facing);
        Vector2 move = input.Move.LengthSquared() > 1 ? Vector2.Normalize(input.Move) : input.Move;
        if (input.Dodge && DodgeCooldownLeft <= 0)
        {
            dodgeDirection = Direction(move, Facing);
            DodgeTime = DodgeDuration; DodgeCooldownLeft = DodgeCooldown;
            Sound?.Invoke("dodge");
        }
        bool dodging = DodgeTime > 0;
        Player = ClampToArena(Player + (dodging ? dodgeDirection * 650 : move * 220) * dt, PlayerRadius);
        if (!dodging && input.Attack && AttackCooldownLeft <= 0) Attack();

        if (State == StageState.Intermission)
        {
            PhaseTime = Countdown(PhaseTime, dt);
            if (PhaseTime <= 0) { Wave++; PrepareWave(); }
        }
        else if (State == StageState.Spawning)
        {
            PhaseTime = Countdown(PhaseTime, dt);
            if (PhaseTime <= 0) State = StageState.Fighting;
        }
        else if (State == StageState.Fighting)
        {
            foreach (var enemy in enemies)
            {
                UpdateEnemy(enemy, dt);
                if (State == StageState.Defeat) break;
            }
            if (State == StageState.Fighting && enemies.Count == 0)
            {
                if (Wave == 3) { State = StageState.Victory; Sound?.Invoke("victory"); }
                else { Health = Math.Min(5, Health + 1); PhaseTime = 5; State = StageState.Intermission; }
            }
        }
        DodgeTime = Countdown(DodgeTime, dt);
    }

    private void Attack()
    {
        AttackCooldownLeft = AttackInterval; SwingTime = .16f; SwingDirection = Facing;
        Sound?.Invoke("attack");
        if (State != StageState.Fighting) return;
        bool hit = false;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            Vector2 delta = enemy.Position - Player;
            if (delta.Length() > AttackReach + enemy.Radius || Vector2.Dot(Direction(delta, Facing), Facing) < .45f) continue;
            enemy.Health--; enemy.HitFlash = .15f; hit = true;
            if (enemy.Health <= 0) { enemies.RemoveAt(i); Defeated++; }
        }
        if (hit) Sound?.Invoke("enemy_hit");
    }

    private void UpdateEnemy(ArenaEnemy enemy, float dt)
    {
        enemy.HitFlash = Countdown(enemy.HitFlash, dt);
        enemy.Timer = Countdown(enemy.Timer, dt);
        Vector2 delta = Player - enemy.Position;
        switch (enemy.Action)
        {
            case EnemyAction.Approach:
                bool close = delta.Length() <= (enemy.Kind == EnemyKind.Chaser ? 62 : 280);
                if (close)
                {
                    enemy.Direction = Direction(delta, Vector2.UnitY);
                    enemy.AreaAttack = enemy.Kind == EnemyKind.Boss && enemy.NextAreaAttack;
                    enemy.Action = EnemyAction.Telegraph;
                    enemy.Timer = enemy.Kind == EnemyKind.Chaser ? .55f : .8f;
                }
                else enemy.Position = ClampToArena(enemy.Position + Direction(delta, Vector2.UnitY) * (enemy.Kind == EnemyKind.Chaser ? 100 : 85) * dt, enemy.Radius);
                break;
            case EnemyAction.Telegraph:
                if (enemy.Timer <= 0)
                {
                    enemy.Action = EnemyAction.Attack;
                    enemy.Timer = enemy.Kind == EnemyKind.Chaser || enemy.AreaAttack ? .12f : .48f;
                }
                break;
            case EnemyAction.Attack:
                if (enemy.Kind == EnemyKind.Chaser)
                {
                    if (delta.Length() <= 72 && Vector2.Dot(Direction(delta, enemy.Direction), enemy.Direction) >= .35f) Hurt();
                }
                else if (enemy.AreaAttack)
                {
                    if (delta.Length() <= 120 + PlayerRadius) Hurt();
                }
                else
                {
                    enemy.Position = ClampToArena(enemy.Position + enemy.Direction * 560 * dt, enemy.Radius);
                    if (Vector2.Distance(enemy.Position, Player) < enemy.Radius + PlayerRadius) Hurt();
                }
                if (enemy.Timer <= 0)
                {
                    enemy.Action = EnemyAction.Recover; enemy.Timer = 1;
                    if (enemy.Kind == EnemyKind.Boss) enemy.NextAreaAttack = !enemy.AreaAttack;
                }
                break;
            case EnemyAction.Recover:
                if (enemy.Timer <= 0) enemy.Action = EnemyAction.Approach;
                break;
        }
    }

    private void Hurt()
    {
        if (Invulnerability > 0 || DodgeTime > 0 || State != StageState.Fighting) return;
        Health--; Invulnerability = HurtInvulnerability; Sound?.Invoke("hurt");
        if (Health <= 0) { State = StageState.Defeat; Sound?.Invoke("defeat"); }
    }
}
