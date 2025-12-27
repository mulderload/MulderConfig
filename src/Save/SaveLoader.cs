using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MulderConfig.src.Save;

public sealed class SaveLoader
{
    private Dictionary<string, Dictionary<string, object?>>? _saves;

    public Dictionary<string, Dictionary<string, object?>> LoadAll()
    {
        if (_saves != null)
            return _saves;

        var savePath = GetSavePath();

        if (!File.Exists(savePath))
        {
            _saves = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
            return _saves;
        }

        try
        {
            var json = File.ReadAllText(savePath);
            _saves = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object?>>>(json)
                     ?? new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            _saves = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
        }

        return _saves;
    }

    public Dictionary<string, object?> Load(string addon)
    {
        if (_saves == null)
            throw new InvalidOperationException("LoadAll must be called before Load.");

        if (!_saves.TryGetValue(addon, out var save))
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in save)
        {
            normalized[entry.Key] = NormalizeValue(entry.Value);
        }

        return normalized;
    }

    internal void SetCache(Dictionary<string, Dictionary<string, object?>> newCache)
    {
        _saves = newCache;
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.StartupPath, "MulderConfig.save.json");
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
