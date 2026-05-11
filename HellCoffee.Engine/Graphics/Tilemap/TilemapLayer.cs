using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Graphics.Camera;

namespace HellCoffee.Engine.Graphics.Tilemap;

/// <summary>
/// Uma camada do tilemap. Cada camada tem seu próprio grid de tiles e tileset.
/// Suporta múltiplas camadas para efeitos de parallax ou separação visual/lógica.
/// </summary>
public class TilemapLayer
{
    private readonly Tile[] _tiles;

    public string Name { get; }
    public Tileset Tileset { get; }
    public int Columns { get; }
    public int Rows { get; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public bool IsVisible { get; set; } = true;
    public bool IsCollisionLayer { get; set; } = false;
    public Color Tint { get; set; } = Color.White;

    public int TileWidth => (int)(Tileset.TileWidth * Scale.X);
    public int TileHeight => (int)(Tileset.TileHeight * Scale.Y);
    public int PixelWidth => Columns * TileWidth;
    public int PixelHeight => Rows * TileHeight;

    public TilemapLayer(string name, Tileset tileset, int columns, int rows)
    {
        Name = name;
        Tileset = tileset;
        Columns = columns;
        Rows = rows;
        _tiles = new Tile[columns * rows];
        for (int i = 0; i < _tiles.Length; i++)
            _tiles[i] = Tile.Empty;
    }

    public void SetTile(int index, Tile tile) => _tiles[index] = tile;
    public void SetTile(int col, int row, Tile tile) => _tiles[row * Columns + col] = tile;
    public Tile GetTile(int index) => _tiles[index];
    public Tile GetTile(int col, int row) => _tiles[row * Columns + col];

    /// <summary>Retorna o tile na posição de pixel mundial (considera Scale).</summary>
    public Tile GetTileAtPixel(int worldX, int worldY)
    {
        int col = worldX / TileWidth;
        int row = worldY / TileHeight;
        if (col < 0 || col >= Columns || row < 0 || row >= Rows) return Tile.Empty;
        return GetTile(col, row);
    }

    /// <summary>Verifica se o ponto em pixel colide com tile sólido.</summary>
    public bool IsSolidAt(int worldX, int worldY)
    {
        var tile = GetTileAtPixel(worldX, worldY);
        return !tile.IsEmpty && tile.IsSolid;
    }

    /// <summary>Verifica colisão retangular com tiles da camada.</summary>
    public bool IntersectsSolid(Rectangle worldBounds)
    {
        int startCol = Math.Max(0, worldBounds.Left / TileWidth);
        int endCol = Math.Min(Columns - 1, (worldBounds.Right - 1) / TileWidth);
        int startRow = Math.Max(0, worldBounds.Top / TileHeight);
        int endRow = Math.Min(Rows - 1, (worldBounds.Bottom - 1) / TileHeight);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var tile = GetTile(col, row);
                if (!tile.IsEmpty && tile.IsSolid)
                    return true;
            }
        }
        return false;
    }

    /// <summary>Retorna todos os retângulos de tiles sólidos que intersectam os bounds.</summary>
    public IEnumerable<Rectangle> GetSolidTileRects(Rectangle worldBounds)
    {
        int startCol = Math.Max(0, worldBounds.Left / TileWidth);
        int endCol = Math.Min(Columns - 1, (worldBounds.Right - 1) / TileWidth);
        int startRow = Math.Max(0, worldBounds.Top / TileHeight);
        int endRow = Math.Min(Rows - 1, (worldBounds.Bottom - 1) / TileHeight);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var tile = GetTile(col, row);
                if (!tile.IsEmpty && tile.IsSolid)
                    yield return new Rectangle(col * TileWidth, row * TileHeight, TileWidth, TileHeight);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Camera2D camera = null)
    {
        if (!IsVisible) return;

        int viewLeft = 0, viewTop = 0, viewRight = Columns, viewBottom = Rows;

        if (camera != null)
        {
            var viewBounds = camera.GetWorldViewBounds();
            viewLeft = Math.Max(0, viewBounds.Left / TileWidth);
            viewTop = Math.Max(0, viewBounds.Top / TileHeight);
            viewRight = Math.Min(Columns, viewBounds.Right / TileWidth + 1);
            viewBottom = Math.Min(Rows, viewBounds.Bottom / TileHeight + 1);
        }

        for (int row = viewTop; row < viewBottom; row++)
        {
            for (int col = viewLeft; col < viewRight; col++)
            {
                var tile = GetTile(col, row);
                if (tile.IsEmpty) continue;

                var region = Tileset.GetTile(tile.TilesetId);
                if (region == null) continue;

                var dest = new Rectangle(col * TileWidth, row * TileHeight, TileWidth, TileHeight);
                spriteBatch.Draw(region.Texture, dest, region.SourceRectangle, Tint);
            }
        }
    }
}
