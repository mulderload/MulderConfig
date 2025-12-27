namespace MulderConfig.src.Save;

public class SavedSelectionProvider : ISelectionProvider
{
    private readonly string addon;
    private readonly SaveLoader saveLoader;

    public SavedSelectionProvider(SaveLoader saveLoader, string addon)
    {
        this.addon = addon;
        this.saveLoader = saveLoader;
    }

    public string GetAddon() => addon;

    public Dictionary<string, object?> GetChoices()
        => saveLoader.Load(addon);
}
