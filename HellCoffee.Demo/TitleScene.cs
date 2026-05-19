using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HellCoffee.Engine;
using HellCoffee.Engine.Debug;
using HellCoffee.Engine.Graphics.Background;
using HellCoffee.Engine.Input;
using HellCoffee.Engine.Scene;
using HellCoffee.Engine.UI;

namespace HellCoffee.Demo;

/// <summary>
/// Tela de título com menu principal.
/// Demonstra: UIPanel, UIButton, UISlider, transição de cena com fade.
/// </summary>
public class TitleScene : Scene
{
    private SolidBackground _bg;
    private UIPanel _panel;
    private UIButton _playButton;
    private UIButton _quitButton;
    private UISlider _musicSlider;
    private UISlider _sfxSlider;
    private float _pulse;

    protected override void OnInitialize()
    {
        _bg = new SolidBackground(new Color(12, 8, 22));

        int cx = Core.VirtualWidth  / 2;
        int cy = Core.VirtualHeight / 2;

        const int panelW = 160;
        const int panelH = 96;
        const int bw     = 110;
        const int bh     = 13;
        int bx = cx - bw / 2;

        _panel = new UIPanel
        {
            Position        = new Vector2(cx - panelW / 2, 56),
            Width           = panelW,
            Height          = panelH,
            BackgroundColor = new Color(18, 12, 32, 225),
            BorderColor     = new Color(100, 75, 160),
        };

        _playButton = new UIButton("PLAY", new Vector2(bx, 67), bw, bh)
        {
            NormalColor = new Color(55, 35, 100),
            HoverColor  = new Color(85, 55, 150),
            BorderColor = new Color(130, 95, 210),
        };
        _playButton.OnClick += () => Core.Scenes.ChangeScene(new DemoScene());

        _quitButton = new UIButton("QUIT", new Vector2(bx, 84), bw, bh)
        {
            NormalColor = new Color(80, 28, 28),
            HoverColor  = new Color(120, 48, 48),
            BorderColor = new Color(180, 75, 75),
        };
        _quitButton.OnClick += () => Core.Instance.Exit();

        _musicSlider = new UISlider("MUSIC", new Vector2(bx, 103), bw, Core.Audio.MusicVolume);
        _musicSlider.OnValueChanged += v => Core.Audio.MusicVolume = v;

        _sfxSlider = new UISlider("SFX", new Vector2(bx, 125), bw, Core.Audio.SfxVolume);
        _sfxSlider.OnValueChanged += v => Core.Audio.SfxVolume = v;

        _panel.Add(_playButton);
        _panel.Add(_quitButton);
        _panel.Add(_musicSlider);
        _panel.Add(_sfxSlider);
    }

    public override void Update(GameTime gameTime)
    {
        _pulse += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Core.Input.Keyboard.JustPressed(Keys.Enter) ||
            Core.Input.Keyboard.JustPressed(Keys.Space))
        {
            Core.Scenes.ChangeScene(new DemoScene());
            return;
        }

        var mousePos = Core.ScreenToVirtual(Core.Input.Mouse.Position.ToVector2());
        bool clicked = Core.Input.Mouse.JustPressed(MouseButton.Left);
        _panel.Update(gameTime, mousePos, clicked);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _bg.Draw(spriteBatch, null);

        // título pulsante
        float t = (MathF.Sin(_pulse * 2f) + 1f) * 0.5f;
        var titleColor = Color.Lerp(new Color(190, 150, 255), new Color(255, 220, 255), t);

        const string title = "HELLCOFFEE";
        var titleSize = PixelFont.Measure(title, 2);
        var titlePos  = new Vector2((Core.VirtualWidth - titleSize.X) / 2f, 22f);
        PixelFont.DrawWithShadow(spriteBatch, Core.Pixel, title, titlePos, titleColor, 2);

        const string sub = "ENGINE DEMO";
        var subSize = PixelFont.Measure(sub, 1);
        PixelFont.Draw(spriteBatch, Core.Pixel, sub,
            new Vector2((Core.VirtualWidth - subSize.X) / 2f, titlePos.Y + titleSize.Y + 2),
            new Color(150, 120, 200));

        // hint piscante
        const string hint = "ENTER OR CLICK PLAY";
        float alpha  = (MathF.Sin(_pulse * 3.5f) + 1f) * 0.5f;
        var hintSize = PixelFont.Measure(hint, 1);
        PixelFont.Draw(spriteBatch, Core.Pixel, hint,
            new Vector2((Core.VirtualWidth - hintSize.X) / 2f, Core.VirtualHeight - 14f),
            Color.White * alpha);

        _panel.Draw(spriteBatch);

        spriteBatch.End();
    }
}
