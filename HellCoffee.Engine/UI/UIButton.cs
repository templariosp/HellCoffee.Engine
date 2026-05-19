using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Debug;

namespace HellCoffee.Engine.UI;

/// <summary>
/// Botão clicável com estados Normal, Hover e Pressed.
/// </summary>
public class UIButton : UIElement
{
    public string Text { get; set; }
    public Color NormalColor { get; set; } = new Color(50, 50, 80);
    public Color HoverColor  { get; set; } = new Color(80, 80, 130);
    public Color PressColor  { get; set; } = new Color(30, 30, 50);
    public Color BorderColor { get; set; } = new Color(100, 100, 160);
    public Color TextColor   { get; set; } = Color.White;
    public int TextScale { get; set; } = 1;

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }

    public event Action? OnClick;

    public UIButton(string text, Vector2 position, int width, int height)
    {
        Text     = text;
        Position = position;
        Width    = width;
        Height   = height;
    }

    public override void Update(GameTime gameTime, Vector2 mousePos, bool mouseClicked)
    {
        if (!IsVisible || !IsEnabled) return;

        IsHovered = Bounds.Contains((int)mousePos.X, (int)mousePos.Y);
        IsPressed = false;

        if (IsHovered && mouseClicked)
        {
            IsPressed = true;
            OnClick?.Invoke();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var pixel  = Core.Pixel;
        var b      = Bounds;
        var bgColor = IsPressed ? PressColor : IsHovered ? HoverColor : NormalColor;

        spriteBatch.Draw(pixel, b, bgColor);

        // borda
        spriteBatch.Draw(pixel, new Rectangle(b.X,          b.Y,           b.Width, 1), BorderColor);
        spriteBatch.Draw(pixel, new Rectangle(b.X,          b.Bottom - 1,  b.Width, 1), BorderColor);
        spriteBatch.Draw(pixel, new Rectangle(b.X,          b.Y,           1, b.Height), BorderColor);
        spriteBatch.Draw(pixel, new Rectangle(b.Right - 1,  b.Y,           1, b.Height), BorderColor);

        if (!string.IsNullOrEmpty(Text))
        {
            var size    = PixelFont.Measure(Text, TextScale);
            var textPos = new Vector2(
                b.X + (b.Width  - size.X) / 2f,
                b.Y + (b.Height - size.Y) / 2f);
            PixelFont.DrawWithShadow(spriteBatch, pixel, Text, textPos, TextColor, TextScale);
        }
    }
}
