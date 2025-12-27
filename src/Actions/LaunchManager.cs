using System.Diagnostics;
using MulderConfig.src.Configuration;
using MulderConfig.src.Logic;
using MulderConfig.src.Apply;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MulderConfigTests")]

namespace MulderConfig.src.Actions;

public class LaunchManager(ConfigModel config, ISelectionProvider selectionProvider)
{
    public void Launch()
    {
        var (exePath, workDir, args) = ResolveLaunch();

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException("Can't find executable.", exePath);
        }

        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workDir,
                Arguments = args,
                UseShellExecute = false
            }
        };
        process.Start();
    }

    internal (string exePath, string workDir, string args) ResolveLaunch()
    {
        var selected = selectionProvider.GetChoices();
        selected["Addon"] = selectionProvider.GetAddon();

        // Defaults
        var exePath = MakePath(config.Game.OriginalExe);
        var workDir = Application.StartupPath;
        var args = new List<string>();

        foreach (var rule in config.Actions.Launch)
        {
            if (rule.When != null && !WhenResolver.Match(rule.When, selected))
                continue;

            // Atomic override: last match wins
            if (rule.Exec != null)
            {
                exePath = MakePath(rule.Exec.Name);
                workDir = MakePath(rule.Exec.WorkDir);
            }

            // Cumulative: append all matching args
            if (rule.Args != null)
            {
                foreach (var a in rule.Args)
                {
                    if (!string.IsNullOrWhiteSpace(a))
                        args.Add(a);
                }
            }
        }

        return (exePath, workDir, string.Join(" ", args));
    }

    private static string MakePath(string path)
    {
        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(Application.StartupPath, path));
    }
}
