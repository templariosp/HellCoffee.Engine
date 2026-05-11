using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using HellCoffee.Engine.Graphics.Animation;
using HellCoffee.Engine.Graphics.Sprites;

namespace HellCoffee.Engine.Graphics;

/// <summary>
/// Gerencia regiões e animações de uma textura atlas.
/// Suporta carregamento via XML com sintaxe simples.
///
/// Formato XML suportado:
/// <TextureAtlas texture="images/atlas">
///   <Regions>
///     <Region name="player-idle" x="0" y="0" width="16" height="16" />
///     <!-- Frame não-uniforme com atributos opcionais -->
///   </Regions>
///   <Animations>
///     <Animation name="player-run" delay="0.1" loop="true">
///       <Frame region="run-1" />
///       <Frame region="run-2" delay="0.15" />  <!-- override por frame -->
///     </Animation>
///     <!-- Grid animation: gera frames automaticamente de uma spritesheet -->
///     <Animation name="player-idle" delay="0.2" frameWidth="16" frameHeight="16" row="0" columns="4" />
///   </Animations>
/// </TextureAtlas>
/// </summary>
public class TextureAtlas
{
    private readonly Dictionary<string, TextureRegion> _regions = new();
    private readonly Dictionary<string, Animation.Animation> _animations = new();

    public Texture2D Texture { get; private set; }

    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
    }

    public TextureAtlas AddRegion(string name, int x, int y, int width, int height)
    {
        _regions[name] = new TextureRegion(Texture, x, y, width, height);
        return this;
    }

    public TextureAtlas AddAnimation(string name, Animation.Animation animation)
    {
        _animations[name] = animation;
        return this;
    }

    public TextureRegion GetRegion(string name)
    {
        if (!_regions.TryGetValue(name, out var region))
            throw new KeyNotFoundException($"Region '{name}' not found in atlas.");
        return region;
    }

    public Animation.Animation GetAnimation(string name)
    {
        if (!_animations.TryGetValue(name, out var anim))
            throw new KeyNotFoundException($"Animation '{name}' not found in atlas.");
        return anim;
    }

    public bool HasRegion(string name) => _regions.ContainsKey(name);
    public bool HasAnimation(string name) => _animations.ContainsKey(name);

    public Sprite CreateSprite(string regionName)
    {
        return new Sprite(GetRegion(regionName));
    }

    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        var sprite = new AnimatedSprite();
        sprite.Controller.Register(GetAnimation(animationName));
        sprite.Play(animationName);
        return sprite;
    }

    /// <summary>
    /// Cria um AnimatedSprite com múltiplas animações pré-registradas.
    /// Útil para criar um sprite com todas as animações do personagem de uma vez.
    /// </summary>
    public AnimatedSprite CreateAnimatedSprite(params string[] animationNames)
    {
        var sprite = new AnimatedSprite();
        foreach (var name in animationNames)
            sprite.Controller.Register(GetAnimation(name));
        if (animationNames.Length > 0)
            sprite.Play(animationNames[0]);
        return sprite;
    }

    /// <summary>
    /// Carrega o atlas a partir de um arquivo XML no ContentManager.
    /// </summary>
    public static TextureAtlas FromFile(ContentManager content, string xmlPath)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, xmlPath));
        var doc = XDocument.Load(stream);
        var root = doc.Root!;

        var texturePath = root.Attribute("texture")?.Value
            ?? root.Element("Texture")?.Value
            ?? throw new Exception("TextureAtlas XML missing 'texture' attribute.");

        var texture = content.Load<Texture2D>(texturePath);
        var atlas = new TextureAtlas(texture);

        // Parse regions
        foreach (var elem in root.Element("Regions")?.Elements("Region") ?? [])
        {
            var name = elem.Attribute("name")!.Value;
            int x = int.Parse(elem.Attribute("x")!.Value);
            int y = int.Parse(elem.Attribute("y")!.Value);
            int w = int.Parse(elem.Attribute("width")!.Value);
            int h = int.Parse(elem.Attribute("height")!.Value);
            atlas.AddRegion(name, x, y, w, h);
        }

        // Parse animations
        foreach (var animElem in root.Element("Animations")?.Elements("Animation") ?? [])
        {
            string name = animElem.Attribute("name")!.Value;
            float defaultDelay = float.Parse(animElem.Attribute("delay")?.Value ?? "0.1",
                System.Globalization.CultureInfo.InvariantCulture);
            bool loop = bool.Parse(animElem.Attribute("loop")?.Value ?? "true");

            // Grid animation: gera frames de uma grade automaticamente
            if (animElem.Attribute("frameWidth") != null)
            {
                int fw = int.Parse(animElem.Attribute("frameWidth")!.Value);
                int fh = int.Parse(animElem.Attribute("frameHeight")!.Value);
                int row = int.Parse(animElem.Attribute("row")?.Value ?? "0");
                int cols = int.Parse(animElem.Attribute("columns")!.Value);
                int startCol = int.Parse(animElem.Attribute("startColumn")?.Value ?? "0");

                var frames = new List<AnimationFrame>();
                for (int c = startCol; c < startCol + cols; c++)
                {
                    var region = new TextureRegion(texture, c * fw, row * fh, fw, fh);
                    frames.Add(new AnimationFrame(region, defaultDelay));
                }
                atlas.AddAnimation(name, new Animation.Animation(name, frames, loop));
                continue;
            }

            // Frame-by-frame animation
            {
                var frames = new List<AnimationFrame>();
                foreach (var frameElem in animElem.Elements("Frame"))
                {
                    var regionName = frameElem.Attribute("region")!.Value;
                    float delay = float.Parse(frameElem.Attribute("delay")?.Value
                        ?? defaultDelay.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        System.Globalization.CultureInfo.InvariantCulture);
                    frames.Add(new AnimationFrame(atlas.GetRegion(regionName), delay));
                }
                atlas.AddAnimation(name, new Animation.Animation(name, frames, loop));
            }
        }

        return atlas;
    }
}
