using Microsoft.Xna.Framework;

namespace HellCoffee.Engine.Collision.Shapes;

public class RectShape : ICollisionShape
{
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle BoundingBox => new((int)Position.X, (int)Position.Y, Width, Height);

    public RectShape(float x, float y, int width, int height)
    {
        Position = new Vector2(x, y);
        Width = width;
        Height = height;
    }

    public RectShape(Rectangle rect)
    {
        Position = new Vector2(rect.X, rect.Y);
        Width = rect.Width;
        Height = rect.Height;
    }

    public bool Intersects(ICollisionShape other) => other switch
    {
        RectShape r => BoundingBox.Intersects(r.BoundingBox),
        CircleShape c => c.Intersects(this),
        _ => BoundingBox.Intersects(other.BoundingBox)
    };

    /// <summary>
    /// Retorna o vetor de separação mínima (MTV) para resolver a sobreposição.
    /// </summary>
    public Vector2 GetSeparation(RectShape other)
    {
        var a = BoundingBox;
        var b = other.BoundingBox;
        if (!a.Intersects(b)) return Vector2.Zero;

        int overlapX = (a.Right < b.Right) ? a.Right - b.Left : b.Right - a.Left;
        int overlapY = (a.Bottom < b.Bottom) ? a.Bottom - b.Top : b.Bottom - a.Top;

        return Math.Abs(overlapX) < Math.Abs(overlapY)
            ? new Vector2(a.Center.X < b.Center.X ? -overlapX : overlapX, 0)
            : new Vector2(0, a.Center.Y < b.Center.Y ? -overlapY : overlapY);
    }
}
