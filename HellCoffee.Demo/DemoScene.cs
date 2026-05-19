using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HellCoffee.Engine;
using HellCoffee.Engine.Collision;
using HellCoffee.Engine.Debug;
using HellCoffee.Engine.Graphics.Background;
using HellCoffee.Engine.Graphics.Camera;
using HellCoffee.Engine.Graphics.Particles;
using HellCoffee.Engine.Graphics.Sprites;
using HellCoffee.Engine.Graphics.Tilemap;
using HellCoffee.Engine.Input;
using HellCoffee.Engine.Scene;
using HellCoffee.Engine.UI;
using GM = HellCoffee.Engine.GameManager.GameManager;

namespace HellCoffee.Demo;

/// <summary>
/// Demonstração principal da engine.
/// Controles:
///   A/D ou Setas  — mover
///   W/Seta cima/Space — pular
///   ESC           — pausa (UIPanel overlay)
///   S             — salvar posição
///   L             — carregar posição
///   F1-F4         — toggles de debug
/// </summary>
public class DemoScene : Scene
{
    // --- Constantes ---
    private const float Speed     = 80f;
    private const float Gravity   = 420f;
    private const float JumpForce = -200f;
    private const float MaxFall   = 350f;
    private const int   PlayerW   = 16;
    private const int   PlayerH   = 16;
    private const int   TileSize  = 8;
    private const int   MapCols   = 80;
    private const int   MapRows   = 24;

    // --- Câmera & Backgrounds ---
    private Camera2D _camera;
    private SolidBackground _bgSky;
    private ParallaxBackground _bgFar;
    private ParallaxBackground _bgNear;

    // --- Jogador ---
    private Vector2 _playerPos;
    private Vector2 _velocity;
    private bool _isGrounded;
    private bool _facingLeft;
    private AnimatedSprite _playerSprite;

    // --- Tilemap ---
    private TilemapLayer _tileLayer;

    // --- Partículas ---
    private ParticleEmitter _dustEmitter;

    // --- HUD ---
    private UILabel _scoreLabel;
    private UILabel _coordsLabel;
    private UILabel _notifLabel;
    private float _notifTimer;

    protected override void OnInitialize()
    {
        _playerPos  = new Vector2(44f, 120f);
        _velocity   = Vector2.Zero;
        _isGrounded = false;

        // Câmera
        _camera = new Camera2D();
        _camera.SetWorldBounds(MapCols * TileSize, MapRows * TileSize);
        _camera.LookAt(_playerPos);

        // Backgrounds
        _bgSky  = new SolidBackground(new Color(20, 15, 35));
        var farTex  = MakeStripedH(320, 60, new Color(40, 30, 60), new Color(50, 38, 72), 10);
        _bgFar  = new ParallaxBackground(new TextureRegion(farTex),  parallaxFactorX: 0.2f) { RepeatX = true };
        var nearTex = MakeStripedV(48, 80, new Color(55, 40, 30), new Color(65, 50, 38), 6);
        _bgNear = new ParallaxBackground(new TextureRegion(nearTex), parallaxFactorX: 0.6f) { RepeatX = true };

        // Tilemap
        var tileTex = BuildTilesetTexture();
        var tileset = new Tileset(tileTex, TileSize, TileSize);
        _tileLayer  = new TilemapLayer("world", tileset, MapCols, MapRows);
        FillMap(_tileLayer);

        // Sprite do jogador
        var sheet = BuildPlayerSheet();
        _playerSprite = new AnimatedSprite();
        _playerSprite.Controller
            .Register("idle", sheet.GetFrameRange(0, 1), frameDuration: 0.4f)
            .Register("run",  sheet.GetFrameRange(0, 4), frameDuration: 0.1f)
            .Register("jump", sheet.GetFrameRange(4, 1), frameDuration: 0.2f, isLooping: false);
        _playerSprite.Play("idle");

        // Partículas de poeira ao pousar
        _dustEmitter = new ParticleEmitter(new ParticleEmitterConfig
        {
            MaxParticles = 30,
            EmissionRate = 0f,
            MinLifetime  = 0.25f,
            MaxLifetime  = 0.55f,
            MinVelocity  = new Vector2(-28f, -38f),
            MaxVelocity  = new Vector2( 28f,  -5f),
            Gravity      = new Vector2(  0f,  90f),
            SpawnRadius  = 5f,
            StartColor   = new Color(200, 175, 130),
            EndColor     = Color.Transparent,
            StartScale   = 2.5f,
            EndScale     = 0f,
            IsLooping    = false,
        });

        // HUD
        _scoreLabel  = new UILabel("SCORE: 0", new Vector2(2, 2),  new Color(255, 220, 80));
        _coordsLabel = new UILabel("X:0 Y:0",  new Vector2(2, 12), new Color(160, 160, 180));
        _notifLabel  = new UILabel("",         new Vector2(2, 22), new Color(100, 230, 110)) { IsVisible = false };
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var kb   = Core.Input.Keyboard;

        // Pausa
        if (kb.JustPressed(Keys.Escape))
        {
            Core.Scenes.PushOverlay(new PauseOverlay());
            return;
        }

        // Save / Load
        if (kb.JustPressed(Keys.S)) SavePlayerPos();
        if (kb.JustPressed(Keys.L)) LoadPlayerPos();

        // Timer da notificação HUD
        if (_notifTimer > 0f)
        {
            _notifTimer -= dt;
            if (_notifTimer <= 0f) _notifLabel.IsVisible = false;
        }

        // Movimento horizontal
        float dx = 0f;
        if (kb.IsDown(Keys.A) || kb.IsDown(Keys.Left))  { dx = -1f; _facingLeft = true;  }
        if (kb.IsDown(Keys.D) || kb.IsDown(Keys.Right)) { dx =  1f; _facingLeft = false; }
        _playerPos.X += dx * Speed * dt;

        // Pulo
        bool jumpPressed = kb.JustPressed(Keys.W)
                        || kb.JustPressed(Keys.Up)
                        || kb.JustPressed(Keys.Space);
        if (jumpPressed && _isGrounded)
        {
            _velocity.Y = JumpForce;
            _isGrounded = false;
        }

        // Gravidade
        _velocity.Y  = Math.Min(_velocity.Y + Gravity * dt, MaxFall);
        _playerPos.Y += _velocity.Y * dt;

        // Colisão com tilemap
        bool wasGrounded = _isGrounded;
        var sep          = TileCollision.Resolve(PlayerRect(), _tileLayer);
        _playerPos      += sep;

        if (sep.Y < 0f) // empurrado para cima → aterrisou no chão
        {
            bool justLanded = !wasGrounded && _velocity.Y > 0f;
            _velocity.Y = 0f;
            _isGrounded = true;

            if (justLanded)
            {
                _camera.Shake.Trigger(intensity: 3f, duration: 0.2f);
                _dustEmitter.Position = new Vector2(_playerPos.X + PlayerW / 2f, _playerPos.Y + PlayerH);
                _dustEmitter.Burst(8);
            }
        }
        else
        {
            _isGrounded = TileCollision.IsGrounded(PlayerRect(), _tileLayer);
            if (_isGrounded && _velocity.Y > 0f) _velocity.Y = 0f;
        }

        if (sep.X != 0f) _velocity.X = 0f;
        if (sep.Y > 0f)  _velocity.Y = 0f; // teto

        // Limitar ao mundo
        _playerPos.X = Math.Clamp(_playerPos.X, 0f, MapCols * TileSize - PlayerW);
        if (_playerPos.Y > MapRows * TileSize) { _playerPos.Y = 0f; _velocity.Y = 0f; }

        // Animação
        string anim = !_isGrounded ? "jump" : dx != 0f ? "run" : "idle";
        _playerSprite.Play(anim);
        _playerSprite.FlipHorizontal(_facingLeft);
        _playerSprite.Update(gameTime);

        // Partículas
        _dustEmitter.Update(gameTime);

        // Câmera
        _camera.Follow(new Vector2(_playerPos.X + PlayerW / 2f, _playerPos.Y + PlayerH / 2f),
                       smoothing: 0.04f);
        _camera.Update(dt);

        // HUD
        _scoreLabel.Text  = $"SCORE: {GM.Instance.Score}";
        _coordsLabel.Text = $"X:{(int)_playerPos.X} Y:{(int)_playerPos.Y}";

        // Debug overlay
        Core.Debug.ClearInfoLines();
        Core.Debug.AddInfoLine($"VEL Y:{_velocity.Y:F0}  {(_isGrounded ? "GRND" : "AIR")}");
        Core.Debug.AddInfoLine("WASD/ARROWS=MOVE  SPACE=JUMP");
        Core.Debug.AddInfoLine("ESC=PAUSE  S=SAVE  L=LOAD");
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Backgrounds em screen space (sem câmera)
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _bgSky.Draw(spriteBatch, _camera);
        _bgFar.Draw(spriteBatch, _camera);
        _bgNear.Draw(spriteBatch, _camera);
        spriteBatch.End();

        // Mundo em world space (com câmera)
        spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                          transformMatrix: _camera.GetViewMatrix());

        _tileLayer.Draw(spriteBatch, _camera);
        _dustEmitter.Draw(spriteBatch);
        _playerSprite.Draw(spriteBatch, _playerPos);

        Core.Debug.DrawCollisionRect(spriteBatch, PlayerRect(), Color.Lime);
        Core.Debug.DrawTileGrid(spriteBatch, _tileLayer);

        spriteBatch.End();

        // HUD em screen space (em cima de tudo)
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _scoreLabel.Draw(spriteBatch);
        _coordsLabel.Draw(spriteBatch);
        if (_notifLabel.IsVisible) _notifLabel.Draw(spriteBatch);
        spriteBatch.End();

        Core.Debug.Draw(spriteBatch);
    }

    // -------------------------------------------------------
    // Save / Load
    // -------------------------------------------------------

    private void SavePlayerPos()
    {
        HellCoffee.Engine.GameManager.SaveSystem.Save("demo_pos",
            new PlayerSaveData { X = _playerPos.X, Y = _playerPos.Y });
        ShowNotif("POSITION SAVED");
    }

    private void LoadPlayerPos()
    {
        var data = HellCoffee.Engine.GameManager.SaveSystem.Load<PlayerSaveData>("demo_pos");
        if (data.X != 0f || data.Y != 0f)
        {
            _playerPos = new Vector2(data.X, data.Y);
            _velocity  = Vector2.Zero;
            ShowNotif("POSITION LOADED");
        }
    }

    private void ShowNotif(string msg)
    {
        _notifLabel.Text      = msg;
        _notifLabel.IsVisible = true;
        _notifTimer           = 2f;
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private Rectangle PlayerRect()
        => new Rectangle((int)_playerPos.X, (int)_playerPos.Y, PlayerW, PlayerH);

    // -------------------------------------------------------
    // Construção do mapa
    // -------------------------------------------------------

    private static void FillMap(TilemapLayer layer)
    {
        // Tile 0 = chão sólido  |  Tile 1 = plataforma sólida

        // Chão principal (linhas 20-23)
        for (int row = 20; row < MapRows; row++)
            for (int col = 0; col < MapCols; col++)
                layer.SetTile(col, row, new Tile(0, TileFlags.Solid));

        // Paredes laterais
        for (int row = 18; row < 20; row++)
        {
            layer.SetTile(0,           row, new Tile(0, TileFlags.Solid));
            layer.SetTile(MapCols - 1, row, new Tile(0, TileFlags.Solid));
        }

        // Plataformas flutuantes (col_início, linha, largura)
        SetPlatform(layer,  5, 17,  8);
        SetPlatform(layer, 18, 15,  6);
        SetPlatform(layer, 28, 13,  8);
        SetPlatform(layer, 38, 16,  5);
        SetPlatform(layer, 48, 14,  7);
        SetPlatform(layer, 55, 18,  6);
        SetPlatform(layer, 62, 12,  8);
        SetPlatform(layer, 70, 17,  8);
    }

    private static void SetPlatform(TilemapLayer layer, int startCol, int row, int width)
    {
        for (int c = startCol; c < startCol + width && c < MapCols; c++)
            layer.SetTile(c, row, new Tile(1, TileFlags.Solid));
    }

    // -------------------------------------------------------
    // Texturas procedurais
    // -------------------------------------------------------

    private static Texture2D BuildTilesetTexture()
    {
        // 2 tiles de 8×8 lado a lado
        const int tw = 8, th = 8;
        var data = new Color[tw * 2 * th];

        // Tile 0 — chão escuro (checkerboard)
        for (int y = 0; y < th; y++)
            for (int x = 0; x < tw; x++)
                data[y * (tw * 2) + x] = ((x / 4) + (y / 4)) % 2 == 0
                    ? new Color(80, 60, 40)
                    : new Color(68, 50, 32);

        // Tile 1 — plataforma clara com borda superior destacada
        for (int y = 0; y < th; y++)
            for (int x = 0; x < tw; x++)
            {
                int px = tw + x;
                data[y * (tw * 2) + px] = y == 0
                    ? new Color(160, 130, 80)
                    : ((x / 4) + (y / 4)) % 2 == 0
                        ? new Color(110, 90, 55)
                        : new Color(98, 80, 46);
            }

        var tex = new Texture2D(Core.GD, tw * 2, th);
        tex.SetData(data);
        return tex;
    }

    private static Texture2D MakeStripedH(int w, int h, Color a, Color b, int stripeH)
    {
        var data = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                data[y * w + x] = (y / stripeH) % 2 == 0 ? a : b;
        var tex = new Texture2D(Core.GD, w, h);
        tex.SetData(data);
        return tex;
    }

    private static Texture2D MakeStripedV(int w, int h, Color a, Color b, int stripeW)
    {
        var data = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                data[y * w + x] = (x / stripeW) % 2 == 0 ? a : b;
        var tex = new Texture2D(Core.GD, w, h);
        tex.SetData(data);
        return tex;
    }

    private static SpriteSheet BuildPlayerSheet()
    {
        const int fw = 16, fh = 16, frames = 5;
        var data = new Color[fw * frames * fh];

        var body = new Color(220, 80,  80);
        var leg  = new Color(80,  60, 140);
        var skin = new Color(255, 200, 150);
        var eye  = Color.White;

        int[] legOffsetL  = {  0,  2,  0, -2,  0 };
        int[] legOffsetR  = {  0, -2,  0,  2,  0 };
        int[] legRowShift = {  0,  0,  0,  0, -3 };

        for (int f = 0; f < frames; f++)
        {
            int ox = f * fw;

            void Set(int x, int y, Color c)
            {
                if (x < 0 || x >= fw || y < 0 || y >= fh) return;
                data[y * (fw * frames) + ox + x] = c;
            }

            // Cabeça
            for (int y = 0; y < 4; y++)
                for (int x = 6; x < 10; x++)
                    Set(x, y, skin);
            Set(7, 1, eye); Set(8, 1, eye);

            // Corpo
            for (int y = 4; y < 9; y++)
                for (int x = 5; x < 11; x++)
                    Set(x, y, body);

            // Braços abertos no frame de pulo
            if (f == 4)
            {
                Set(3, 5, skin); Set(4,  5, skin);
                Set(11, 5, skin); Set(12, 5, skin);
            }

            // Perna esquerda
            int lx = 5 + legOffsetL[f], ly = 9 + legRowShift[f];
            for (int y = 0; y < 5; y++) { Set(lx, ly + y, leg); Set(lx + 1, ly + y, leg); }

            // Perna direita
            int rx = 9 + legOffsetR[f], ry = 9 + legRowShift[f];
            for (int y = 0; y < 5; y++) { Set(rx, ry + y, leg); Set(rx + 1, ry + y, leg); }
        }

        var tex = new Texture2D(Core.GD, fw * frames, fh);
        tex.SetData(data);
        return SpriteSheet.FromGrid(tex, fw, fh, columns: frames, rows: 1);
    }
}

internal class PlayerSaveData
{
    public float X { get; set; }
    public float Y { get; set; }
}
