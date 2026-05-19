using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.Graphics.Particles;

/// <summary>
/// Emissor de partículas com pool de tamanho fixo.
/// Uso:
///   var emitter = new ParticleEmitter(config);
///   emitter.Position = spawnPos;
///   emitter.Start();          // emissão contínua
///   emitter.Burst(10);        // explosão de N partículas
///   emitter.Update(gameTime);
///   emitter.Draw(spriteBatch);
/// </summary>
public class ParticleEmitter
{
    private readonly Particle[] _pool;
    private readonly ParticleEmitterConfig _config;
    private readonly Random _rng = new();
    private float _emitAccumulator;

    public Vector2 Position { get; set; }
    public bool IsEmitting { get; private set; }
    public int ActiveCount { get; private set; }

    public ParticleEmitter(ParticleEmitterConfig config)
    {
        _config = config;
        _pool = new Particle[config.MaxParticles];
    }

    public void Start() => IsEmitting = true;

    public void Stop(bool clearExisting = false)
    {
        IsEmitting = false;
        if (!clearExisting) return;
        for (int i = 0; i < _pool.Length; i++)
            _pool[i].LifetimeLeft = 0f;
        ActiveCount = 0;
    }

    /// <summary>Emite N partículas imediatamente, independente de IsEmitting.</summary>
    public void Burst(int count)
    {
        for (int i = 0; i < count; i++)
            Spawn();
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (IsEmitting && _config.EmissionRate > 0f)
        {
            _emitAccumulator += _config.EmissionRate * dt;
            while (_emitAccumulator >= 1f)
            {
                Spawn();
                _emitAccumulator -= 1f;
            }
        }

        ActiveCount = 0;
        for (int i = 0; i < _pool.Length; i++)
        {
            if (!_pool[i].IsAlive) continue;
            ActiveCount++;

            ref var p = ref _pool[i];
            p.LifetimeLeft -= dt;
            if (p.LifetimeLeft <= 0f) continue;

            p.Velocity += _config.Gravity * dt;
            p.Position += p.Velocity * dt;
            p.Rotation += p.RotationSpeed * dt;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (ActiveCount == 0) return;

        var texture = _config.Texture;

        for (int i = 0; i < _pool.Length; i++)
        {
            ref var p = ref _pool[i];
            if (!p.IsAlive) continue;

            float t = p.Progress;
            var color = Color.Lerp(p.StartColor, p.EndColor, t);
            float scale = MathHelper.Lerp(p.StartScale, p.EndScale, t);

            if (scale <= 0f || color.A == 0) continue;

            if (texture != null)
            {
                var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                spriteBatch.Draw(texture.Texture, p.Position, texture.SourceRectangle,
                    color, p.Rotation, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(Core.Pixel, p.Position, null,
                    color, p.Rotation, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
            }
        }
    }

    private void Spawn()
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            if (_pool[i].IsAlive) continue;

            float lt = MathHelper.Lerp(_config.MinLifetime, _config.MaxLifetime, (float)_rng.NextDouble());

            var spawnPos = Position + _config.SpawnOffset;
            if (_config.SpawnRadius > 0f)
            {
                float angle = (float)_rng.NextDouble() * MathF.PI * 2f;
                float r = (float)_rng.NextDouble() * _config.SpawnRadius;
                spawnPos += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * r;
            }

            _pool[i] = new Particle
            {
                Position = spawnPos,
                Velocity = new Vector2(
                    MathHelper.Lerp(_config.MinVelocity.X, _config.MaxVelocity.X, (float)_rng.NextDouble()),
                    MathHelper.Lerp(_config.MinVelocity.Y, _config.MaxVelocity.Y, (float)_rng.NextDouble())),
                LifetimeLeft  = lt,
                MaxLifetime   = lt,
                StartColor    = _config.StartColor,
                EndColor      = _config.EndColor,
                StartScale    = _config.StartScale,
                EndScale      = _config.EndScale,
                Rotation      = (float)_rng.NextDouble() * MathF.PI * 2f,
                RotationSpeed = MathHelper.Lerp(_config.MinRotationSpeed, _config.MaxRotationSpeed,
                                                (float)_rng.NextDouble()),
            };
            return;
        }
    }
}
