using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Animation;

public class Animation
{
    private readonly AnimationFrame[] _frames;

    public string Name { get; }
    public bool IsLooping { get; }
    public int FrameCount => _frames.Length;

    public Animation(string name, IEnumerable<AnimationFrame> frames, bool isLooping = true)
    {
        Name = name;
        _frames = frames.ToArray();
        IsLooping = isLooping;
        if (_frames.Length == 0)
            throw new ArgumentException("Animation must have at least one frame.", nameof(frames));
    }

    /// <summary>
    /// Cria animação com duração uniforme por frame.
    /// </summary>
    public Animation(string name, IEnumerable<TextureRegion> regions, float frameDuration = 0.1f, bool isLooping = true)
    {
        Name = name;
        IsLooping = isLooping;
        _frames = regions.Select(r => new AnimationFrame(r, frameDuration)).ToArray();
        if (_frames.Length == 0)
            throw new ArgumentException("Animation must have at least one frame.", nameof(regions));
    }

    public AnimationFrame GetFrame(int index) => _frames[index];
    public float GetTotalDuration() => _frames.Sum(f => f.Duration);
}
