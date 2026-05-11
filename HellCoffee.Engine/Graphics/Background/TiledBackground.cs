using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine;
using HellCoffee.Engine.Graphics.Camera;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Background;

/// <summary>
/// Background que repete a textura para preencher a tela infinitamente.
/// Suporta scroll automático para criar efeitos de movimento.
/// </summary>
public class TiledBackground : BackgroundLayer
{
    private readonly TextureRegion _region;
    private Vector2 _offset;
    private Vector2 _scrollSpeed;

    public Color Tint { get; set; } = Color.White;

    public TiledBackground(TextureRegion region, Vector2 scrollSpeed = default)
    {
        _region = region;
        _scrollSpeed = scrollSpeed;
    }

    public override void Update(GameTime gameTime, Camera2D camera)
    {
        _offset += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _offset.X %= _region.Width;
        _offset.Y %= _region.Height;
    }

    public override void Draw(SpriteBatch spriteBatch, Camera2D camera)
    {
        if (!IsVisible) return;

        int screenW = Core.VirtualWidth;
        int screenH = Core.VirtualHeight;
        int tw = _region.Width;
        int th = _region.Height;

        int startX = (int)(-_offset.X % tw) - tw;
        int startY = (int)(-_offset.Y % th) - th;

        for (int y = startY; y < screenH + th; y += th)
        {
            for (int x = startX; x < screenW + tw; x += tw)
            {
                spriteBatch.Draw(_region.Texture,
                    new Vector2(x, y),
                    _region.SourceRectangle,
                    Tint);
            }
        }
    }
}
