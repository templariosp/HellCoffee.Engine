using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HellCoffee.Engine;
using HellCoffee.Engine.Graphics.Tilemap;

namespace HellCoffee.Engine.Debug;

public class DebugOverlay
{
    private Texture2D _pixel;
    private KeyboardState _prevKb;
    private KeyboardState _currKb;

    public bool ShowCollisions { get; private set; }
    public bool ShowFps       { get; private set; }
    public bool ShowTileGrid  { get; private set; }
    public bool Invincible    { get; private set; }

    private int _frameCount;
    private float _fpsTimer;
    public float CurrentFps { get; private set; }

    private readonly List<string> _infoLines = new();

    internal void LoadContent(ContentManager content)
    {
        _pixel = new Texture2D(Core.GD, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void AddInfoLine(string line) => _infoLines.Add(line);
    public void ClearInfoLines() => _infoLines.Clear();

    internal void Update(GameTime gameTime)
    {
        _prevKb = _currKb;
        _currKb = Keyboard.GetState();

#if DEBUG
        if (JustPressed(Keys.F1)) ShowCollisions = !ShowCollisions;
        if (JustPressed(Keys.F2)) ShowFps        = !ShowFps;
        if (JustPressed(Keys.F3)) ShowTileGrid   = !ShowTileGrid;
        if (JustPressed(Keys.F4)) Invincible     = !Invincible;
#endif

        _frameCount++;
        _fpsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_fpsTimer >= 1f)
        {
            CurrentFps = _frameCount / _fpsTimer;
            _frameCount = 0;
            _fpsTimer   = 0f;
        }
    }

    private bool JustPressed(Keys key)
        => _currKb.IsKeyDown(key) && _prevKb.IsKeyUp(key);

    public void Draw(SpriteBatch spriteBatch)
    {
        bool hasContent = ShowFps || ShowCollisions || ShowTileGrid || Invincible || _infoLines.Count > 0;
        if (!hasContent) return;

        spriteBatch.Begin();

        var lines = new List<string>();
        if (ShowFps)        lines.Add($"FPS:{CurrentFps:F0}");
        if (ShowCollisions) lines.Add("[F1] COLISOES ON");
        if (ShowTileGrid)   lines.Add("[F3] TILE GRID ON");
        if (Invincible)     lines.Add("[F4] INVENCIVEL");
        lines.AddRange(_infoLines);

        const int scale = 1;
        int lineH = PixelFont.GlyphH * scale + 1;

        for (int i = 0; i < lines.Count; i++)
        {
            var size = PixelFont.Measure(lines[i], scale);
            var pos  = new Vector2(Core.VirtualWidth - size.X - 2, 2 + i * lineH);
            PixelFont.DrawWithShadow(spriteBatch, _pixel, lines[i], pos, Color.Yellow, scale);
        }

        spriteBatch.End();
    }

    public void DrawCollisionRect(SpriteBatch spriteBatch, Rectangle rect, Color? color = null)
    {
        if (!ShowCollisions) return;
        var c = color ?? Color.Red;
        DrawRect(spriteBatch, rect, c * 0.25f, c);
    }

    public void DrawCollisionCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color? color = null)
    {
        if (!ShowCollisions) return;
        var c = color ?? Color.Cyan;
        const int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float a1 = MathF.PI * 2 * i / segments;
            float a2 = MathF.PI * 2 * (i + 1) / segments;
            var p1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;
            var p2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;
            DrawLine(spriteBatch, p1, p2, c);
        }
    }

    public void DrawTileGrid(SpriteBatch spriteBatch, TilemapLayer layer, Color? color = null)
    {
        if (!ShowTileGrid) return;
        var c = (color ?? Color.White) * 0.2f;
        for (int row = 0; row <= layer.Rows; row++)
            DrawLine(spriteBatch,
                new Vector2(0, row * layer.TileHeight),
                new Vector2(layer.PixelWidth, row * layer.TileHeight), c);
        for (int col = 0; col <= layer.Columns; col++)
            DrawLine(spriteBatch,
                new Vector2(col * layer.TileWidth, 0),
                new Vector2(col * layer.TileWidth, layer.PixelHeight), c);
    }

    private void DrawRect(SpriteBatch sb, Rectangle r, Color fill, Color border)
    {
        sb.Draw(_pixel, r, fill);
        sb.Draw(_pixel, new Rectangle(r.X,         r.Y,          r.Width,  1), border);
        sb.Draw(_pixel, new Rectangle(r.X,         r.Bottom - 1, r.Width,  1), border);
        sb.Draw(_pixel, new Rectangle(r.X,         r.Y,          1, r.Height), border);
        sb.Draw(_pixel, new Rectangle(r.Right - 1, r.Y,          1, r.Height), border);
    }

    private void DrawLine(SpriteBatch sb, Vector2 from, Vector2 to, Color color)
    {
        var diff = to - from;
        float length = diff.Length();
        if (length < 0.01f) return;
        float angle = MathF.Atan2(diff.Y, diff.X);
        sb.Draw(_pixel, from, null, color, angle, Vector2.Zero,
                new Vector2(length, 1), SpriteEffects.None, 0f);
    }
}
