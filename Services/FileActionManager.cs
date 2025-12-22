using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MulderLauncher.Models;

namespace MulderLauncher.Services
{
    public class FileActionManager
    {
        public void ExecuteFileOperations(List<FileOperationAction> operations, Dictionary<string, object> selected)
        {
            foreach (var action in operations)
            {
                if (!WhenResolver.Match(action.When, selected))
                    continue;

                var sourcePath = Path.Combine(Application.StartupPath, action.Source);
                var targetPath = action.Target != null ? Path.Combine(Application.StartupPath, action.Target) : null;

                try
                {
                    switch (action.Operation.ToLower())
                    {
                        case "rename":
                        case "move":
                            if (targetPath != null && File.Exists(sourcePath))
                            {
                                // Supprimer la cible si elle existe déjà
                                if (File.Exists(targetPath))
                                    File.Delete(targetPath);
                                File.Move(sourcePath, targetPath);
                            }
                            break;

                        case "copy":
                            if (targetPath != null && File.Exists(sourcePath))
                                File.Copy(sourcePath, targetPath, overwrite: true);
                            break;

                        case "delete":
                            if (File.Exists(sourcePath))
                                File.Delete(sourcePath);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"File operation failed: {action.Operation} {action.Source}\n{ex.Message}", 
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void ExecuteFileEdits(List<FileEditAction> edits, Dictionary<string, object> selected)
        {
            foreach (var edit in edits)
            {
                if (!WhenResolver.Match(edit.When, selected))
                    continue;

                var filePath = Path.Combine(Application.StartupPath, edit.File);

                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"File not found: {edit.File}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                try
                {
                    switch (edit.Operation.ToLower())
                    {
                        case "replaceline":
                            ReplaceLineInFile(filePath, edit.Pattern!, edit.Replacement!);
                            break;

                        case "removeline":
                            RemoveLineInFile(filePath, edit.Pattern!);
                            break;

                        case "replacetext":
                            ReplaceTextInFile(filePath, edit.Search!, edit.Replacement!);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"File edit failed: {edit.Operation} in {edit.File}\n{ex.Message}", 
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
                    // si replacement vide, on supprime la ligne
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
            {
                File.WriteAllLines(filePath, newLines.ToArray());
            }
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