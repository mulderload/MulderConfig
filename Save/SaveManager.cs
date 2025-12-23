using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MulderLauncher.UI;

namespace MulderLauncher.Save
{
    public class SaveManager(FormStateManager formStateManager)
    {
        private Dictionary<string, Dictionary<string, object>>? saves;

        private static string GetSavePath()
        {
            return Path.Combine(Application.StartupPath, "MulderLauncher.save.json");
        }

        private Dictionary<string, Dictionary<string, object>> GetSaves()
        {
            if (saves == null)
            {
                var savePath = GetSavePath();

                if (!File.Exists(savePath))
                {
                    saves = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    try
                    {
                        var json = File.ReadAllText(savePath);
                        saves = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json)
                                ?? new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to load saved configuration.");
                        saves = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return saves;
        }

        public void LoadChoices()
        {
            if (!GetSaves().TryGetValue(formStateManager.GetAddon(), out Dictionary<string, object>? save))
                return;

            formStateManager.ResetChoices();

            foreach (var entry in save)
            {
                // checkboxGroup case
                if (entry.Value is JArray jEntryValue)
                {
                    foreach (JToken jValue in jEntryValue)
                    {
                        var value = jValue.ToString();
                        if (formStateManager.CheckBoxes.TryGetValue(value, out var cb))
                        {
                            cb.Checked = true;
                        }
                    }
                }
                // radioGroup case
                else if (entry.Value is string)
                {
                    string groupName = entry.Key;
                    var value = entry.Value.ToString();
                    if (value != null && formStateManager.RadioButtons.TryGetValue(groupName, out var radios) && radios.TryGetValue(value, out var rb))
                    {
                        rb.Checked = true;
                    }
                }
            }
        }

        public void SaveChoices()
        {
            var saves = GetSaves();
            saves[formStateManager.GetAddon()] = formStateManager.GetChoices()!;

            string json = JsonConvert.SerializeObject(saves, Formatting.Indented);
            File.WriteAllText(GetSavePath(), json);
        }
    }
}
