using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Input;
using HellCoffee.Engine.Audio;
using HellCoffee.Engine.Scene;
using HellCoffee.Engine.Debug;

namespace HellCoffee.Engine;

public abstract class Core : Game
{
    public static Core Instance { get; private set; }

    public static GraphicsDevice GD => Instance.GraphicsDevice;
    public static SpriteBatch SB { get; private set; }
    public static InputManager Input { get; private set; }
    public static AudioManager Audio { get; private set; }
    public static SceneManager Scenes { get; private set; }
    public static DebugOverlay Debug { get; private set; }

    public static int VirtualWidth { get; private set; }
    public static int VirtualHeight { get; private set; }

    protected GraphicsDeviceManager Graphics;
    private RenderTarget2D _renderTarget;
    private Rectangle _renderDestination;

    protected Core(int virtualWidth, int virtualHeight, int windowWidth = 0, int windowHeight = 0)
    {
        Instance = this;
        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;

        Graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth  = windowWidth  > 0 ? windowWidth  : virtualWidth  * 3,
            PreferredBackBufferHeight = windowHeight > 0 ? windowHeight : virtualHeight * 3,
            IsFullScreen = false
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        SB     = new SpriteBatch(GraphicsDevice);
        Input  = new InputManager();
        Audio  = new AudioManager();
        Scenes = new SceneManager();
        Debug  = new DebugOverlay();

        _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);
        UpdateRenderDestination();

        Window.ClientSizeChanged += (_, _) => UpdateRenderDestination();

        base.Initialize();
        OnInitialize();
    }

    protected override void LoadContent()
    {
        Scenes.LoadFadeTexture(GraphicsDevice);
        Debug.LoadContent(Content);
        OnLoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update(gameTime);
        Audio.Update(gameTime);
        Debug.Update(gameTime);
        Scenes.Update(gameTime);
        OnUpdate(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        Scenes.Draw(gameTime, SB);
        OnDraw(gameTime, SB);

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        SB.Begin(samplerState: SamplerState.PointClamp);
        SB.Draw(_renderTarget, _renderDestination, Color.White);
        SB.End();

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        Scenes.Dispose();
        _renderTarget?.Dispose();
        base.UnloadContent();
    }

    private void UpdateRenderDestination()
    {
        var screenWidth  = GraphicsDevice.Viewport.Width;
        var screenHeight = GraphicsDevice.Viewport.Height;
        float scale = Math.Min((float)screenWidth / VirtualWidth, (float)screenHeight / VirtualHeight);

        int destWidth  = (int)(VirtualWidth  * scale);
        int destHeight = (int)(VirtualHeight * scale);

        _renderDestination = new Rectangle(
            (screenWidth  - destWidth)  / 2,
            (screenHeight - destHeight) / 2,
            destWidth, destHeight);
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnLoadContent() { }
    protected virtual void OnUpdate(GameTime gameTime) { }
    protected virtual void OnDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
}
