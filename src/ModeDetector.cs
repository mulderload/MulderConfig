using MulderConfig.src.Configuration;
using System.Diagnostics;

namespace MulderConfig.src;

public sealed class ModeDetector(ConfigModel config, string[] args)
{
    public bool IsLaunchMode()
    {
        if (args.Any(a => a.Equals("-launch", StringComparison.OrdinalIgnoreCase))) {
            return true; // useful for local test
        }

        var originalExeName = Path.GetFileName(config.Game.OriginalExe);
        var processExeName = Process.GetCurrentProcess().ProcessName + ".exe";

        return originalExeName.Equals(processExeName, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsApplyMode()
    {
        return args.Any(a => a.Equals("-apply", StringComparison.OrdinalIgnoreCase));
    }

    public bool IsNormalMode()
    {
        return !IsLaunchMode() && !IsApplyMode();
    }
}
