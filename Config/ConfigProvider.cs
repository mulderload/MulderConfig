using Newtonsoft.Json;

namespace MulderLauncher.Config
{
    public class ConfigProvider
    {
        private ConfigModel? config;

        public ConfigModel GetConfig()
        {
            if (config == null)
            {
                string configPath = Path.Combine(Application.StartupPath, "MulderLauncher.config.json");
                try
                {
                    config = LoadConfig(configPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error: config failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    throw;
                }
            }

            return config;
        }

        private static ConfigModel LoadConfig(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The file 'MulderLauncher.config.json' does not exist.", path);

            try
            {
                string json = File.ReadAllText(path);
                   return JsonConvert.DeserializeObject<ConfigModel>(json)
                       ?? throw new InvalidDataException("The file 'MulderLauncher.config.json' is empty or invalid.");
            }
            catch (JsonException)
            {
                throw new Exception("The file 'MulderLauncher.config.json' is invalid");
            }
        }
    }
}
