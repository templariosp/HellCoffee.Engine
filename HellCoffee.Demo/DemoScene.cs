using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HellCoffee.Engine;
using HellCoffee.Engine.Graphics.Camera;
using HellCoffee.Engine.Graphics.Background;
using HellCoffee.Engine.Graphics.Sprites;
using HellCoffee.Engine.Collision.Shapes;
using HellCoffee.Engine.Scene;
using System;
using System.Linq;

namespace HellCoffee.Demo;

/// <summary>
/// Controles:
///   A / D / Setas  — mover
///   W / Seta cima / Space  — pular
///   F1-F4          — debug overlays
///   Escape         — sair
/// </summary>
public class DemoScene : Scene
{
    // câmera e backgrounds
    private Camera2D _camera;
    private ParallaxBackground _bgFar;
    private ParallaxBackground _bgNear;
    private SolidBackground _bgSky;

    // personagem
    private Vector2 _playerPos;
    private Vector2 _velocity;
    private bool _isGrounded;
    private bool _facingLeft;
    private AnimatedSprite _playerSprite;

    private const float Speed      = 80f;
    private const float Gravity    = 420f;
    private const float JumpForce  = -200f;
    private const float MaxFall    = 350f;
    private const int   GroundY    = 140;
    private const int   PlayerW    = 16;
    private const int   PlayerH    = 16;

    // chão
    private Sprite _ground;
    private RectShape _playerBounds;
    private RectShape _groundBounds;

    protected override void OnInitialize()
    {
        _playerPos  = new Vector2(Core.VirtualWidth / 2f - PlayerW / 2f, GroundY - PlayerH);
        _velocity   = Vector2.Zero;
        _isGrounded = true;

        // câmera
        _camera = new Camera2D();
        _camera.LookAt(Core.VirtualWidth / 2f, Core.VirtualHeight / 2f);

        // backgrounds
        _bgSky = new SolidBackground(new Color(20, 15, 35));

        var farTex  = MakeStripedH(320, 60, new Color(40, 30, 60), new Color(50, 38, 72), 10);
        _bgFar = new ParallaxBackground(new TextureRegion(farTex), parallaxFactorX: 0.2f)
            { RepeatX = true };

        var nearTex = MakeStripedV(48, 80, new Color(55, 40, 30), new Color(65, 50, 38), 6);
        _bgNear = new ParallaxBackground(new TextureRegion(nearTex), parallaxFactorX: 0.6f)
            { RepeatX = true };

        // chão
        var groundTex = MakeCheckerboard(Core.VirtualWidth, 40, new Color(80, 60, 40), new Color(70, 52, 34), 8);
        _ground       = new Sprite(new TextureRegion(groundTex));
        _groundBounds = new RectShape(0, GroundY, Core.VirtualWidth, 40);

        // spritesheet: 5 frames (0-3 corrida, 4 pulo)
        var sheet     = BuildPlayerSheet();
        var idleFrames = sheet.GetFrameRange(0, 1);
        var runFrames  = sheet.GetFrameRange(0, 4);
        var jumpFrames = sheet.GetFrameRange(4, 1);

        _playerSprite = new AnimatedSprite();
        _playerSprite.Controller.Register("idle", idleFrames, frameDuration: 0.4f);
        _playerSprite.Controller.Register("run",  runFrames,  frameDuration: 0.1f);
        _playerSprite.Controller.Register("jump", jumpFrames, frameDuration: 0.2f, isLooping: false);
        _playerSprite.Play("idle");

        _playerBounds = new RectShape(_playerPos.X, _playerPos.Y, PlayerW, PlayerH);
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var   kb = Core.Input.Keyboard;

        if (kb.IsDown(Keys.Escape))
            Core.Instance.Exit();

        // --- Movimento horizontal ---
        float dx = 0;
        if (kb.IsDown(Keys.A) || kb.IsDown(Keys.Left))  { dx = -1; _facingLeft = true;  }
        if (kb.IsDown(Keys.D) || kb.IsDown(Keys.Right)) { dx =  1; _facingLeft = false; }

        _playerPos.X += dx * Speed * dt;
        _playerPos.X  = Math.Clamp(_playerPos.X, 0, Core.VirtualWidth - PlayerW);

        // --- Pulo ---
        bool jumpPressed = kb.JustPressed(Keys.W)
                        || kb.JustPressed(Keys.Up)
                        || kb.JustPressed(Keys.Space);

        if (jumpPressed && _isGrounded)
        {
            _velocity.Y = JumpForce;
            _isGrounded = false;
        }

        // --- Gravidade ---
        _velocity.Y  = Math.Min(_velocity.Y + Gravity * dt, MaxFall);
        _playerPos.Y += _velocity.Y * dt;

        // --- Colisão com o chão ---
        bool wasInAir = !_isGrounded;
        if (_playerPos.Y + PlayerH >= GroundY)
        {
            _playerPos.Y = GroundY - PlayerH;
            bool justLanded = wasInAir && _velocity.Y > 0;
            _velocity.Y = 0;
            _isGrounded = true;

            if (justLanded)
                _camera.Shake.Trigger(intensity: 3f, duration: 0.2f);
        }

        // --- Animação ---
        string anim;
        if (!_isGrounded)                anim = "jump";
        else if (dx != 0)                anim = "run";
        else                             anim = "idle";

        _playerSprite.Play(anim);
        _playerSprite.FlipHorizontal(_facingLeft);

        // --- Bounds de colisão ---
        _playerBounds.Position = _playerPos;

        // --- Câmera segue X e Y do player ---
        float camTargetY = Core.VirtualHeight / 2f + (_playerPos.Y - (GroundY - PlayerH)) * 0.3f;
        _camera.Follow(
            new Vector2(_playerPos.X + PlayerW / 2f, camTargetY),
            smoothing: 0.04f);
        _camera.Update(dt);

        // --- Animação ---
        _playerSprite.Update(gameTime);

        // --- Debug info ---
        Core.Debug.ClearInfoLines();
        Core.Debug.AddInfoLine($"vel Y:{_velocity.Y:F0} {(_isGrounded ? "GROUND" : "AIR")}");
        Core.Debug.AddInfoLine("AD=mov  W/SPACE=jump");
        Core.Debug.AddInfoLine("F1=col F2=fps F3=grid");
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // backgrounds (sem câmera)
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _bgSky.Draw(spriteBatch, _camera);
        _bgFar.Draw(spriteBatch, _camera);
        _bgNear.Draw(spriteBatch, _camera);
        spriteBatch.End();

        // mundo (com câmera)
        spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                          transformMatrix: _camera.GetViewMatrix());

        _ground.Draw(spriteBatch, new Vector2(0, GroundY));
        _playerSprite.Draw(spriteBatch, _playerPos);

        Core.Debug.DrawCollisionRect(spriteBatch, _playerBounds.BoundingBox, Color.Lime);
        Core.Debug.DrawCollisionRect(spriteBatch, _groundBounds.BoundingBox, Color.Orange);

        spriteBatch.End();

        // HUD
        Core.Debug.Draw(spriteBatch);
    }

    // -------------------------------------------------------
    // Geradores de textura procedural
    // -------------------------------------------------------

    private static Texture2D MakeSolid(int w, int h, Color color)
    {
        var tex = new Texture2D(Core.GD, w, h);
        tex.SetData(Enumerable.Repeat(color, w * h).ToArray());
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

    private static Texture2D MakeCheckerboard(int w, int h, Color a, Color b, int size)
    {
        var data = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                data[y * w + x] = ((x / size) + (y / size)) % 2 == 0 ? a : b;
        var tex = new Texture2D(Core.GD, w, h);
        tex.SetData(data);
        return tex;
    }

    /// <summary>
    /// Spritesheet 80x16: 5 frames de 16x16.
    /// Frames 0-3: corrida (pernas alternadas).
    /// Frame 4: pulo (pernas recolhidas, braços abertos).
    /// </summary>
    private static SpriteSheet BuildPlayerSheet()
    {
        const int fw = 16, fh = 16, frames = 5;
        var data = new Color[fw * frames * fh];

        var body = new Color(220, 80,  80);
        var leg  = new Color(80,  60, 140);
        var skin = new Color(255, 200, 150);
        var eye  = Color.White;

        int[] legOffsetL = { 0,  2,  0, -2,  0 };
        int[] legOffsetR = { 0, -2,  0,  2,  0 };
        // frame 4: pernas recolhidas para cima
        int[] legRowOffset = { 0, 0, 0, 0, -3 };

        for (int f = 0; f < frames; f++)
        {
            int ox = f * fw;

            void Set(int x, int y, Color c)
            {
                if (x < 0 || x >= fw || y < 0 || y >= fh) return;
                data[y * (fw * frames) + ox + x] = c;
            }

            // cabeça
            for (int y = 0; y < 4; y++)
                for (int x = 6; x < 10; x++)
                    Set(x, y, skin);
            Set(7, 1, eye);
            Set(8, 1, eye);

            // corpo
            for (int y = 4; y < 9; y++)
                for (int x = 5; x < 11; x++)
                    Set(x, y, body);

            // braços abertos no frame de pulo
            if (f == 4)
            {
                Set(3, 5, skin); Set(4, 5, skin);   // braço esq
                Set(11, 5, skin); Set(12, 5, skin); // braço dir
            }

            // perna esquerda
            int lx = 5 + legOffsetL[f];
            int ly = 9 + legRowOffset[f];
            for (int y = 0; y < 5; y++)
            {
                Set(lx,     ly + y, leg);
                Set(lx + 1, ly + y, leg);
            }

            // perna direita
            int rx = 9 + legOffsetR[f];
            int ry = 9 + legRowOffset[f];
            for (int y = 0; y < 5; y++)
            {
                Set(rx,     ry + y, leg);
                Set(rx + 1, ry + y, leg);
            }
        }

        var tex = new Texture2D(Core.GD, fw * frames, fh);
        tex.SetData(data);
        return SpriteSheet.FromGrid(tex, fw, fh, columns: frames, rows: 1);
    }
}
