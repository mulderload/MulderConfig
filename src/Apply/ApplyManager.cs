using MulderConfig.src.Actions;
using MulderConfig.src.Configuration;
using MulderConfig.src.Save;

namespace MulderConfig.src.Apply;

public sealed class ApplyManager(
    ConfigModel config,
    SaveManager saveManager,
    FileOperationManager FileOperationManager,
    ExeReplacer exeReplacer,
    ModeDetector modeDetector)
{
    public void Apply(ISelectionProvider selectionProvider, bool persistSelections)
    {
        if (persistSelections)
        {
            saveManager.SaveChoices(selectionProvider.GetAddon(), selectionProvider.GetChoices());
        }

        var selected = selectionProvider.GetChoices();
        selected["Addon"] = selectionProvider.GetAddon();
        FileOperationManager.ExecuteOperations(config.Actions.Operations, selected);

        if (config.Actions.Launch is { Count: > 0 } && !modeDetector.IsWrapMode())
        {
            exeReplacer.EnsureReplaced();
        }
    }
}
