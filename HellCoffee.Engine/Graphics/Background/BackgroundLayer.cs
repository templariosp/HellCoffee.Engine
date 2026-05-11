using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Graphics.Camera;

namespace HellCoffee.Engine.Graphics.Background;

public abstract class BackgroundLayer
{
    public bool IsVisible { get; set; } = true;
    public int ZOrder { get; set; } = 0;

    public abstract void Update(GameTime gameTime, Camera2D camera);
    public abstract void Draw(SpriteBatch spriteBatch, Camera2D camera);
}
