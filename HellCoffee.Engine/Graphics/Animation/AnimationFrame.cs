using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Animation;

public class AnimationFrame
{
    public TextureRegion Region { get; }
    public float Duration { get; }

    public AnimationFrame(TextureRegion region, float duration = 0.1f)
    {
        Region = region;
        Duration = duration;
    }
}
