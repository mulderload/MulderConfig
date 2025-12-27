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
        var saveLoader = new SaveLoader();
        var saveSaver = new SaveSaver(saveLoader);
        var steamAddonHandler = new SteamAddonHandler(config, args);
        var applyManager = new ApplyManager(config, exeReplacer, fileOperationManager);

        // Handle Steam Addons
        var steamAddonId = steamAddonHandler.ResolveAddonId();
        var addonTitle = steamAddonHandler.ResolveAddonTitle(steamAddonId) ?? "_";

        // Select current addon save
        saveLoader.LoadAll();
        saveLoader.Load(addonTitle);

        // Headless modes
        if (modeDetector.IsApplyMode())
        {
            RunHeadlessApplyMode(addonTitle, applyManager, saveLoader);
            return;
        }
        else if (modeDetector.IsLaunchMode())
        {
            RunHeadlessLaunchMode(config, exeReplacer, addonTitle, saveLoader);
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
            saveLoader,
            saveSaver));
    }

    private static void RunHeadlessApplyMode(string addonTitle, ApplyManager applyManager, SaveLoader saveLoader)
    {
        var selectionProvider = new SavedSelectionProvider(saveLoader, addonTitle);
        applyManager.Apply(selectionProvider);
    }

    private static void RunHeadlessLaunchMode(ConfigModel config, ExeReplacer exeReplacer, string addonTitle, SaveLoader saveLoader)
    {
        var selectionProvider = new SavedSelectionProvider(saveLoader, addonTitle);
        var launchManager = new LaunchManager(config, selectionProvider, exeReplacer);
        launchManager.Launch();
    }
}