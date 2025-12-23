using MulderLauncher.Config;

namespace MulderLauncher.Replacement
{
    public class ExeReplacer(ConfigProvider configProvider)
    {
        private const string LAUNCHER_NAME = "MulderLauncher";

        private (string originalExe, string targetExe) GetExePaths()
        {
            var config = configProvider.GetConfig();
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

        public bool IsReplacing()
        {
            if (!IsReplaced())
                return false;

            var originalExeName = configProvider.GetConfig().Game.OriginalExe.ToLowerInvariant();
            var processExeName = (System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe").ToLowerInvariant();

            return originalExeName.Equals(processExeName);
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

        public void EnsureReplaced()
        {
            if (IsReplaced())
                return;

            Replace();
        }
    }
}
