using Microsoft.Xna.Framework;
using HellCoffee.Engine.Graphics.Tilemap;

namespace HellCoffee.Engine.Collision;

/// <summary>
/// Utilitários para resolver colisão de entidades com tilemaps.
/// Retorna o vetor de separação para empurrar a entidade para fora dos tiles.
/// </summary>
public static class TileCollision
{
    /// <summary>
    /// Resolve colisão de um retângulo contra um tilemap.
    /// Retorna o deslocamento que deve ser aplicado à posição da entidade.
    /// </summary>
    public static Vector2 Resolve(Rectangle entityBounds, TilemapLayer layer, bool oneWayFromAbove = false)
    {
        // Usa o MÁXIMO de separação em cada eixo, não a soma.
        // Somar separações de múltiplos tiles da mesma linha/coluna causaria
        // empurrões excessivos (N tiles × overlap = bounce involuntário).
        float maxSepX = 0f, maxSepY = 0f;

        foreach (var tileRect in layer.GetSolidTileRects(entityBounds))
        {
            // One-way: só colide vindo de cima
            if (oneWayFromAbove)
            {
                var tile = layer.GetTileAtPixel(tileRect.Center.X, tileRect.Center.Y);
                if (tile.IsOneWay && entityBounds.Bottom - tileRect.Top > 4) continue;
            }

            int overlapX = entityBounds.Right < tileRect.Right
                ? entityBounds.Right - tileRect.Left
                : tileRect.Right - entityBounds.Left;

            int overlapY = entityBounds.Bottom < tileRect.Bottom
                ? entityBounds.Bottom - tileRect.Top
                : tileRect.Bottom - entityBounds.Top;

            if (overlapX < overlapY)
            {
                float sx = entityBounds.Center.X < tileRect.Center.X ? -overlapX : overlapX;
                if (MathF.Abs(sx) > MathF.Abs(maxSepX)) maxSepX = sx;
            }
            else
            {
                float sy = entityBounds.Center.Y < tileRect.Center.Y ? -overlapY : overlapY;
                if (MathF.Abs(sy) > MathF.Abs(maxSepY)) maxSepY = sy;
            }
        }

        return new Vector2(maxSepX, maxSepY);
    }

    /// <summary>Verifica se a entidade está tocando o chão (tile sólido logo abaixo).</summary>
    public static bool IsGrounded(Rectangle entityBounds, TilemapLayer layer)
    {
        var checkRect = new Rectangle(entityBounds.X + 1, entityBounds.Bottom, entityBounds.Width - 2, 2);
        return layer.IntersectsSolid(checkRect);
    }

    /// <summary>Verifica se há parede à esquerda da entidade.</summary>
    public static bool HasWallLeft(Rectangle entityBounds, TilemapLayer layer)
    {
        var checkRect = new Rectangle(entityBounds.Left - 2, entityBounds.Y + 2, 2, entityBounds.Height - 4);
        return layer.IntersectsSolid(checkRect);
    }

    /// <summary>Verifica se há parede à direita da entidade.</summary>
    public static bool HasWallRight(Rectangle entityBounds, TilemapLayer layer)
    {
        var checkRect = new Rectangle(entityBounds.Right, entityBounds.Y + 2, 2, entityBounds.Height - 4);
        return layer.IntersectsSolid(checkRect);
    }

    /// <summary>Verifica se há teto acima da entidade.</summary>
    public static bool HasCeiling(Rectangle entityBounds, TilemapLayer layer)
    {
        var checkRect = new Rectangle(entityBounds.X + 1, entityBounds.Top - 2, entityBounds.Width - 2, 2);
        return layer.IntersectsSolid(checkRect);
    }
}
