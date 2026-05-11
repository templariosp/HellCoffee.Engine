using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Graphics.Animation;

namespace HellCoffee.Engine.Graphics.Sprites;

/// <summary>
/// Sprite que usa um AnimationController para controle de estado.
/// </summary>
public class AnimatedSprite : Sprite
{
    public AnimationController Controller { get; }

    public AnimatedSprite() : base(null)
    {
        Controller = new AnimationController();
    }

    public void Play(string animationName, bool force = false)
    {
        Controller.Play(animationName, force);
        Region = Controller.CurrentRegion;
    }

    public void Update(GameTime gameTime)
    {
        Controller.Update(gameTime);
        Region = Controller.CurrentRegion;
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        if (Region == null) return;
        base.Draw(spriteBatch, position);
    }
}
