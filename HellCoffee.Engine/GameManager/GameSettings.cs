using System.Text.Json;

namespace HellCoffee.Engine.GameManager;

public class GameSettings
{
    public float MusicVolume { get; set; } = 0.8f;
    public float SfxVolume { get; set; } = 1.0f;
    public bool Fullscreen { get; set; } = false;
    public bool ShowFps { get; set; } = false;
    public string Language { get; set; } = "en";
    public Dictionary<string, string> KeyBindings { get; set; } = new();

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HellCoffee", "settings.json");

    public static GameSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { }
        return new GameSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
