using MulderConfig.src.Actions;
using MulderConfig.src.Configuration;
using MulderConfig.src.Save;
using MulderConfig.src.Apply;
using MulderConfig.src.UI;
using MulderConfig.src;

namespace MulderConfig;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Read and validate config
        ConfigModel config;
        try
        {
            config = ConfigProvider.GetConfig();
        } 
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading configuration:\n{ex.Message}");
            return;
        }
        if (!ConfigValidator.IsValid(config))
        {
            MessageBox.Show("Error loading configuration:\nThe file 'MulderConfig.json' has invalid data.");
            return;
        }

        // Read and validate saves

        // Initialize core components
        var exeReplacer = new ExeReplacer(config);
        var fileOperationManager = new FileOperationManager();
        var modeDetector = new ModeDetector(config, args);
        var saveManager = new SaveManager();
        var steamAddonHandler = new SteamAddonHandler(config, args);
        var applyManager = new ApplyManager(config, saveManager, fileOperationManager, exeReplacer, modeDetector);

        // Handle Steam Addons
        var steamAddonId = steamAddonHandler.ResolveAddonId();
        var gameTitle = steamAddonHandler.ResolveAddonTitle(steamAddonId) ?? "_";

        // Select current addon save

        // Headless modes
        if (modeDetector.IsApplyMode())
        {
            RunHeadlessApplyMode(config, steamAddonId, applyManager);
            return;
        }
        else if (modeDetector.IsLaunchMode())
        {
            RunHeadlessLaunchMode(config, exeReplacer, steamAddonId, applyManager);
            return;
        }

        // Initialize UI components
        var formSelectionProvider = new FormSelectionProvider(config);
        var formValidator = new FormValidator(config, formSelectionProvider);
        var formBuilder = new FormBuilder(formValidator, formSelectionProvider);

        // Normal UI mode
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1(
            steamAddonId,
            config,
            applyManager,
            formBuilder,
            formValidator,
            formSelectionProvider,
            saveManager));
    }

    private static void RunHeadlessApplyMode(ConfigModel config, int? steamAddonId, ApplyManager applyManager)
    {
        string? addonTitle = null;
        if (steamAddonId != null)
            addonTitle = config.Addons.FirstOrDefault(a => a.SteamId == steamAddonId)?.Title;

        addonTitle ??= config.Addons.FirstOrDefault()?.Title;
        if (addonTitle == null)
            return;

        var selectionProvider = new SavedSelectionProvider(addonTitle);

        // Apply only: reads save, runs operations + replacement; no saving.
        applyManager.Apply(selectionProvider, persistSelections: false);
    }

    private static void RunHeadlessLaunchMode(ConfigModel config, ExeReplacer exeReplacer, int? steamAddonId, ApplyManager applyManager)
    {
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
        var launchManager = new LaunchManager(config, selectionProvider, exeReplacer);
        launchManager.Launch();
    }
}