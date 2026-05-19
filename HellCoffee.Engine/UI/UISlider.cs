using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Debug;
using HellCoffee.Engine.Input;

namespace HellCoffee.Engine.UI;

/// <summary>
/// Slider horizontal com label e valor numérico.
/// </summary>
public class UISlider : UIElement
{
    public string Label    { get; set; }
    public float MinValue  { get; set; } = 0f;
    public float MaxValue  { get; set; } = 1f;
    public float Value     { get; set; } = 0.5f;
    public int TextScale   { get; set; } = 1;
    public int TrackHeight { get; set; } = 4;
    public int HandleSize  { get; set; } = 8;

    public Color TrackColor  { get; set; } = new Color(30, 30, 50);
    public Color FillColor   { get; set; } = new Color(80, 80, 160);
    public Color HandleColor { get; set; } = Color.White;
    public Color LabelColor  { get; set; } = Color.White;

    public event Action<float>? OnValueChanged;

    private bool _dragging;

    private const int LabelGap = 2;

    public UISlider(string label, Vector2 position, int width, float value = 0.5f)
    {
        Label    = label;
        Position = position;
        Width    = width;
        Value    = value;
        Height   = PixelFont.GlyphH + LabelGap + HandleSize;
    }

    public override void Update(GameTime gameTime, Vector2 mousePos, bool mouseClicked)
    {
        if (!IsVisible || !IsEnabled) return;

        int trackY   = (int)Position.Y + PixelFont.GlyphH + LabelGap + (HandleSize - TrackHeight) / 2;
        float norm   = Math.Clamp((Value - MinValue) / (MaxValue - MinValue), 0f, 1f);
        int handleX  = (int)(Position.X + norm * Width);

        var trackRect  = new Rectangle((int)Position.X, trackY, Width, TrackHeight);
        var handleRect = new Rectangle(handleX - HandleSize / 2,
                                       (int)Position.Y + PixelFont.GlyphH + LabelGap,
                                       HandleSize, HandleSize);

        bool mouseDown = Core.Input.Mouse.IsDown(MouseButton.Left);

        if (mouseClicked &&
            (handleRect.Contains((int)mousePos.X, (int)mousePos.Y) ||
             trackRect.Contains((int)mousePos.X,  (int)mousePos.Y)))
        {
            _dragging = true;
        }

        if (!mouseDown)
            _dragging = false;

        if (_dragging)
        {
            float newVal = MinValue + Math.Clamp((mousePos.X - Position.X) / Width, 0f, 1f)
                                    * (MaxValue - MinValue);
            if (MathF.Abs(newVal - Value) > 0.001f)
            {
                Value = newVal;
                OnValueChanged?.Invoke(Value);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var pixel = Core.Pixel;

        // label + valor
        string display = $"{Label}: {Value:P0}".ToUpper();
        PixelFont.Draw(spriteBatch, pixel, display, Position, LabelColor, TextScale);

        int trackTop  = (int)Position.Y + PixelFont.GlyphH + LabelGap + (HandleSize - TrackHeight) / 2;
        float norm    = Math.Clamp((Value - MinValue) / (MaxValue - MinValue), 0f, 1f);
        int fillWidth = (int)(norm * Width);
        int handleX   = (int)(Position.X + norm * Width);

        spriteBatch.Draw(pixel, new Rectangle((int)Position.X, trackTop, Width, TrackHeight), TrackColor);
        if (fillWidth > 0)
            spriteBatch.Draw(pixel, new Rectangle((int)Position.X, trackTop, fillWidth, TrackHeight), FillColor);

        var handleRect = new Rectangle(handleX - HandleSize / 2,
                                       (int)Position.Y + PixelFont.GlyphH + LabelGap,
                                       HandleSize, HandleSize);
        spriteBatch.Draw(pixel, handleRect, HandleColor);
    }
}
