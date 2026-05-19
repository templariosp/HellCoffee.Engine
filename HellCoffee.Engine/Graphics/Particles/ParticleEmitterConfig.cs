using Microsoft.Xna.Framework;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Particles;

/// <summary>
/// Configuração de um emissor de partículas.
/// Crie uma instância, ajuste as propriedades e passe ao ParticleEmitter.
/// </summary>
public class ParticleEmitterConfig
{
    /// <summary>Tamanho máximo do pool (número máximo de partículas simultâneas).</summary>
    public int MaxParticles { get; set; } = 100;

    /// <summary>Partículas emitidas por segundo quando IsLooping=true. 0 = apenas modo burst.</summary>
    public float EmissionRate { get; set; } = 20f;

    public float MinLifetime { get; set; } = 0.5f;
    public float MaxLifetime { get; set; } = 1.5f;

    public Vector2 MinVelocity { get; set; } = new(-30f, -80f);
    public Vector2 MaxVelocity { get; set; } = new(30f, -20f);

    /// <summary>Aceleração constante aplicada a cada partícula (ex: gravidade).</summary>
    public Vector2 Gravity { get; set; } = new(0f, 60f);

    /// <summary>Deslocamento do ponto de spawn em relação a Position.</summary>
    public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

    /// <summary>Raio de dispersão aleatória ao redor do ponto de spawn. 0 = spawn pontual.</summary>
    public float SpawnRadius { get; set; } = 0f;

    public Color StartColor { get; set; } = Color.White;
    public Color EndColor { get; set; } = Color.Transparent;

    public float StartScale { get; set; } = 2f;
    public float EndScale { get; set; } = 0f;

    public float MinRotationSpeed { get; set; } = -3f;
    public float MaxRotationSpeed { get; set; } = 3f;

    /// <summary>true = emite continuamente; false = apenas via Burst().</summary>
    public bool IsLooping { get; set; } = true;

    /// <summary>Textura da partícula. null = ponto de pixel simples (Core.Pixel).</summary>
    public TextureRegion? Texture { get; set; } = null;
}
