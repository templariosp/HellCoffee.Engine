using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.Collision;

/// <summary>
/// Colisão pixel-perfect. Use para casos onde a precisão visual importa
/// (ex: projéteis vs sprites detalhados). Mais caro que AABB.
///
/// Antes de usar, extraia os dados de cor com:
///   Color[] data = new Color[tex.Width * tex.Height];
///   tex.GetData(data);
/// </summary>
public static class PixelCollision
{
    /// <summary>
    /// Colisão pixel-perfect simples (sem rotação/escala).
    /// Compara os pixels sobrepostos de dois objetos pelos seus bounds e dados de cor.
    /// </summary>
    public static bool Check(Rectangle boundsA, Color[] dataA,
                             Rectangle boundsB, Color[] dataB)
    {
        if (!boundsA.Intersects(boundsB)) return false;

        int left = Math.Max(boundsA.Left, boundsB.Left);
        int top = Math.Max(boundsA.Top, boundsB.Top);
        int right = Math.Min(boundsA.Right, boundsB.Right);
        int bottom = Math.Min(boundsA.Bottom, boundsB.Bottom);

        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                int idxA = (x - boundsA.Left) + (y - boundsA.Top) * boundsA.Width;
                int idxB = (x - boundsB.Left) + (y - boundsB.Top) * boundsB.Width;

                if (dataA[idxA].A != 0 && dataB[idxB].A != 0)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Colisão pixel-perfect com suporte a transformações (rotação e escala).
    /// transformA e transformB são as matrizes mundo de cada objeto.
    /// </summary>
    public static bool CheckTransformed(
        Matrix transformA, int widthA, int heightA, Color[] dataA,
        Matrix transformB, int widthB, int heightB, Color[] dataB)
    {
        Matrix aToBMatrix = transformA * Matrix.Invert(transformB);
        Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, aToBMatrix);
        Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, aToBMatrix);
        Vector2 rowStart = Vector2.Transform(Vector2.Zero, aToBMatrix);

        for (int yA = 0; yA < heightA; yA++)
        {
            Vector2 posInB = rowStart;
            for (int xA = 0; xA < widthA; xA++)
            {
                int xB = (int)Math.Round(posInB.X);
                int yB = (int)Math.Round(posInB.Y);

                if (xB >= 0 && xB < widthB && yB >= 0 && yB < heightB)
                {
                    if (dataA[xA + yA * widthA].A != 0 && dataB[xB + yB * widthB].A != 0)
                        return true;
                }
                posInB += stepX;
            }
            rowStart += stepY;
        }
        return false;
    }

    /// <summary>Extrai dados de cor de uma textura para uso em pixel collision.</summary>
    public static Color[] GetTextureData(Texture2D texture)
    {
        var data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        return data;
    }
}
