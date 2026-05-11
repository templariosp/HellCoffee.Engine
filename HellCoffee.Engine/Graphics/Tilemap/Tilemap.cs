using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Graphics.Sprites;
using HellCoffee.Engine.Graphics.Camera;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Tilemap;

/// <summary>
/// Tilemap multi-camadas com suporte a colisão por flags.
///
/// Formato XML:
/// <Tilemap tileWidth="16" tileHeight="16">
///   <Tileset name="main" texture="images/tiles" region="0 0 256 256" />
///
///   <!-- Flags de colisão por ID de tile do tileset -->
///   <CollisionFlags>
///     <Solid ids="1,2,3,4-10" />
///     <OneWay ids="20,21" />
///     <Lethal ids="30" />
///   </CollisionFlags>
///
///   <Layer name="background" tileset="main" collision="false">
///     00 00 01 01
///     02 02 03 03
///   </Layer>
///   <Layer name="ground" tileset="main" collision="true">
///     ...
///   </Layer>
/// </Tilemap>
/// </summary>
public class Tilemap
{
    private readonly List<TilemapLayer> _layers = new();
    private readonly Dictionary<string, Tileset> _tilesets = new();
    private readonly Dictionary<int, TileFlags> _tileFlags = new();

    public IReadOnlyList<TilemapLayer> Layers => _layers.AsReadOnly();
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public Vector2 Scale { get; set; } = Vector2.One;

    public Tilemap(int tileWidth, int tileHeight)
    {
        TileWidth = tileWidth;
        TileHeight = tileHeight;
    }

    public void AddTileset(string name, Tileset tileset) => _tilesets[name] = tileset;

    public void SetTileFlags(int tileId, TileFlags flags) => _tileFlags[tileId] = flags;

    public TileFlags GetTileFlags(int tileId)
        => _tileFlags.TryGetValue(tileId, out var f) ? f : TileFlags.None;

    public TilemapLayer AddLayer(string name, string tilesetName, int columns, int rows, bool isCollision = false)
    {
        if (!_tilesets.TryGetValue(tilesetName, out var ts))
            throw new KeyNotFoundException($"Tileset '{tilesetName}' not found.");
        var layer = new TilemapLayer(name, ts, columns, rows) { IsCollisionLayer = isCollision };
        layer.Scale = Scale;
        _layers.Add(layer);
        return layer;
    }

    public TilemapLayer GetLayer(string name)
        => _layers.FirstOrDefault(l => l.Name == name)
           ?? throw new KeyNotFoundException($"Layer '{name}' not found.");

    public bool TryGetLayer(string name, out TilemapLayer layer)
    {
        layer = _layers.FirstOrDefault(l => l.Name == name);
        return layer != null;
    }

    /// <summary>Verifica colisão sólida em qualquer camada marcada como colisão.</summary>
    public bool IsSolidAt(int worldX, int worldY)
        => _layers.Where(l => l.IsCollisionLayer).Any(l => l.IsSolidAt(worldX, worldY));

    public bool IntersectsSolid(Rectangle worldBounds)
        => _layers.Where(l => l.IsCollisionLayer).Any(l => l.IntersectsSolid(worldBounds));

    public IEnumerable<Rectangle> GetSolidTileRects(Rectangle worldBounds)
        => _layers.Where(l => l.IsCollisionLayer).SelectMany(l => l.GetSolidTileRects(worldBounds));

    public void Draw(SpriteBatch spriteBatch, Camera2D camera = null)
    {
        foreach (var layer in _layers)
            layer.Draw(spriteBatch, camera);
    }

    public static Tilemap FromFile(ContentManager content, string xmlPath)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, xmlPath));
        var doc = XDocument.Load(stream);
        var root = doc.Root!;

        int tw = int.Parse(root.Attribute("tileWidth")!.Value);
        int th = int.Parse(root.Attribute("tileHeight")!.Value);
        var tilemap = new Tilemap(tw, th);

        // Load tilesets
        foreach (var tsElem in root.Elements("Tileset"))
        {
            var tsName = tsElem.Attribute("name")?.Value ?? "default";
            var texturePath = tsElem.Attribute("texture")!.Value;
            var texture = content.Load<Texture2D>(texturePath);

            TextureRegion region;
            var regionAttr = tsElem.Attribute("region");
            if (regionAttr != null)
            {
                var parts = regionAttr.Value.Split(' ');
                region = new TextureRegion(texture,
                    int.Parse(parts[0]), int.Parse(parts[1]),
                    int.Parse(parts[2]), int.Parse(parts[3]));
            }
            else
            {
                region = new TextureRegion(texture);
            }

            int spacing = int.Parse(tsElem.Attribute("spacing")?.Value ?? "0");
            int margin = int.Parse(tsElem.Attribute("margin")?.Value ?? "0");
            tilemap.AddTileset(tsName, new Tileset(region, tw, th, spacing, margin));
        }

        // Load collision flags
        var flagsElem = root.Element("CollisionFlags");
        if (flagsElem != null)
        {
            foreach (var flagElem in flagsElem.Elements())
            {
                var flagType = Enum.Parse<TileFlags>(flagElem.Name.LocalName, true);
                var ids = ParseIdList(flagElem.Attribute("ids")!.Value);
                foreach (var id in ids)
                    tilemap.SetTileFlags(id, tilemap.GetTileFlags(id) | flagType);
            }
        }

        // Load layers
        foreach (var layerElem in root.Elements("Layer"))
        {
            var name = layerElem.Attribute("name")?.Value ?? "layer";
            var tsName = layerElem.Attribute("tileset")?.Value ?? "default";
            bool isCollision = bool.Parse(layerElem.Attribute("collision")?.Value ?? "false");
            int cols = int.Parse(layerElem.Attribute("columns")!.Value);
            int rows = int.Parse(layerElem.Attribute("rows")!.Value);

            var layer = tilemap.AddLayer(name, tsName, cols, rows, isCollision);

            var rawText = layerElem.Value.Trim();
            var tokens = rawText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Math.Min(tokens.Length, cols * rows); i++)
            {
                int tileId = int.Parse(tokens[i]) - 1; // 0 = empty, 1+ = tileset id 0+
                if (tileId < 0) continue;
                var flags = tilemap.GetTileFlags(tileId);
                layer.SetTile(i, new Tile(tileId, flags));
            }
        }

        return tilemap;
    }

    private static IEnumerable<int> ParseIdList(string value)
    {
        foreach (var part in value.Split(','))
        {
            var trimmed = part.Trim();
            if (trimmed.Contains('-'))
            {
                var range = trimmed.Split('-');
                int start = int.Parse(range[0]);
                int end = int.Parse(range[1]);
                for (int i = start; i <= end; i++) yield return i;
            }
            else
            {
                yield return int.Parse(trimmed);
            }
        }
    }
}
