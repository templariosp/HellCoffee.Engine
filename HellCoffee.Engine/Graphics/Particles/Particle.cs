using Microsoft.Xna.Framework;

namespace HellCoffee.Engine.Graphics.Particles;

public struct Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float LifetimeLeft;
    public float MaxLifetime;
    public Color StartColor;
    public Color EndColor;
    public float StartScale;
    public float EndScale;
    public float Rotation;
    public float RotationSpeed;

    public readonly bool IsAlive => LifetimeLeft > 0f;

    /// <summary>Progresso de 0 (recém nascido) a 1 (morto).</summary>
    public readonly float Progress => MaxLifetime > 0f ? 1f - LifetimeLeft / MaxLifetime : 1f;
}
