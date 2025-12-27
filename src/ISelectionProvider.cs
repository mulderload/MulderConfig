namespace MulderConfig.src;

public interface ISelectionProvider
{
    string GetAddon();
    Dictionary<string, object?> GetChoices();
}
