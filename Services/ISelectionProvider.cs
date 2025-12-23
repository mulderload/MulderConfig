using System.Collections.Generic;

namespace MulderLauncher.Services
{
    public interface ISelectionProvider
    {
        string GetAddon();
        Dictionary<string, object?> GetChoices();
    }
}
