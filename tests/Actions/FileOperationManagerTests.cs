#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using MulderConfig.Actions;
using MulderConfig.Configuration;
using Xunit;

namespace MulderConfigTests.Actions;

public class FileOperationManagerTests
{
    [Fact]
    public void ExecuteOperations_Move_MovesDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "MulderConfigTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var sourceDir = Path.Combine(root, "srcDir");
        var targetDir = Path.Combine(root, "dstDir");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "a.txt"), "hello");

        try
        {
            var mgr = new FileOperationManager();
            mgr.ExecuteOperations(
                new List<OperationAction>
                {
                    new()
                    {
                        Operation = "move",
                        Source = sourceDir,
                        Target = targetDir
                    }
                },
                selected: new Dictionary<string, object?>());

            Assert.False(Directory.Exists(sourceDir));
            Assert.True(Directory.Exists(targetDir));
            Assert.True(File.Exists(Path.Combine(targetDir, "a.txt")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
