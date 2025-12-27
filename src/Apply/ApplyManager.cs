using MulderConfig.src.Actions;
using MulderConfig.src.Configuration;
using MulderConfig.src.Save;

namespace MulderConfig.src.Apply;

public sealed class ApplyManager(
    ConfigModel config,
    ExeReplacer exeReplacer,
    FileOperationManager FileOperationManager)
{
    public void Apply(ISelectionProvider selectionProvider)
    {
        var selected = selectionProvider.GetChoices();
        selected["Addon"] = selectionProvider.GetTitle();

        FileOperationManager.ExecuteOperations(config.Actions.Operations, selected);

        if (!exeReplacer.IsReplaced())
        {
            exeReplacer.Replace();
        }
    }
}
