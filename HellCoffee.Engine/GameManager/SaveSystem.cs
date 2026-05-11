using System.Text.Json;

namespace HellCoffee.Engine.GameManager;

/// <summary>
/// Sistema genérico de save/load local usando JSON.
/// Use T para definir seu SaveData customizado com os dados do jogo.
///
/// Exemplo:
///   var save = SaveSystem.Load<MySaveData>("slot1");
///   save.Level = 3;
///   SaveSystem.Save("slot1", save);
/// </summary>
public static class SaveSystem
{
    private static string GetSavePath(string slot) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HellCoffee", "saves", $"{slot}.json");

    public static void Save<T>(string slot, T data)
    {
        var path = GetSavePath(slot);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static T Load<T>(string slot) where T : new()
    {
        var path = GetSavePath(slot);
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? new T();
        }
        catch { }
        return new T();
    }

    public static bool HasSave(string slot) => File.Exists(GetSavePath(slot));

    public static void DeleteSave(string slot)
    {
        var path = GetSavePath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public static IEnumerable<string> GetSaveSlots()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HellCoffee", "saves");
        if (!Directory.Exists(dir)) yield break;
        foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
            yield return Path.GetFileNameWithoutExtension(file);
    }
}
