using MulderConfig.Actions;
using MulderConfig.Configuration;
using MulderConfig.Save;

namespace MulderConfig.Apply;

public sealed class ApplyManager(
    ConfigModel config,
    ExeReplacer exeReplacer,
    FileOperationManager FileOperationManager)
{
    public void Apply(ISelectionProvider selectionProvider)
    {
        var selected = selectionProvider.GetChoices();
        selected["Title"] = selectionProvider.GetTitle();

        var operations = config.Actions.Operations;
        if (operations != null && operations.Count > 0)
            FileOperationManager.ExecuteOperations(operations, selected);

        // If there is no launch section/rules, there is no exe replacement to perform.
        if ((config.Actions.Launch?.Count ?? 0) > 0 && !exeReplacer.IsReplaced())
            exeReplacer.Replace();
    }
}
