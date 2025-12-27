using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MulderConfig.src.Save; 
public class SaveManager
{
    private Dictionary<string, Dictionary<string, object?>>? saves;

    private static string GetSavePath()
    {
        return Path.Combine(Application.StartupPath, "MulderConfig.save.json");
    }

    private Dictionary<string, Dictionary<string, object?>> GetSaves()
    {
        if (saves == null)
        {
            var savePath = GetSavePath();

            if (!File.Exists(savePath))
            {
                saves = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(savePath);
                    saves = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object?>>>(json)
                            ?? new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    saves = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        return saves;
    }

    public Dictionary<string, object?> LoadChoices(string addon)
    {
        if (!GetSaves().TryGetValue(addon, out var save))
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in save)
        {
            normalized[entry.Key] = NormalizeValue(entry.Value);
        }

        return normalized;
    }

    public void SaveChoices(string addon, Dictionary<string, object?> choices)
    {
        var saves = GetSaves();
        saves[addon] = new Dictionary<string, object?>(choices, StringComparer.OrdinalIgnoreCase);

        string json = JsonConvert.SerializeObject(saves, Formatting.Indented);
        File.WriteAllText(GetSavePath(), json);
    }

    private static object? NormalizeValue(object? value)
    {
        if (value is JArray array)
        {
            return array.Values<string>().Where(v => v != null).Select(v => v!).ToList();
        }

        if (value is JValue jValue)
        {
            return jValue.Type == JTokenType.Null ? null : jValue.ToObject<object?>();
        }

        if (value is IList<object?> list)
        {
            return list.Select(v => v?.ToString()).Where(v => v != null).Select(v => v!).ToList();
        }

        return value;
    }
}
