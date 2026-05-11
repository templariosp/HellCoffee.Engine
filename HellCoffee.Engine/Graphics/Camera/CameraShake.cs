using Microsoft.Xna.Framework;

namespace HellCoffee.Engine.Graphics.Camera;

/// <summary>
/// Screen shake com easing quadrático decrescente.
/// Integrado com Camera2D via Offset.
/// </summary>
public class CameraShake
{
    private float _intensity;
    private float _duration;
    private float _elapsed;

    public bool IsActive => _elapsed < _duration;
    public Vector2 Offset { get; private set; }

    public void Trigger(float intensity, float duration)
    {
        _intensity = intensity;
        _duration = duration;
        _elapsed = 0f;
    }

    public void Stop()
    {
        _elapsed = _duration;
        Offset = Vector2.Zero;
    }

    public void Update(float deltaSeconds)
    {
        if (!IsActive)
        {
            Offset = Vector2.Zero;
            return;
        }

        _elapsed += deltaSeconds;
        float t = _elapsed / _duration;
        // easing quadrático: intensidade vai de 1 → 0
        float strength = _intensity * (1f - t * t);

        Offset = new Vector2(
            (float)(Random.Shared.NextDouble() * 2 - 1) * strength,
            (float)(Random.Shared.NextDouble() * 2 - 1) * strength
        );
    }
}
