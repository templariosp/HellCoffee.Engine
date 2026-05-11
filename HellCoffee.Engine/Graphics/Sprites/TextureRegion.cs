using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.Graphics.Sprites;

public class TextureRegion
{
    public Texture2D Texture { get; }
    public Rectangle SourceRectangle { get; }

    public int X => SourceRectangle.X;
    public int Y => SourceRectangle.Y;
    public int Width => SourceRectangle.Width;
    public int Height => SourceRectangle.Height;

    public TextureRegion(Texture2D texture, int x, int y, int width, int height)
    {
        Texture = texture;
        SourceRectangle = new Rectangle(x, y, width, height);
    }

    public TextureRegion(Texture2D texture, Rectangle sourceRectangle)
    {
        Texture = texture;
        SourceRectangle = sourceRectangle;
    }

    public TextureRegion(Texture2D texture)
    {
        Texture = texture;
        SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        spriteBatch.Draw(Texture, position, SourceRectangle, color);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        spriteBatch.Draw(Texture, position, SourceRectangle, color,
            rotation, origin, scale, effects, layerDepth);
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color)
    {
        spriteBatch.Draw(Texture, destinationRectangle, SourceRectangle, color);
    }
}
