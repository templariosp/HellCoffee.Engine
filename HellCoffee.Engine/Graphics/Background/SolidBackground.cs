using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine;
using HellCoffee.Engine.Graphics.Camera;

namespace HellCoffee.Engine.Graphics.Background;

public class SolidBackground : BackgroundLayer
{
    private static Texture2D _pixel;

    public Color Color { get; set; }

    public SolidBackground(Color color)
    {
        Color = color;
    }

    public override void Update(GameTime gameTime, Camera2D camera) { }

    public override void Draw(SpriteBatch spriteBatch, Camera2D camera)
    {
        if (!IsVisible) return;
        _pixel ??= CreatePixel();
        spriteBatch.Draw(_pixel,
            new Rectangle(0, 0, Core.VirtualWidth, Core.VirtualHeight),
            Color);
    }

    private static Texture2D CreatePixel()
    {
        var tex = new Texture2D(Core.GD, 1, 1);
        tex.SetData(new[] { Color.White });
        return tex;
    }
}
