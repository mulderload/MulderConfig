using System;
using System.Linq;
using MulderLauncher.Actions.Launch;
using MulderLauncher.Actions.Operations;
using MulderLauncher.Config;
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
            var exeWrapper = new ExeWrapper(configProvider);

            // Wrapper mode must be headless (no UI initialization).
            if (exeWrapper.IsWrapped())
            {
                RunHeadlessWrapperMode(configProvider, exeWrapper, steamAddonId);
                return;
            }

            ApplicationConfiguration.Initialize();

            var formStateManager = new FormStateManager(configProvider);
            var formValidator = new FormValidator(configProvider, formStateManager);
            var formBuilder = new FormBuilder(formValidator, formStateManager);
            var fileActionManager = new FileActionManager();
            var saveManager = new SaveManager(formStateManager);
            var launchManager = new LaunchManager(configProvider, formStateManager, exeWrapper);

            Application.Run(new Form1(
                steamAddonId,
                configProvider,
                exeWrapper,
                fileActionManager,
                formBuilder,
                formValidator,
                formStateManager,
                launchManager,
                saveManager));
        }

        private static void RunHeadlessWrapperMode(ConfigProvider configProvider, ExeWrapper exeWrapper, int? steamAddonId)
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
            var selected = selectionProvider.GetChoices();
            selected["Addon"] = selectionProvider.GetAddon();

            var fileActionManager = new FileActionManager();
            fileActionManager.ExecuteOperations(config.Actions.Operations, selected);

            var launchManager = new LaunchManager(configProvider, selectionProvider, exeWrapper);
            launchManager.Launch();
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