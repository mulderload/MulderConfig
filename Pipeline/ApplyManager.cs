using MulderLauncher.Actions.Operations;
using MulderLauncher.Config;
using MulderLauncher.Replacement;
using MulderLauncher.Save;
using MulderLauncher.Selections;

namespace MulderLauncher.Pipeline;

public sealed class ApplyManager(
    ConfigProvider configProvider,
    SaveManager saveManager,
    FileActionManager fileActionManager,
    ExeReplacer exeReplacer)
{
    public void Apply(ISelectionProvider selectionProvider, bool persistSelections)
    {
        var config = configProvider.GetConfig();

        if (persistSelections)
        {
            saveManager.SaveChoices(selectionProvider.GetAddon(), selectionProvider.GetChoices());
        }

        var selected = selectionProvider.GetChoices();
        selected["Addon"] = selectionProvider.GetAddon();
        fileActionManager.ExecuteOperations(config.Actions.Operations, selected);

        if (config.Actions.Launch is { Count: > 0 } && !exeReplacer.IsReplacing())
        {
            exeReplacer.EnsureReplaced();
        }
    }
}
