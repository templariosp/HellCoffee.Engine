using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.UI;

/// <summary>
/// Painel com fundo sólido que pode conter outros elementos como filhos.
/// </summary>
public class UIPanel : UIElement
{
    private readonly List<UIElement> _children = new();

    public Color BackgroundColor { get; set; } = new Color(20, 20, 30, 220);
    public Color BorderColor { get; set; } = new Color(80, 80, 120);
    public int BorderWidth { get; set; } = 1;

    public void Add(UIElement element) => _children.Add(element);
    public void Remove(UIElement element) => _children.Remove(element);
    public void Clear() => _children.Clear();

    public override void Update(GameTime gameTime, Vector2 mousePos, bool mouseClicked)
    {
        if (!IsVisible || !IsEnabled) return;
        foreach (var child in _children)
            child.Update(gameTime, mousePos, mouseClicked);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var pixel = Core.Pixel;
        var b = Bounds;

        spriteBatch.Draw(pixel, b, BackgroundColor);

        if (BorderWidth > 0 && BorderColor.A > 0)
        {
            int bw = BorderWidth;
            spriteBatch.Draw(pixel, new Rectangle(b.X,          b.Y,           b.Width, bw), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(b.X,          b.Bottom - bw, b.Width, bw), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(b.X,          b.Y,           bw, b.Height), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(b.Right - bw, b.Y,           bw, b.Height), BorderColor);
        }

        foreach (var child in _children)
            child.Draw(spriteBatch);
    }
}
