namespace HellCoffee.Engine.Graphics.Tilemap;

[Flags]
public enum TileFlags
{
    None = 0,
    Solid = 1,
    OneWay = 2,  // plataforma passável de baixo para cima
    Lethal = 4,  // instakill (espinhos, lava)
    Water = 8,
    Ladder = 16
}

public readonly struct Tile
{
    public static readonly Tile Empty = new(-1, TileFlags.None);

    public int TilesetId { get; }
    public TileFlags Flags { get; }

    public bool IsEmpty => TilesetId < 0;
    public bool IsSolid => Flags.HasFlag(TileFlags.Solid);
    public bool IsOneWay => Flags.HasFlag(TileFlags.OneWay);
    public bool IsLethal => Flags.HasFlag(TileFlags.Lethal);

    public Tile(int tilesetId, TileFlags flags = TileFlags.None)
    {
        TilesetId = tilesetId;
        Flags = flags;
    }
}
