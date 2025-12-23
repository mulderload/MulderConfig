using System;
using System.Linq;
using MulderLauncher.Pipeline;
using MulderLauncher.Actions.Launch;
using MulderLauncher.Actions.Operations;
using MulderLauncher.Config;
using MulderLauncher.Replacement;
using MulderLauncher.Save;
using MulderLauncher.UI;

namespace MulderLauncher
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var steamAddonId = ParseSteamAddonId(args);

            var configProvider = new ConfigProvider();
            var exeReplacer = new ExeReplacer(configProvider);
            var saveManager = new SaveManager();
            var fileActionManager = new FileActionManager();
            var applyManager = new ApplyManager(configProvider, saveManager, fileActionManager, exeReplacer);

            // Wrapper mode must be headless (no UI initialization).
            if (exeReplacer.IsReplacing())
            {
                RunHeadlessWrapperMode(configProvider, exeReplacer, steamAddonId, applyManager);
                return;
            }

            ApplicationConfiguration.Initialize();

            var formSelectionProvider = new FormSelectionProvider(configProvider);
            var formValidator = new FormValidator(configProvider, formSelectionProvider);
            var formBuilder = new FormBuilder(formValidator, formSelectionProvider);

            Application.Run(new Form1(
                steamAddonId,
                configProvider,
                applyManager,
                formBuilder,
                formValidator,
                formSelectionProvider,
                saveManager));
        }

        private static void RunHeadlessWrapperMode(ConfigProvider configProvider, ExeReplacer exeReplacer, int? steamAddonId, ApplyManager applyManager)
        {
            var config = configProvider.GetConfig();

            string? addonTitle = null;
            if (steamAddonId != null)
            {
                addonTitle = config.Addons.FirstOrDefault(a => a.SteamId == steamAddonId)?.Title;
            }

            addonTitle ??= config.Addons.FirstOrDefault()?.Title;
            if (addonTitle == null)
            {
                return;
            }

            var selectionProvider = new SavedSelectionProvider(addonTitle);
            var launchManager = new LaunchManager(configProvider, selectionProvider, exeReplacer);
            var runner = new GameRunner(applyManager, launchManager);
            runner.Run(selectionProvider, persistSelections: false);
        }

        private static int? ParseSteamAddonId(string[] args)
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
    }
}