using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MulderLauncher.Selections;

namespace MulderLauncher.Save
{
    public class SavedSelectionProvider : ISelectionProvider
    {
        private readonly string addon;
        private readonly Dictionary<string, object?> choices;

        public SavedSelectionProvider(string addon)
        {
            this.addon = addon;
            this.choices = LoadChoicesFromSaveFile(addon);
        }

        public string GetAddon() => addon;

        public Dictionary<string, object?> GetChoices() => new(choices, StringComparer.OrdinalIgnoreCase);

        private static string GetSavePath() => Path.Combine(Application.StartupPath, "MulderLauncher.save.json");

        private static Dictionary<string, object?> LoadChoicesFromSaveFile(string addon)
        {
            var savePath = GetSavePath();
            if (!File.Exists(savePath))
                return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var json = File.ReadAllText(savePath);
                var root = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object?>>>(json)
                           ?? new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

                if (!root.TryGetValue(addon, out var raw))
                    return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                var converted = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var (k, v) in raw)
                {
                    if (v is JArray ja)
                    {
                        var list = new List<string>();
                        foreach (var t in ja)
                            list.Add(t.ToString());
                        converted[k] = list;
                    }
                    else if (v is JValue jv)
                    {
                        converted[k] = jv.ToString();
                    }
                    else
                    {
                        converted[k] = v;
                    }
                }

                return converted;
            }
            catch
            {
                return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
