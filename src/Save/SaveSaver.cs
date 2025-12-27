using Newtonsoft.Json;

namespace MulderConfig.src.Save;

public sealed class SaveSaver(SaveLoader loader)
{
    public void SaveChoices(string addon, Dictionary<string, object?> choices)
    {
        var saves = loader.LoadAll();

        saves[addon] = new Dictionary<string, object?>(choices, StringComparer.OrdinalIgnoreCase);

        var json = JsonConvert.SerializeObject(saves, Formatting.Indented);
        File.WriteAllText(GetSavePath(), json);

        // keep loader cache in sync
        loader.SetCache(saves);
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.StartupPath, "MulderConfig.save.json");
    }
}
