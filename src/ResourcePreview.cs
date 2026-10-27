using Vortice.Direct2D1;
using Vortice.Mathematics;

// Static resource display only. No movement, combat, health, enemies or game rules.
sealed class ResourcePreview(bool gallery) : G2AppBase
{
    public override System.Drawing.Size ScreenSize => new(960, 640);
    public override string GameName => "Arena Survivor";
    public static readonly string[] SpriteNames = { "player", "chaser", "charger", "boss", "sword", "floor", "wall" };
    private static readonly int[] Sizes = { 48, 48, 48, 80, 32, 48, 48 };
    private readonly Dictionary<string, G2Texture> textures = new();
    private G2Font heading = null!, body = null!;
    private static readonly Color4 White = new(.92f, .95f, 1, 1), Gold = new(1, .78f, .4f, 1);

    protected override void Initialize()
    {
        ClearColor = new Color4(.055f, .07f, .10f, 1);
        foreach (var name in SpriteNames) textures.Add(name, new G2Texture($"resource/graphics/{name}.png"));
        heading = new G2Font("Malgun Gothic", 26);
        body = new G2Font("Malgun Gothic", 16);
    }
    protected override void Update()
    {
    }
    private void Label(string value, float x, float y, float width, bool title = false)
    { (title ? heading : body).DrawText(value, new Rect(x, y, width, 52), title ? White : Gold); }
    private void Sprite(string name, float x, float y, int size)
    { textures[name].Draw(new Rect(x, y, size, size), new Rect(0, 0, 16, 16), 1, BitmapInterpolationMode.NearestNeighbor); }
    protected override void Render()
    {
        Label("ARENA SURVIVOR", 28, 18, 920, true);
        if (gallery)
        {
            for (int i = 0; i < SpriteNames.Length; i++)
            {
                float x = 28 + i * 130;
                Label(SpriteNames[i], x, 130, 120);
                Sprite(SpriteNames[i], x + 42, 210, 16);
                Sprite(SpriteNames[i], x + (100 - Sizes[i]) / 2, 350 - Sizes[i] / 2, Sizes[i]);
            }
        }
        else
        {
            for (int y = 112; y < 544; y += 48)
                for (int x = 48; x < 912; x += 48)
                    Sprite(y == 112 || y == 496 || x == 48 || x == 864 ? "wall" : "floor", x, y, 48);
            Sprite("player", 220, 330, 48); Sprite("sword", 270, 338, 32);
            Sprite("chaser", 420, 250, 48); Sprite("charger", 600, 250, 48); Sprite("boss", 704, 380, 80);
        }
    }
    public override void Dispose()
    {
        foreach (var texture in textures.Values) texture.Dispose();
        body?.Dispose(); heading?.Dispose();
        base.Dispose();
    }
}
