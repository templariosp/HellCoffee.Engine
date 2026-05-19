using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HellCoffee.Engine;
using HellCoffee.Engine.Debug;
using HellCoffee.Engine.Input;
using HellCoffee.Engine.Scene;
using HellCoffee.Engine.UI;

namespace HellCoffee.Demo;

/// <summary>
/// Overlay de pausa sobreposto à DemoScene.
/// ESC ou RESUME fecham o overlay sem descartar a cena ativa.
/// </summary>
public class PauseOverlay : Scene
{
    private UIPanel _panel;
    private UIButton _resumeButton;
    private UIButton _titleButton;
    private UIButton _quitButton;

    protected override void OnInitialize()
    {
        int cx = Core.VirtualWidth  / 2;
        int cy = Core.VirtualHeight / 2;

        const int panelW = 120;
        const int panelH = 72;
        const int bw     = 90;
        const int bh     = 13;
        int bx = cx - bw / 2;

        _panel = new UIPanel
        {
            Position        = new Vector2(cx - panelW / 2, cy - panelH / 2),
            Width           = panelW,
            Height          = panelH,
            BackgroundColor = new Color(10, 7, 20, 235),
            BorderColor     = new Color(100, 75, 160),
        };

        _resumeButton = new UIButton("RESUME", new Vector2(bx, cy - 22), bw, bh);
        _resumeButton.OnClick += () => Core.Scenes.PopOverlay();

        _titleButton = new UIButton("TITLE", new Vector2(bx, cy - 5), bw, bh)
        {
            NormalColor = new Color(35, 60, 35),
            HoverColor  = new Color(55, 90, 55),
            BorderColor = new Color(75, 135, 75),
        };
        _titleButton.OnClick += () =>
        {
            Core.Scenes.PopOverlay();
            Core.Scenes.ChangeScene(new TitleScene());
        };

        _quitButton = new UIButton("QUIT", new Vector2(bx, cy + 12), bw, bh)
        {
            NormalColor = new Color(80, 28, 28),
            HoverColor  = new Color(120, 48, 48),
            BorderColor = new Color(180, 75, 75),
        };
        _quitButton.OnClick += () => Core.Instance.Exit();

        _panel.Add(_resumeButton);
        _panel.Add(_titleButton);
        _panel.Add(_quitButton);
    }

    public override void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.JustPressed(Keys.Escape))
        {
            Core.Scenes.PopOverlay();
            return;
        }

        var mousePos = Core.ScreenToVirtual(Core.Input.Mouse.Position.ToVector2());
        bool clicked = Core.Input.Mouse.JustPressed(MouseButton.Left);
        _panel.Update(gameTime, mousePos, clicked);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // escurece o fundo sem apagá-lo
        spriteBatch.Draw(Core.Pixel,
            new Rectangle(0, 0, Core.VirtualWidth, Core.VirtualHeight),
            Color.Black * 0.55f);

        // título
        const string title = "PAUSED";
        var size    = PixelFont.Measure(title, 2);
        var titlePos = new Vector2((Core.VirtualWidth - size.X) / 2f,
                                   Core.VirtualHeight / 2f - 46f);
        PixelFont.DrawWithShadow(spriteBatch, Core.Pixel, title, titlePos,
            new Color(200, 175, 255), 2);

        _panel.Draw(spriteBatch);

        spriteBatch.End();
    }
}
