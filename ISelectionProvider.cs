namespace MulderConfig;

public interface ISelectionProvider
{
    string GetTitle(); // GameTitle or AddonTitle
    Dictionary<string, object?> GetChoices();
}
