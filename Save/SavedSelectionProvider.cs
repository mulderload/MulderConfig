namespace MulderConfig.Save;

public class SavedSelectionProvider(SaveLoader saveLoader, string title) : ISelectionProvider
{
    public string GetTitle()
    {
        return title;
    }

    public Dictionary<string, object?> GetChoices()
    {
        return saveLoader.Load(title);
    } 
}
