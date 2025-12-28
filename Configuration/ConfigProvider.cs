using Newtonsoft.Json;

namespace MulderConfig.Configuration;

public class ConfigProvider
{
    private const string FILENAME = "MulderConfig.json";

    public static ConfigModel GetConfig()
    {
        string configPath = Path.Combine(Application.StartupPath, FILENAME);

        if (!File.Exists(configPath))
            throw new FileNotFoundException($"The file '{FILENAME}' does not exist.", configPath);

        try
        {
            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ConfigModel>(json) ?? throw new InvalidDataException($"The file '{FILENAME}' is empty or invalid.");
        }
        catch (JsonException)
        {
            throw new Exception($"The file '{FILENAME}' is an invalid json.");
        }
    }
}
