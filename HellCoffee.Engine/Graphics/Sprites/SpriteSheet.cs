using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HellCoffee.Engine.Graphics.Sprites;

/// <summary>
/// Configuração de um frame não-uniforme dentro de uma spritesheet.
/// </summary>
public record FrameDefinition(int X, int Y, int Width, int Height);

/// <summary>
/// Recorta automaticamente frames de uma textura.
/// Suporta frames uniformes (grade fixa) ou frames customizados de tamanhos variados.
/// </summary>
public class SpriteSheet
{
    private readonly Texture2D _texture;
    private readonly List<TextureRegion> _frames = new();

    public int FrameCount => _frames.Count;
    public Texture2D Texture => _texture;

    public SpriteSheet(Texture2D texture)
    {
        _texture = texture;
    }

    /// <summary>
    /// Divide a textura em frames de tamanho uniforme (grade regular).
    /// </summary>
    public static SpriteSheet FromGrid(Texture2D texture, int frameWidth, int frameHeight,
        int columns = 0, int rows = 0, int startX = 0, int startY = 0, int spacing = 0, int margin = 0)
    {
        var sheet = new SpriteSheet(texture);
        int cols = columns > 0 ? columns : (texture.Width - margin * 2 + spacing) / (frameWidth + spacing);
        int rowCount = rows > 0 ? rows : (texture.Height - margin * 2 + spacing) / (frameHeight + spacing);

        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int x = startX + margin + c * (frameWidth + spacing);
                int y = startY + margin + r * (frameHeight + spacing);
                sheet._frames.Add(new TextureRegion(texture, x, y, frameWidth, frameHeight));
            }
        }
        return sheet;
    }

    /// <summary>
    /// Cria uma spritesheet com frames de tamanhos diferentes definidos manualmente.
    /// </summary>
    public static SpriteSheet FromFrames(Texture2D texture, IEnumerable<FrameDefinition> frames)
    {
        var sheet = new SpriteSheet(texture);
        foreach (var f in frames)
            sheet._frames.Add(new TextureRegion(texture, f.X, f.Y, f.Width, f.Height));
        return sheet;
    }

    /// <summary>
    /// Cria uma spritesheet com sprites separados (cada um é sua própria textura).
    /// </summary>
    public static SpriteSheet FromTextures(IEnumerable<Texture2D> textures)
    {
        var frames = textures.ToList();
        if (frames.Count == 0) throw new ArgumentException("No textures provided.");
        var sheet = new SpriteSheet(frames[0]);
        foreach (var tex in frames)
            sheet._frames.Add(new TextureRegion(tex));
        return sheet;
    }

    public TextureRegion GetFrame(int index)
    {
        if (index < 0 || index >= _frames.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _frames[index];
    }

    /// <summary>
    /// Retorna um subconjunto de frames pelo índice de início e quantidade.
    /// </summary>
    public IReadOnlyList<TextureRegion> GetFrameRange(int startIndex, int count)
    {
        return _frames.GetRange(startIndex, count);
    }

    public IReadOnlyList<TextureRegion> AllFrames => _frames.AsReadOnly();
}
