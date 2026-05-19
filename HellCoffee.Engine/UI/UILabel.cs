using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Debug;

namespace HellCoffee.Engine.UI;

/// <summary>
/// Texto renderizado com a PixelFont embutida.
/// </summary>
public class UILabel : UIElement
{
    public string Text { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public bool HasShadow { get; set; } = true;
    public int Scale { get; set; } = 1;

    public UILabel(string text, Vector2 position, Color? color = null, int scale = 1)
    {
        Text = text;
        Position = position;
        Scale = scale;
        if (color.HasValue) TextColor = color.Value;
        RefreshSize();
    }

    /// <summary>Recalcula Width/Height a partir do texto atual. Chame após mudar Text.</summary>
    public void RefreshSize()
    {
        var size = PixelFont.Measure(Text ?? "", Scale);
        Width  = (int)size.X;
        Height = (int)size.Y;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible || string.IsNullOrEmpty(Text)) return;

        if (HasShadow)
            PixelFont.DrawWithShadow(spriteBatch, Core.Pixel, Text, Position, TextColor, Scale);
        else
            PixelFont.Draw(spriteBatch, Core.Pixel, Text, Position, TextColor, Scale);
    }
}
