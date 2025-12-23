using MulderLauncher.Actions.Launch;
using MulderLauncher.Selections;

namespace MulderLauncher.Pipeline;

public sealed class GameRunner(ApplyManager applyManager, LaunchManager launchManager)
{
    public void Run(ISelectionProvider selectionProvider, bool persistSelections)
    {
        applyManager.Apply(selectionProvider, persistSelections);
        launchManager.Launch();
    }
}
