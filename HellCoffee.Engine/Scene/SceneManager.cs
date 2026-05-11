using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine;

namespace HellCoffee.Engine.Scene;

public class SceneManager : IDisposable
{
    private Scene _active;
    private Scene _next;
    private bool _fadingOut;
    private bool _fadingIn;
    private float _fadeAlpha;
    private const float FadeSpeed = 2.5f;

    // 1x1 pixel branco para o fade
    private Texture2D _fadeTexture;

    internal void LoadFadeTexture(GraphicsDevice gd)
    {
        _fadeTexture = new Texture2D(gd, 1, 1);
        _fadeTexture.SetData(new[] { Color.Black });
    }

    public void ChangeScene(Scene next)
    {
        _next = next;
        _fadingOut = true;
        _fadingIn = false;
        _fadeAlpha = 0f;
    }

    /// <summary>Troca imediatamente sem fade.</summary>
    public void ChangeSceneImmediate(Scene next, ContentManager content)
    {
        _active?.Dispose();
        _active = next;
        _active.Initialize(content);
        _next = null;
        _fadingOut = false;
        _fadingIn = false;
        _fadeAlpha = 0f;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_fadingOut)
        {
            _fadeAlpha += FadeSpeed * dt;
            if (_fadeAlpha >= 1f)
            {
                _fadeAlpha = 1f;
                _fadingOut = false;
                TransitionNow();
                _fadingIn = true;
            }
        }
        else if (_fadingIn)
        {
            _fadeAlpha -= FadeSpeed * dt;
            if (_fadeAlpha <= 0f)
            {
                _fadeAlpha = 0f;
                _fadingIn = false;
            }
        }

        _active?.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _active?.Draw(gameTime, spriteBatch);

        if (_fadeAlpha > 0f && _fadeTexture != null)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_fadeTexture,
                new Rectangle(0, 0, Core.VirtualWidth, Core.VirtualHeight),
                Color.Black * _fadeAlpha);
            spriteBatch.End();
        }
    }

    private void TransitionNow()
    {
        _active?.Dispose();
        _active = _next;
        _next = null;
        _active?.Initialize(Core.Instance.Content);
    }

    public void Dispose()
    {
        _active?.Dispose();
        _fadeTexture?.Dispose();
    }
}
