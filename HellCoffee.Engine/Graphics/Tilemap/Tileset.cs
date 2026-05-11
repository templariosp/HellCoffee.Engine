using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics.Tilemap;

public class Tileset
{
    private readonly TextureRegion[] _tiles;

    public Texture2D Texture { get; }
    public int TileWidth { get; }
    public int TileHeight { get; }
    public int Columns { get; }
    public int Rows { get; }
    public int Count => _tiles.Length;

    /// <summary>
    /// Cria um tileset dividindo uma região da textura em tiles de tamanho uniforme.
    /// </summary>
    public Tileset(TextureRegion region, int tileWidth, int tileHeight, int spacing = 0, int margin = 0)
    {
        Texture = region.Texture;
        TileWidth = tileWidth;
        TileHeight = tileHeight;

        Columns = (region.Width - margin * 2 + spacing) / (tileWidth + spacing);
        Rows = (region.Height - margin * 2 + spacing) / (tileHeight + spacing);
        _tiles = new TextureRegion[Columns * Rows];

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                int x = region.X + margin + col * (tileWidth + spacing);
                int y = region.Y + margin + row * (tileHeight + spacing);
                _tiles[row * Columns + col] = new TextureRegion(region.Texture, x, y, tileWidth, tileHeight);
            }
        }
    }

    /// <summary>Cria tileset a partir de uma textura inteira.</summary>
    public Tileset(Texture2D texture, int tileWidth, int tileHeight, int spacing = 0, int margin = 0)
        : this(new TextureRegion(texture), tileWidth, tileHeight, spacing, margin)
    {
    }

    public TextureRegion GetTile(int index)
    {
        if (index < 0 || index >= _tiles.Length) return null;
        return _tiles[index];
    }

    public TextureRegion GetTile(int column, int row)
        => GetTile(row * Columns + column);
}
