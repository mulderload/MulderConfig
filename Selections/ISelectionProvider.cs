using System.Collections.Generic;

namespace MulderLauncher.Selections
{
    public interface ISelectionProvider
    {
        string GetAddon();
        Dictionary<string, object?> GetChoices();
    }
}
