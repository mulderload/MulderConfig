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
        Apply(selectionProvider.GetTitle(), selectionProvider.GetChoices());
    }

    public void Apply(string title, IReadOnlyDictionary<string, object?> choices)
    {
        var selected = new Dictionary<string, object?>(choices, StringComparer.OrdinalIgnoreCase)
        {
            ["Title"] = title
        };

        var operations = config.Actions.Operations;
        if (operations != null && operations.Count > 0)
            FileOperationManager.ExecuteOperations(operations, selected);

        // If there is no launch section/rules, there is no exe replacement to perform.
        if ((config.Actions.Launch?.Count ?? 0) > 0 && !exeReplacer.IsReplaced())
            exeReplacer.Replace();
    }
}
