using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.Graphics.Sprites;

public class Sprite
{
    public TextureRegion Region { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    public float LayerDepth { get; set; } = 0f;

    public int Width => (int)(Region.Width * Scale.X);
    public int Height => (int)(Region.Height * Scale.Y);

    public Sprite(TextureRegion region)
    {
        Region = region;
    }

    public void CenterOrigin()
    {
        Origin = new Vector2(Region.Width / 2f, Region.Height / 2f);
    }

    public void FlipHorizontal(bool flip)
    {
        Effects = flip
            ? Effects | SpriteEffects.FlipHorizontally
            : Effects & ~SpriteEffects.FlipHorizontally;
    }

    public void FlipVertical(bool flip)
    {
        Effects = flip
            ? Effects | SpriteEffects.FlipVertically
            : Effects & ~SpriteEffects.FlipVertically;
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        Region.Draw(spriteBatch, position, Color, Rotation, Origin, Scale, Effects, LayerDepth);
    }
}
