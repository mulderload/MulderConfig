using MulderConfig.Save;

namespace MulderConfig.UI;

public sealed class FormController(
    FormSelectionProvider selectionProvider,
    FormValidator validator,
    Button btnApply,
    Button btnSave)
{
    public void LoadSavedChoices(SaveLoader saveLoader)
    {
        var saved = saveLoader.Load(selectionProvider.GetTitle());
        ResetChoices();
        ApplyChoices(saved);

        validator.ApplyWhenConstraints();
        UpdateButtons();
    }

    private void ResetChoices()
    {
        foreach (var grp in selectionProvider.RadioButtons)
        {
            foreach (var rb in grp.Value.Values)
            {
                rb.Enabled = true;
                rb.Checked = false;
            }
        }

        foreach (var cb in selectionProvider.CheckBoxes.Values)
        {
            cb.Enabled = true;
            cb.Checked = false;
        }
    }

    private void ApplyChoices(Dictionary<string, object?> savedChoices)
    {
        foreach (var entry in savedChoices)
        {
            if (entry.Value is IEnumerable<string> values && entry.Value is not string)
            {
                foreach (var value in values)
                {
                    if (selectionProvider.CheckBoxes.TryGetValue(value, out var cb))
                    {
                        cb.Checked = true;
                    }
                }

                continue;
            }

            if (entry.Value is string selected)
            {
                var groupName = entry.Key;
                if (selectionProvider.RadioButtons.TryGetValue(groupName, out var radios) && radios.TryGetValue(selected, out var rb))
                {
                    rb.Checked = true;
                }
            }
        }
    }

    public void UpdateButtons()
    {
        var isValid = validator.IsValid();
        btnApply.Enabled = isValid;
        btnSave.Enabled = isValid;
    }
}
