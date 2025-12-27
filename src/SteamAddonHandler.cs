using MulderConfig.src.Configuration;

namespace MulderConfig.src;

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
        if (steamAddonId == null)
            return null;

        string? addonTitle = null;

        addonTitle = config.Addons.FirstOrDefault(a => a.SteamId == steamAddonId)?.Title;

        addonTitle ??= config.Addons.FirstOrDefault()?.Title;
        return addonTitle;
    }
}
