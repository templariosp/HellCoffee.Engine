using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine;
using HellCoffee.Engine.Graphics.Camera;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Background;

/// <summary>
/// Background com efeito parallax: se move a uma fração da velocidade da câmera.
/// Pode ser configurado para repetir horizontalmente, verticalmente ou em ambos.
///
/// ParallaxFactor = 0.0f → completamente estático (fundo distante)
/// ParallaxFactor = 0.5f → move a metade da velocidade da câmera
/// ParallaxFactor = 1.0f → move igual à câmera (sem parallax)
/// </summary>
public class ParallaxBackground : BackgroundLayer
{
    private readonly TextureRegion _region;

    public float ParallaxFactorX { get; set; }
    public float ParallaxFactorY { get; set; }
    public bool RepeatX { get; set; } = true;
    public bool RepeatY { get; set; } = false;
    public Color Tint { get; set; } = Color.White;
    public Vector2 Offset { get; set; } = Vector2.Zero;

    public ParallaxBackground(TextureRegion region, float parallaxFactorX = 0.5f, float parallaxFactorY = 0f)
    {
        _region = region;
        ParallaxFactorX = parallaxFactorX;
        ParallaxFactorY = parallaxFactorY;
    }

    public override void Update(GameTime gameTime, Camera2D camera) { }

    public override void Draw(SpriteBatch spriteBatch, Camera2D camera)
    {
        if (!IsVisible) return;

        int screenW = Core.VirtualWidth;
        int screenH = Core.VirtualHeight;
        int tw = _region.Width;
        int th = _region.Height;

        float camX = camera?.Position.X ?? 0f;
        float camY = camera?.Position.Y ?? 0f;

        float scrollX = camX * ParallaxFactorX + Offset.X;
        float scrollY = camY * ParallaxFactorY + Offset.Y;

        if (RepeatX)
        {
            float modX = scrollX % tw;
            if (modX > 0) modX -= tw;
            for (float x = modX; x < screenW; x += tw)
            {
                float drawY = RepeatY ? DrawRepeatY(spriteBatch, x, scrollY, screenH, th) : screenH - th - scrollY;
                spriteBatch.Draw(_region.Texture, new Vector2(x, RepeatY ? 0 : drawY), _region.SourceRectangle, Tint);
            }
        }
        else if (RepeatY)
        {
            float modY = scrollY % th;
            if (modY > 0) modY -= th;
            for (float y = modY; y < screenH; y += th)
                spriteBatch.Draw(_region.Texture, new Vector2(screenW / 2f - tw / 2f - scrollX, y), _region.SourceRectangle, Tint);
        }
        else
        {
            spriteBatch.Draw(_region.Texture,
                new Vector2(screenW / 2f - tw / 2f - scrollX, screenH - th - scrollY),
                _region.SourceRectangle, Tint);
        }
    }

    private float DrawRepeatY(SpriteBatch sb, float x, float scrollY, int screenH, int th)
    {
        float modY = scrollY % th;
        if (modY > 0) modY -= th;
        for (float y = modY; y < screenH; y += th)
            sb.Draw(_region.Texture, new Vector2(x, y), _region.SourceRectangle, Tint);
        return 0f;
    }
}
