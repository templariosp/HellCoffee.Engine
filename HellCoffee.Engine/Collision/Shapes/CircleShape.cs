using Microsoft.Xna.Framework;

namespace HellCoffee.Engine.Collision.Shapes;

public class CircleShape : ICollisionShape
{
    public Vector2 Position { get; set; }
    public float Radius { get; set; }

    public Vector2 Center => new(Position.X + Radius, Position.Y + Radius);

    public Rectangle BoundingBox => new(
        (int)(Position.X), (int)(Position.Y),
        (int)(Radius * 2), (int)(Radius * 2));

    public CircleShape(float centerX, float centerY, float radius)
    {
        Position = new Vector2(centerX - radius, centerY - radius);
        Radius = radius;
    }

    public CircleShape(Vector2 center, float radius)
        : this(center.X, center.Y, radius) { }

    public bool Intersects(ICollisionShape other) => other switch
    {
        CircleShape c => Vector2.DistanceSquared(Center, c.Center) < (Radius + c.Radius) * (Radius + c.Radius),
        RectShape r => IntersectsRect(r.BoundingBox),
        _ => BoundingBox.Intersects(other.BoundingBox)
    };

    private bool IntersectsRect(Rectangle rect)
    {
        float nearX = Math.Clamp(Center.X, rect.Left, rect.Right);
        float nearY = Math.Clamp(Center.Y, rect.Top, rect.Bottom);
        float dx = Center.X - nearX;
        float dy = Center.Y - nearY;
        return dx * dx + dy * dy < Radius * Radius;
    }

    /// <summary>
    /// Retorna o vetor de separação entre dois círculos.
    /// </summary>
    public Vector2 GetSeparation(CircleShape other)
    {
        var diff = Center - other.Center;
        float dist = diff.Length();
        float overlap = Radius + other.Radius - dist;
        if (overlap <= 0) return Vector2.Zero;
        return dist > 0 ? diff / dist * overlap : new Vector2(overlap, 0);
    }
}
