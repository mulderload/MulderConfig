using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MulderLauncher.Actions.When;
using MulderLauncher.Config;

namespace MulderLauncher.Actions.Operations
{
    public class FileActionManager
    {
        public void ExecuteOperations(List<OperationAction> operations, IReadOnlyDictionary<string, object?> selected)
        {
            foreach (var action in operations)
            {
                if (action.When != null && !WhenResolver.Match(action.When, selected))
                    continue;

                try
                {
                    switch ((action.Operation ?? string.Empty).ToLower())
                    {
                        case "rename":
                        case "move":
                            ExecuteMove(action);
                            break;

                        case "copy":
                            ExecuteCopy(action);
                            break;

                        case "delete":
                            ExecuteDelete(action);
                            break;

                        case "replaceline":
                            ExecuteReplaceLine(action);
                            break;

                        case "removeline":
                            ExecuteRemoveLine(action);
                            break;

                        case "replacetext":
                            ExecuteReplaceText(action);
                            break;

                        default:
                            MessageBox.Show($"Unknown operation: {action.Operation}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Operation failed: {action.Operation}\n{ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private static void ExecuteMove(OperationAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Source) || string.IsNullOrWhiteSpace(action.Target))
                throw new InvalidOperationException("Missing 'source' or 'target' for rename/move.");

            var sourcePath = Path.Combine(Application.StartupPath, action.Source);
            var targetPath = Path.Combine(Application.StartupPath, action.Target);

            if (!File.Exists(sourcePath))
                return;

            if (File.Exists(targetPath))
                File.Delete(targetPath);

            File.Move(sourcePath, targetPath);
        }

        private static void ExecuteCopy(OperationAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Source) || string.IsNullOrWhiteSpace(action.Target))
                throw new InvalidOperationException("Missing 'source' or 'target' for copy.");

            var sourcePath = Path.Combine(Application.StartupPath, action.Source);
            var targetPath = Path.Combine(Application.StartupPath, action.Target);

            if (!File.Exists(sourcePath))
                return;

            File.Copy(sourcePath, targetPath, overwrite: true);
        }

        private static void ExecuteDelete(OperationAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Source))
                throw new InvalidOperationException("Missing 'source' for delete.");

            var sourcePath = Path.Combine(Application.StartupPath, action.Source);
            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
        }

        private void ExecuteReplaceLine(OperationAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Pattern) || action.Replacement == null)
                throw new InvalidOperationException("Missing 'pattern' or 'replacement' for replaceLine.");

            foreach (var filePath in ResolveFiles(action.Files))
                ReplaceLineInFile(filePath, action.Pattern, action.Replacement);
        }

        private void ExecuteRemoveLine(OperationAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Pattern))
                throw new InvalidOperationException("Missing 'pattern' for removeLine.");

            foreach (var filePath in ResolveFiles(action.Files))
                RemoveLineInFile(filePath, action.Pattern);
        }

        private void ExecuteReplaceText(OperationAction action)
        {
            if (action.Search == null || action.Replacement == null)
                throw new InvalidOperationException("Missing 'search' or 'replacement' for replaceText.");

            foreach (var filePath in ResolveFiles(action.Files))
                ReplaceTextInFile(filePath, action.Search, action.Replacement);
        }

        private static IEnumerable<string> ResolveFiles(List<string>? files)
        {
            if (files == null || files.Count == 0)
                yield break;

            foreach (var f in files)
            {
                if (string.IsNullOrWhiteSpace(f))
                    continue;

                var filePath = Path.Combine(Application.StartupPath, f);
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"File not found: {f}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                yield return filePath;
            }
        }

        private void ReplaceLineInFile(string filePath, string pattern, string replacement)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            var interpretedReplacement = Regex.Unescape(replacement);
            var replacementLines = interpretedReplacement
                .Split('\n')
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();

            bool modified = false;
            var newLines = new List<string>();

            for (int i = 0; i < lines.Count; i++)
            {
                if (regex.IsMatch(lines[i]))
                {
                    modified = true;
                    if (replacementLines.Count > 0)
                        newLines.AddRange(replacementLines);
                }
                else
                {
                    newLines.Add(lines[i]);
                }
            }

            if (modified)
                File.WriteAllLines(filePath, newLines.ToArray());
        }

        private void RemoveLineInFile(string filePath, string pattern)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            var newLines = lines.Where(line => !regex.IsMatch(line)).ToList();

            if (newLines.Count < lines.Count)
                File.WriteAllLines(filePath, newLines.ToArray());
        }

        private void ReplaceTextInFile(string filePath, string search, string replacement)
        {
            var content = File.ReadAllText(filePath);
            var newContent = content.Replace(search, replacement);

            if (content != newContent)
                File.WriteAllText(filePath, newContent);
        }
    }
}
