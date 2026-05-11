using Microsoft.Xna.Framework;
using HellCoffee.Engine;

namespace HellCoffee.Engine.Graphics.Camera;

public class Camera2D
{
    public CameraShake Shake { get; } = new();

    private Vector2 _position;
    private float _zoom = 1f;
    private float _rotation = 0f;

    // Limites do mundo (opcional)
    private int _worldWidth;
    private int _worldHeight;
    private bool _hasBounds;

    // Suavização do follow
    private Vector2? _followTarget;
    private float _followSmoothing = 0.1f;
    private Vector2 _followOffset;

    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Max(0.1f, value);
    }

    public float Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    /// <summary>Retorna a matrix de transformação para SpriteBatch.</summary>
    public Matrix GetViewMatrix()
    {
        var screenCenter = new Vector2(Core.VirtualWidth / 2f, Core.VirtualHeight / 2f);
        return Matrix.CreateTranslation(-_position.X, -_position.Y, 0f)
             * Matrix.CreateRotationZ(_rotation)
             * Matrix.CreateScale(_zoom, _zoom, 1f)
             * Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0f)
             * Matrix.CreateTranslation(Shake.Offset.X, Shake.Offset.Y, 0f);
    }

    /// <summary>Posiciona a câmera para centralizar no ponto dado.</summary>
    public void LookAt(Vector2 target) => _position = target;

    public void LookAt(float x, float y) => _position = new Vector2(x, y);

    /// <summary>
    /// Define um alvo de seguimento suave.
    /// smoothing: 0 = instantâneo, ~0.1 = suave, 1 = estático.
    /// </summary>
    public void Follow(Vector2 target, float smoothing = 0.1f, Vector2 offset = default)
    {
        _followTarget = target;
        _followSmoothing = smoothing;
        _followOffset = offset;
    }

    public void ClearFollow() => _followTarget = null;

    /// <summary>Define os limites do mundo para clampar a câmera.</summary>
    public void SetWorldBounds(int worldWidth, int worldHeight)
    {
        _worldWidth = worldWidth;
        _worldHeight = worldHeight;
        _hasBounds = true;
    }

    public void Update(float deltaSeconds)
    {
        Shake.Update(deltaSeconds);

        if (_followTarget.HasValue)
        {
            var desired = _followTarget.Value + _followOffset;
            _position = Vector2.Lerp(_position, desired, 1f - MathF.Pow(_followSmoothing, deltaSeconds * 60f));
        }

        if (_hasBounds)
        {
            float halfW = Core.VirtualWidth / (2f * _zoom);
            float halfH = Core.VirtualHeight / (2f * _zoom);
            _position.X = Math.Clamp(_position.X, halfW, _worldWidth - halfW);
            _position.Y = Math.Clamp(_position.Y, halfH, _worldHeight - halfH);
        }
    }

    /// <summary>Converte posição de tela para posição de mundo.</summary>
    public Vector2 ScreenToWorld(Vector2 screenPos)
        => Vector2.Transform(screenPos, Matrix.Invert(GetViewMatrix()));

    /// <summary>Converte posição de mundo para posição de tela.</summary>
    public Vector2 WorldToScreen(Vector2 worldPos)
        => Vector2.Transform(worldPos, GetViewMatrix());

    /// <summary>Retorna o retângulo visível em coordenadas de mundo.</summary>
    public Rectangle GetWorldViewBounds()
    {
        var topLeft = ScreenToWorld(Vector2.Zero);
        var bottomRight = ScreenToWorld(new Vector2(Core.VirtualWidth, Core.VirtualHeight));
        return new Rectangle(
            (int)topLeft.X, (int)topLeft.Y,
            (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
    }
}
