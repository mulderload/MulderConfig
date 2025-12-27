namespace MulderConfig.src.Configuration;

public class ConfigValidator
{
    public static bool IsValid(ConfigModel config)
    {
        if (config == null)
            return false;

        if (config.Game == null)
            return false;

        if (string.IsNullOrWhiteSpace(config.Game.Name))
            return false;

        if (string.IsNullOrWhiteSpace(config.Game.OriginalExe))
            return false;

        if (config.Addons == null || config.Addons.Count == 0)
            return false;

        if (config.Addons.Any(a => a == null || string.IsNullOrWhiteSpace(a.Title)))
            return false;

        if (config.OptionGroups == null)
            return false;

        if (config.Actions == null)
            return false;

        if (config.Actions.Launch == null)
            return false;

        if (config.Actions.Operations == null)
            return false;

        var groupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in config.OptionGroups)
        {
            if (group == null)
                return false;

            if (string.IsNullOrWhiteSpace(group.Name))
                return false;

            if (!groupNames.Add(group.Name))
                return false;

            if (group.Type != "radioGroup" && group.Type != "checkboxGroup")
                return false;

            if (group.Type == "radioGroup")
            {
                if (group.Radios == null || group.Radios.Count == 0)
                    return false;

                var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in group.Radios)
                {
                    if (r == null || string.IsNullOrWhiteSpace(r.Value))
                        return false;

                    if (!values.Add(r.Value))
                        return false;
                }
            }

            if (group.Type == "checkboxGroup")
            {
                if (group.Checkboxes == null || group.Checkboxes.Count == 0)
                    return false;

                var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in group.Checkboxes)
                {
                    if (c == null || string.IsNullOrWhiteSpace(c.Value))
                        return false;

                    if (!values.Add(c.Value))
                        return false;
                }
            }
        }

        foreach (var rule in config.Actions.Launch)
        {
            if (rule == null)
                return false;

            if (rule.Exec != null)
            {
                if (string.IsNullOrWhiteSpace(rule.Exec.Name))
                    return false;

                if (string.IsNullOrWhiteSpace(rule.Exec.WorkDir))
                    return false;
            }

            if (rule.Args != null && rule.Args.Any(a => a == null))
                return false;
        }

        foreach (var op in config.Actions.Operations)
        {
            if (op == null)
                return false;

            if (string.IsNullOrWhiteSpace(op.Operation))
                return false;

            var operation = op.Operation.Trim().ToLowerInvariant();

            if (operation is "rename" or "move" or "copy")
            {
                if (string.IsNullOrWhiteSpace(op.Source) || string.IsNullOrWhiteSpace(op.Target))
                    return false;
            }

            if (operation is "delete")
            {
                if (string.IsNullOrWhiteSpace(op.Source))
                    return false;
            }

            if (operation is "replaceline" or "removeline")
            {
                if (op.Files == null || op.Files.Count == 0)
                    return false;

                if (string.IsNullOrWhiteSpace(op.Pattern))
                    return false;

                if (operation == "replaceline" && op.Replacement == null)
                    return false;
            }

            if (operation is "replacetext")
            {
                if (op.Files == null || op.Files.Count == 0)
                    return false;

                if (string.IsNullOrWhiteSpace(op.Search))
                    return false;

                if (op.Replacement == null)
                    return false;
            }
        }

        return true;
    }
}
