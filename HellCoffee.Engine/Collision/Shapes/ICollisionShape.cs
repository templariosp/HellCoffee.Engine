using Microsoft.Xna.Framework;

namespace HellCoffee.Engine.Collision.Shapes;

public interface ICollisionShape
{
    Vector2 Position { get; set; }
    Rectangle BoundingBox { get; }
    bool Intersects(ICollisionShape other);
}
