using Microsoft.Xna.Framework;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Animation;

/// <summary>
/// Controla uma máquina de estados de animação.
/// Registra animações por nome e troca de estado com o mínimo de código.
/// </summary>
public class AnimationController
{
    private readonly Dictionary<string, Animation> _animations = new();
    private Animation _current;
    private int _frameIndex;
    private float _elapsed;

    public string CurrentName { get; private set; } = string.Empty;
    public TextureRegion CurrentRegion { get; private set; }
    public bool IsFinished { get; private set; }

    /// <summary>Evento chamado quando a animação troca de frame.</summary>
    public event Action<int> OnFrameChanged;
    /// <summary>Evento chamado quando uma animação não-loop termina.</summary>
    public event Action<string> OnAnimationFinished;

    public AnimationController Register(Animation animation)
    {
        _animations[animation.Name] = animation;
        return this;
    }

    public AnimationController Register(string name, IEnumerable<TextureRegion> frames,
        float frameDuration = 0.1f, bool isLooping = true)
    {
        return Register(new Animation(name, frames, frameDuration, isLooping));
    }

    /// <summary>
    /// Troca para a animação indicada. Se já estiver rodando, não reinicia.
    /// Use force=true para forçar o restart.
    /// </summary>
    public void Play(string name, bool force = false)
    {
        if (!_animations.TryGetValue(name, out var anim))
            throw new KeyNotFoundException($"Animation '{name}' not registered.");

        if (!force && CurrentName == name) return;

        _current = anim;
        CurrentName = name;
        _frameIndex = 0;
        _elapsed = 0f;
        IsFinished = false;
        CurrentRegion = _current.GetFrame(0).Region;
    }

    public void Update(GameTime gameTime)
    {
        if (_current == null || IsFinished) return;

        _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
        var frame = _current.GetFrame(_frameIndex);

        if (_elapsed >= frame.Duration)
        {
            _elapsed -= frame.Duration;
            int nextIndex = _frameIndex + 1;

            if (nextIndex >= _current.FrameCount)
            {
                if (_current.IsLooping)
                {
                    _frameIndex = 0;
                }
                else
                {
                    _frameIndex = _current.FrameCount - 1;
                    IsFinished = true;
                    OnAnimationFinished?.Invoke(CurrentName);
                }
            }
            else
            {
                _frameIndex = nextIndex;
            }

            CurrentRegion = _current.GetFrame(_frameIndex).Region;
            OnFrameChanged?.Invoke(_frameIndex);
        }
    }

    public bool HasAnimation(string name) => _animations.ContainsKey(name);
}
