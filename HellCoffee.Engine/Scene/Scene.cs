using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.Scene;

public abstract class Scene : IDisposable
{
    protected ContentManager Content { get; private set; }
    public bool IsDisposed { get; private set; }

    internal void Initialize(ContentManager parentContent)
    {
        Content = new ContentManager(parentContent.ServiceProvider, parentContent.RootDirectory);
        OnInitialize();
        LoadContent();
    }

    protected virtual void OnInitialize() { }

    protected virtual void LoadContent() { }

    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }

    protected virtual void UnloadContent()
    {
        Content?.Unload();
        Content?.Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        UnloadContent();
        GC.Collect();
    }
}
