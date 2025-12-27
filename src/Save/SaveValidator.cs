using MulderConfig.src.Configuration;

namespace MulderConfig.src.Save;

public static class SaveValidator
{
    public static bool IsValid(ConfigModel config, IReadOnlyDictionary<string, object?> saved)
    {
        foreach (var entry in saved)
        {
            var group = config.OptionGroups.FirstOrDefault(g =>
                g.Name.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));

            if (group == null)
                return false;

            if (group.Type == "radioGroup")
            {
                if (entry.Value is not string selected)
                    return false;

                if (group.Radios == null)
                    return false;

                var exists = group.Radios.Any(r =>
                    r.Value.Equals(selected, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    return false;
            }
            else if (group.Type == "checkboxGroup")
            {
                if (group.Checkboxes == null)
                    return false;

                if (entry.Value is IEnumerable<string> values && entry.Value is not string)
                {
                    foreach (var value in values)
                    {
                        var exists = group.Checkboxes.Any(c =>
                            c.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }
}
