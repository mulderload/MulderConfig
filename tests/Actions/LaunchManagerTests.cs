#nullable enable

using System;
using System.Collections.Generic;
using MulderConfig.src;
using MulderConfig.src.Actions;
using MulderConfig.src.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace MulderConfig.tests.Actions;

public class LaunchManagerTests
{
    private sealed class TestSelectionProvider(string addon, Dictionary<string, object?> choices) : ISelectionProvider
    {
        public string GetAddon() => addon;
        public Dictionary<string, object?> GetChoices() => choices;
    }

    private static ConfigModel ParseConfig(string json)
        => JsonConvert.DeserializeObject<ConfigModel>(json)!;

    [Fact]
    public void ResolveLaunch_ReturnsDefaults_WhenNoRuleMatches()
    {
        var json = @"
        {
          ""game"": { ""name"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""optionGroups"": [],
          ""actions"": {
            ""launch"": [
              {
                ""when"": [ { ""Renderer"": ""DX9"" } ],
                ""exec"": { ""name"": ""dx9.exe"", ""workDir"": "".\\"" },
                ""args"": [""-a""]
              }
            ],
            ""operations"": []
          }
        }";

        var config = ParseConfig(json);

        var selectionProvider = new TestSelectionProvider(
            addon: "default",
            choices: new Dictionary<string, object?> { ["Renderer"] = "DX11" });

        var manager = new LaunchManager(config, selectionProvider);

        var (exePath, workDir, args) = manager.ResolveLaunch();

        Assert.Equal(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Game.exe"), exePath);
        Assert.Equal(System.Windows.Forms.Application.StartupPath, workDir);
        Assert.Equal(string.Empty, args);
    }

    [Fact]
    public void ResolveLaunch_AppendsArgs_And_LastExecWins()
    {
        var json = @"
        {
          ""game"": { ""name"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""optionGroups"": [],
          ""actions"": {
            ""launch"": [
              {
                ""when"": [ { ""Renderer"": ""DX9"" } ],
                ""exec"": { ""name"": ""dx9.exe"", ""workDir"": "".\\"" },
                ""args"": [""-nosetup""]
              },
              {
                ""when"": [ { ""Renderer"": ""DX9"" } ],
                ""exec"": { ""name"": ""dx9_alt.exe"", ""workDir"": ""C:\\tmp"" },
                ""args"": [""-novsync"", ""-borderless""]
              }
            ],
            ""operations"": []
          }
        }";

        var config = ParseConfig(json);

        var selectionProvider = new TestSelectionProvider(
            addon: "default",
            choices: new Dictionary<string, object?> { ["Renderer"] = "DX9" });

        var manager = new LaunchManager(config, selectionProvider);

        var (exePath, workDir, args) = manager.ResolveLaunch();

        var expectedExePath = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "dx9_alt.exe"));

        Assert.Equal(expectedExePath, exePath);
        Assert.Equal(System.IO.Path.GetFullPath("C:\\tmp"), workDir);
        Assert.Equal("-nosetup -novsync -borderless", args);
    }
}
