using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.UI;

/// <summary>
/// Base de todos os elementos de UI. Trabalha em coordenadas virtuais da engine.
/// Passe mousePos já convertido via Core.ScreenToVirtual antes de chamar Update.
/// </summary>
public abstract class UIElement
{
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public virtual void Update(GameTime gameTime, Vector2 mousePos, bool mouseClicked) { }
    public abstract void Draw(SpriteBatch spriteBatch);
}
