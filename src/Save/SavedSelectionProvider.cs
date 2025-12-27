namespace MulderConfig.src.Save;

public class SavedSelectionProvider(SaveLoader saveLoader, string addon) : ISelectionProvider
{
    public string GetAddon()
    {
        return addon;
    }

    public Dictionary<string, object?> GetChoices()
    {
        return saveLoader.Load(addon);
    } 
}
