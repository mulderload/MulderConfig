using MulderConfig.Configuration;

namespace MulderConfig;

public sealed class SteamAddonHandler(ConfigModel config, string[] args)
{
    public int? ResolveAddonId()
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("-addon", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(args[i + 1], out int addonId))
            {
                return addonId;
            }
        }

        return null;
    }

    public string? ResolveAddonTitle(int? steamAddonId)
    {
        if (steamAddonId is null)
            return null;

        var addons = config.Addons;
        if (addons is null || addons.Count == 0)
            return null;

        return addons.FirstOrDefault(a => a.SteamId == steamAddonId)?.Title
            ?? addons[0].Title;
    }
}
