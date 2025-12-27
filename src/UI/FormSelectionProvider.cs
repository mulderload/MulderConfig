using MulderConfig.src.Configuration;

namespace MulderConfig.src.UI;

public class FormSelectionProvider(ConfigModel config) : ISelectionProvider
{
    private string? addon;
    public readonly Dictionary<string, Dictionary<string, RadioButton>> RadioButtons = [];
    public readonly Dictionary<string, CheckBox> CheckBoxes = [];

    internal void AddRadioButton(string groupName, RadioButton radioButton, string value)
    {
        if (!RadioButtons.ContainsKey(groupName))
            RadioButtons[groupName] = [];

        RadioButtons[groupName][value] = radioButton;
    }

    internal void AddCheckBox(CheckBox checkBox, string value)
    {
        CheckBoxes[value] = checkBox;
    }

    public void SetAddon(string? addon)
    {
        this.addon = addon;
    }

    public string GetAddon()
    {
        return addon ?? "default";
    }

    public void ResetChoices()
    {
        foreach (var grp in RadioButtons)
        {
            foreach (var rb in grp.Value.Values)
            {
                rb.Enabled = true;
                rb.Checked = false;
            }
        }

        foreach (var kv in CheckBoxes)
        {
            var cb = kv.Value;
            if (cb.Checked)
            {
                cb.Checked = false;
                cb.Enabled = true;
            }
        }
    }

    public void ApplyChoices(Dictionary<string, object?> savedChoices)
    {
        foreach (var entry in savedChoices)
        {
            if (entry.Value is IEnumerable<string> values && entry.Value is not string)
            {
                foreach (var value in values)
                {
                    if (CheckBoxes.TryGetValue(value, out var cb))
                    {
                        cb.Checked = true;
                    }
                }

                continue;
            }

            if (entry.Value is string selected)
            {
                var groupName = entry.Key;
                if (RadioButtons.TryGetValue(groupName, out var radios) && radios.TryGetValue(selected, out var rb))
                {
                    rb.Checked = true;
                }
            }
        }
    }

    public Dictionary<string, object?> GetChoices()
    {
        var choices = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in config.OptionGroups)
        {
            if (group.Type == "radioGroup" && RadioButtons.TryGetValue(group.Name, out var radios))
            {
                var selectedRadio = radios.Values.FirstOrDefault(radio => radio.Checked);
                if (selectedRadio != null)
                    choices[group.Name] = selectedRadio.Text;
            }
            else if (group.Type == "checkboxGroup" && group.Checkboxes != null)
            {
                var selectedCheckboxes = group.Checkboxes
                    .Where(checkbox => CheckBoxes.TryGetValue(checkbox.Value, out var checkbox2) && checkbox2.Checked)
                    .Select(checkbox => checkbox.Value)
                    .ToList();

                choices[group.Name] = selectedCheckboxes;
            }
        }

        return choices;
    }
}
