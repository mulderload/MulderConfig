using MulderConfig.Configuration;

namespace MulderConfig.Apply;

public class ExeReplacer(ConfigModel config)
{
    private const string LAUNCHER_NAME = "MulderConfig";

    private (string originalExe, string targetExe) GetExePaths()
    {
        var originalExe = Path.Combine(Application.StartupPath, config.Game.OriginalExe);
        var targetExeName = Path.GetFileNameWithoutExtension(config.Game.OriginalExe) + "_o" + Path.GetExtension(config.Game.OriginalExe);
        var targetExe = Path.Combine(Application.StartupPath, targetExeName);

        return (originalExe, targetExe);
    }

    public (string originalExe, string targetExe) GetExePathsPublic() => GetExePaths();

    public string GetDefaultLaunchExePath()
    {
        var (originalExe, targetExe) = GetExePaths();
        return IsReplaced() && File.Exists(targetExe) ? targetExe : originalExe;
    }

    private string GetLauncherPath()
    {
        return Path.Combine(Application.StartupPath, $"{LAUNCHER_NAME}.exe");
    }

    private bool FilesEquals(string path1, string path2)
    {
        var fileInfo1 = new FileInfo(path1);
        var fileInfo2 = new FileInfo(path2);

        if (fileInfo1.Length != fileInfo2.Length)
            return false;

        // TODO compare checksums

        return true;
    }

    public bool IsReplaced()
    {
        var (originalExe, targetExe) = GetExePaths();

        if (!File.Exists(originalExe) || !File.Exists(targetExe))
            return false;

        var launcherExe = GetLauncherPath();
        return FilesEquals(originalExe, launcherExe);
    }

    public bool CanReplace()
    {
        var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        if (!processName.Equals(LAUNCHER_NAME, StringComparison.OrdinalIgnoreCase))
            return false;

        var (originalExe, _) = GetExePaths();
        return File.Exists(originalExe);
    }

    public void Replace()
    {
        if (!CanReplace())
            return;

        var (originalExe, targetExe) = GetExePaths();
        File.Move(originalExe, targetExe, true);

        try
        {
            File.Copy(GetLauncherPath(), originalExe, true);
            MessageBox.Show("Replacement done.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Warning: Replacement partially failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
