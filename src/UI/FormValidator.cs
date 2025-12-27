using MulderConfig.src.Logic;
using MulderConfig.src.Configuration;

namespace MulderConfig.src.UI
{
    public class FormValidator(ConfigModel config, FormSelectionProvider formSelectionProvider)
    {
        public bool IsValid()
        {
            if (config.OptionGroups == null)
                return false;

            foreach (var group in config.OptionGroups)
            {
                if (group.Type != "radioGroup")
                    continue;

                if (!formSelectionProvider.RadioButtons.TryGetValue(group.Name, out var radios))
                    continue;

                if (!radios.Values.Any(rb => rb.Enabled && rb.Checked))
                    return false;
            }

            return true;
        }

        public void ApplyWhenConstraints()
        {
            var selected = formSelectionProvider.GetChoices();
            selected["Title"] = formSelectionProvider.GetTitle();

            foreach (var group in config.OptionGroups)
            {
                if (group.Type == "radioGroup"
                    && group.Radios != null
                    && formSelectionProvider.RadioButtons.TryGetValue(group.Name, out var radios))
                {
                    foreach (var radioRow in group.Radios)
                    {
                        if (radioRow.DisabledWhen == null)
                            continue;

                        if (radios.TryGetValue(radioRow.Value, out var radioButton))
                        {
                            var disable = WhenResolver.Match(radioRow.DisabledWhen, selected);
                            radioButton.Enabled = !disable;
                            if (disable)
                                radioButton.Checked = false;
                        }
                    }
                }
                else if (group.Type == "checkboxGroup" && group.Checkboxes != null)
                {
                    foreach (var checkboxRow in group.Checkboxes)
                    {
                        if (checkboxRow.DisabledWhen == null)
                            continue;

                        if (formSelectionProvider.CheckBoxes.TryGetValue(checkboxRow.Value, out var checkBox))
                        {
                            var disable = WhenResolver.Match(checkboxRow.DisabledWhen, selected);
                            checkBox.Enabled = !disable;
                            if (disable)
                                checkBox.Checked = false;
                        }
                    }
                }
            }
        }
    }
}
