namespace MulderLauncher.Services
{
    public class ExeWrapper(ConfigProvider configProvider)
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
            return IsWrapped() && File.Exists(targetExe) ? targetExe : originalExe;
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

        public bool IsWrapped()
        {
            var (originalExe, targetExe) = GetExePaths();

            if (!File.Exists(originalExe) || !File.Exists(targetExe))
                return false;

            var launcherExe = GetLauncherPath();

            return FilesEquals(originalExe, launcherExe);
        }

        public bool IsWrapping()
        {
            if (!IsWrapped())
                return false;

            var originalExeName = configProvider.GetConfig().Game.OriginalExe.ToLower();
            var processExeName = (System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe").ToLower();

            return originalExeName.Equals(processExeName);
        }

        public bool CanWrap()
        {
            var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            if (processName != LAUNCHER_NAME)
                return false;

            var (originalExe, _) = GetExePaths();

            return (File.Exists(originalExe));
        }

        public void Wrap()
        {
            if (!CanWrap())
                return;

            var (originalExe, targetExe) = GetExePaths();
            File.Move(originalExe, targetExe, true);

            try
            {
                File.Copy(Path.Combine(Application.StartupPath, $"{LAUNCHER_NAME}.exe"), originalExe, true);
                MessageBox.Show("Wrapping done.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning: Wrapping partially failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
