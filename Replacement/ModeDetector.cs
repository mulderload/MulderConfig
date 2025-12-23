using MulderLauncher.Config;
using System.Diagnostics;

namespace MulderLauncher.Replacement;

public sealed class ModeDetector(ConfigProvider configProvider)
{
    public bool IsWrapping()
    {
        var originalExeName = Path.GetFileName(configProvider.GetConfig().Game.OriginalExe);
        var processExeName = Process.GetCurrentProcess().ProcessName + ".exe";

        return originalExeName.Equals(processExeName, StringComparison.OrdinalIgnoreCase);
    }
}
