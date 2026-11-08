using System.Numerics;

int passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    Console.WriteLine($"PASS {name}"); passed++;
}
void Advance(ArenaStage s, float seconds, StageInput? input = null)
{
    int frames = (int)MathF.Round(seconds * 120);
    for (int i = 0; i < frames; i++) s.Tick(1f / 120, input ?? new(Vector2.Zero, s.Player + Vector2.UnitX));
}
ArenaStage Fighting()
{
    var s = new ArenaStage(); s.Start(); Advance(s, 1.02f); return s;
}
void Park(ArenaStage s)
{
    foreach (var e in s.Enemies) { e.Position = new(150, 210); e.Action = EnemyAction.Recover; e.Timer = 100; }
}
void FinishWave(ArenaStage s)
{
    foreach (var e in s.Enemies) { e.Position = s.Player + new Vector2(45, 0); e.Action = EnemyAction.Recover; e.Timer = 100; }
    for (int i = 0; i < 1200 && s.State == StageState.Fighting; i++)
        s.Tick(1f / 120, new(Vector2.Zero, s.Player + Vector2.UnitX, Attack: true));
}

var stage = new ArenaStage();
Check(stage.State == StageState.Title, "Title is initial state");
stage.Start(); Advance(stage, .98f);
Check(stage.State == StageState.Spawning && stage.Enemies.Count == 3 && stage.Enemies.All(e => e.Health == 2), "Wave 1 composition and one-second spawn warning");
Advance(stage, .04f); Check(stage.State == StageState.Fighting, "Spawn activates all enemies together");
Park(stage);
var origin = stage.Player;
Advance(stage, .2f, new(Vector2.One, stage.Player + Vector2.UnitX));
Check(Math.Abs(Vector2.Distance(origin, stage.Player) - 44) < .1f, "Diagonal movement is normalized");
Advance(stage, 5, new(Vector2.One, Vector2.Zero));
Check(stage.Player == new Vector2(ArenaStage.Right - 20, ArenaStage.Bottom - 20), "Player stays inside walls");

stage = Fighting(); Park(stage);
stage.Enemies[0].Position = stage.Player + new Vector2(50, 0);
stage.Enemies[1].Position = stage.Player + new Vector2(55, 10);
stage.Enemies[2].Position = stage.Player - new Vector2(50, 0);
var attack = new StageInput(Vector2.Zero, stage.Player + Vector2.UnitX, Attack: true);
stage.Tick(.001f, attack);
Check(stage.Enemies.All(e => e.Health == 2), "Thrust damage waits for full extension");
Advance(stage, .09f, attack);
Check(stage.Enemies[0].Health == 1 && stage.Enemies[1].Health == 1 && stage.Enemies[2].Health == 2, "Melee damages multiple front enemies but not rear enemies");
Advance(stage, .29f, attack);
Check(stage.Enemies.Count == 3, "Attack cannot repeat before 0.4 seconds");
Advance(stage, .12f, attack);
Check(stage.Enemies.Count == 1 && stage.Defeated == 2, "Held attack repeats after cooldown");

stage = Fighting(); Park(stage);
stage.Enemies[0].Position = stage.Player + new Vector2(70, 45);
stage.Enemies[1].Position = stage.Player + new Vector2(130, 0);
stage.Enemies[2].Position = stage.Player + new Vector2(100, 0);
stage.Tick(.001f, new(Vector2.Zero, stage.Player + Vector2.UnitX, Attack: true));
Advance(stage, .1f, new(Vector2.Zero, stage.Player + Vector2.UnitY));
Check(stage.Enemies[0].Health == 2 && stage.Enemies[1].Health == 2 && stage.Enemies[2].Health == 1, "Narrow thrust excludes side/out-of-reach targets and locks attack direction");
Advance(stage, .2f);
Check(stage.Enemies[2].Health == 1, "Retracting spear cannot damage a target twice");

stage = Fighting(); Park(stage);
stage.Enemies[0].Position = stage.Player + new Vector2(50, 0);
stage.Tick(.001f, new(Vector2.Zero, stage.Player + Vector2.UnitX, Attack: true));
stage.Tick(.001f, new(Vector2.Zero, stage.Player + Vector2.UnitX, Dodge: true));
Advance(stage, .1f);
Check(stage.Enemies[0].Health == 2 && stage.ThrustTime == 0, "Dodge cancels an unfinished thrust");

stage = Fighting(); Park(stage); origin = stage.Player;
stage.Tick(1f / 120, new(Vector2.Zero, origin + Vector2.UnitX, Dodge: true));
stage.Tick(.001f, new(Vector2.Zero, origin + Vector2.UnitX, Attack: true));
Check(stage.ThrustTime == 0 && stage.AttackCooldownLeft == 0, "Attacking is disabled during dodge (approved design rule)");
Advance(stage, .19f);
Check(stage.Player.X > origin.X + 120 && stage.DodgeCooldownLeft > .79f, "Stationary dodge follows aim for 0.2 seconds with one-second cooldown");
Advance(stage, .03f); origin = stage.Player;
stage.Tick(.01f, new(Vector2.Zero, origin + Vector2.UnitX, Dodge: true));
Check(stage.Player == origin && stage.DodgeTime == 0, "Dodge cannot be retriggered during cooldown");

stage = Fighting(); Park(stage);
var attacker = stage.Enemies[0];
void ReadyHit()
{
    attacker.Position = stage.Player; attacker.Action = EnemyAction.Attack; attacker.Timer = .1f; attacker.Direction = Vector2.UnitX;
}
ReadyHit(); stage.Tick(.01f, new(Vector2.Zero, stage.Player + Vector2.UnitX, Dodge: true));
Check(stage.Health == 5, "Dodge grants damage immunity");
Advance(stage, .25f); ReadyHit(); stage.Tick(.01f, new(Vector2.Zero, stage.Player));
Check(stage.Health == 4, "Enemy hit deals exactly one damage");
ReadyHit(); Advance(stage, .1f);
Check(stage.Health == 4, "Repeated enemy hits respect 0.7-second invulnerability");
float elapsed = stage.Elapsed, invulnerability = stage.Invulnerability;
stage.TogglePause(); Advance(stage, 2);
Check(stage.State == StageState.Paused && stage.Elapsed == elapsed && stage.Invulnerability == invulnerability, "Pause freezes simulation and cooldowns");
stage.TogglePause(); FinishWave(stage);
Check(stage.State == StageState.Intermission && stage.Health == 5, "Wave completion heals one HP");
Advance(stage, 4.98f); Check(stage.Wave == 1, "Intermission lasts five seconds");
Advance(stage, .04f);
Check(stage.Wave == 2 && stage.State == StageState.Spawning && stage.Enemies.Count(e => e.Kind == EnemyKind.Chaser) == 3 && stage.Enemies.Count(e => e.Kind == EnemyKind.Charger) == 2, "Wave 2 composition");
Advance(stage, 1.02f); Park(stage);
var charger = stage.Enemies.First(e => e.Kind == EnemyKind.Charger);
charger.Position = stage.Player - new Vector2(200, 0); charger.Action = EnemyAction.Approach; charger.Timer = 0;
stage.Tick(.001f, new(Vector2.Zero, stage.Player));
Vector2 locked = charger.Direction;
Advance(stage, .77f, new(new Vector2(0, 1), stage.Player));
Check(charger.Action == EnemyAction.Telegraph && charger.Direction == locked, "Charger locks aim and telegraphs for 0.8 seconds");
Advance(stage, .05f);
Check(charger.Action == EnemyAction.Attack && charger.Direction == locked, "Charge keeps locked direction");
FinishWave(stage); Advance(stage, 6.05f);
Check(stage.Wave == 3 && stage.Enemies.Count == 1 && stage.Enemies[0].Health == 15, "Wave 3 has one 15-HP boss");
var boss = stage.Enemies[0];
boss.Position = stage.Player - new Vector2(200, 0); boss.Action = EnemyAction.Approach;
stage.Tick(.001f, new(Vector2.Zero, stage.Player));
Check(!boss.AreaAttack && boss.Action == EnemyAction.Telegraph, "Boss starts with charge");
Advance(stage, 1.30f);
Check(boss.Action == EnemyAction.Recover && boss.Timer > .95f, "Boss recovers for one second after attack");
Advance(stage, 1.03f);
Check(boss.AreaAttack && boss.Action == EnemyAction.Telegraph, "Boss alternates charge and area attack");
FinishWave(stage);
Check(stage.State == StageState.Victory && stage.Defeated == 9, "All three Waves can complete and trigger victory");
stage.Start();
Check(stage.Health == 5 && stage.Wave == 1 && stage.Elapsed == 0 && stage.Defeated == 0 && stage.DodgeTime == 0 && stage.Invulnerability == 0 && stage.AttackCooldownLeft == 0, "Restart resets all gameplay state");
Advance(stage, 1.02f); Park(stage); attacker = stage.Enemies[0];
for (int i = 0; i < 5; i++) { ReadyHit(); stage.Tick(.01f, new(Vector2.Zero, stage.Player)); Advance(stage, .72f); }
Check(stage.State == StageState.Defeat && stage.Health == 0, "Five separated hits trigger defeat");
elapsed = stage.Elapsed; Advance(stage, 2);
Check(stage.Elapsed == elapsed, "Result freezes play time");
stage.Start(); Check(stage.State == StageState.Spawning && stage.Health == 5, "Restart also works after defeat");
stage.ReturnToTitle(); Check(stage.State == StageState.Title && stage.Enemies.Count == 0, "Return to title clears enemies");
foreach (var facing in new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY, Vector2.Normalize(Vector2.One) })
{
    stage = Fighting(); Park(stage);
    Vector2 side = new(-facing.Y, facing.X);
    stage.Enemies[0].Position = stage.Player + facing * 70 + side * 35;
    stage.Enemies[1].Position = stage.Player + facing * 70 - side * 20;
    stage.Tick(.001f, new(Vector2.Zero, stage.Player + facing, Attack: true));
    Advance(stage, .1f);
    Check(stage.Enemies[0].Health == 1 && stage.Enemies[1].Health == 2,
        $"Hand-offset hitbox follows spear for direction {facing}");
}
Console.WriteLine($"{passed} checks passed.");
