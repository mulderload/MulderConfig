using System.Collections.Generic;
using MulderConfig.src.Logic;
using MulderConfig.src.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace MulderConfig.tests.Configuration;

public class ConfigJsonTests
{
    private static ConfigModel ParseConfig(string json)
    {
      return JsonConvert.DeserializeObject<ConfigModel>(json)!;
    }

    private static Dictionary<string, object> Sel(params (string key, object val)[] items)
    {
        var d = new Dictionary<string, object>();
        foreach (var (k, v) in items) d[k] = v;
        return d;
    }

    private static (string exe, string workDir, string args) ResolveLaunchLikeLaunchManager(ConfigModel config, Dictionary<string, object> selected)
    {
        // This mirrors the intended semantics:
        // - traverse actions.launch in JSON order
        // - args are cumulative (append)
        // - exec is atomic and last match wins
        var exe = config.Game.OriginalExe;
        var workDir = ".\\";
        var args = new List<string>();

        foreach (var rule in config.Actions.Launch)
        {
            if (!WhenResolver.Match(rule.When, selected))
                continue;

            if (rule.Exec != null)
            {
                exe = rule.Exec.Name;
                workDir = rule.Exec.WorkDir;
            }

            if (rule.Args != null)
                args.AddRange(rule.Args);
        }

        return (exe, workDir, string.Join(" ", args));
    }

    [Fact]
    public void PartialJson_DeserializesLaunchAndOperations()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test Game"", ""originalExe"": ""Game.exe"" },
          ""optionGroups"": [],
          ""actions"": {
            ""launch"": [
              {
                ""when"": [ { ""Renderer"": ""DX9"" } ],
                ""exec"": { ""name"": ""dx9.exe"", ""workDir"": "".\\"" },
                ""args"": [""-a""]
              }
            ],
            ""operations"": [
              {
                ""when"": [ { ""Renderer"": ""DX9"" } ],
                ""operation"": ""rename"",
                ""source"": ""a.dll"",
                ""target"": ""b.dll""
              },
              {
                ""operation"": ""replaceLine"",
                ""files"": [""FalloutPrefs.ini""],
                ""pattern"": ""^iSize W=.*$"",
                ""replacement"": ""iSize W=1920""
              }
            ]
          }
        }";

        var config = ParseConfig(json);

        Assert.Equal("Test Game", config.Game.Title);
        Assert.Equal("Game.exe", config.Game.OriginalExe);

        Assert.Single(config.Actions.Launch);
        Assert.NotNull(config.Actions.Launch[0].Exec);
        Assert.Equal("dx9.exe", config.Actions.Launch[0].Exec!.Name);
        Assert.Equal(@".\", config.Actions.Launch[0].Exec!.WorkDir);
        Assert.Equal(new[] { "-a" }, config.Actions.Launch[0].Args);

        Assert.Equal(2, config.Actions.Operations.Count);
        Assert.Equal("rename", config.Actions.Operations[0].Operation);
        Assert.Equal("a.dll", config.Actions.Operations[0].Source);
        Assert.Equal("b.dll", config.Actions.Operations[0].Target);

        Assert.Equal("replaceLine", config.Actions.Operations[1].Operation);
        Assert.Equal(new[] { "FalloutPrefs.ini" }, config.Actions.Operations[1].Files);
    }

    [Fact]
    public void LaunchRules_ArgsAppend_And_LastExecWins()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
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
        var selected = Sel(("Renderer", "DX9"));

        var (exe, workDir, args) = ResolveLaunchLikeLaunchManager(config, selected);

        Assert.Equal("dx9_alt.exe", exe);
        Assert.Equal(@"C:\tmp", workDir);
        Assert.Equal("-nosetup -novsync -borderless", args);
    }

    [Fact]
    public void NullWhen_IsTreatedAsAlwaysApply()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""optionGroups"": [],
          ""actions"": {
            ""launch"": [ { ""args"": [""-a""] } ],
            ""operations"": [ { ""operation"": ""removeLine"", ""files"": [""a.ini""], ""pattern"": ""^x=.*$"" } ]
          }
        }";

        var config = ParseConfig(json);
        var selected = Sel(("Renderer", "Anything"));

        Assert.True(WhenResolver.Match(config.Actions.Launch[0].When, selected));
        Assert.True(WhenResolver.Match(config.Actions.Operations[0].When, selected));
    }

    [Fact]
    public void MissingLaunchSection_DefaultsToEmpty_AndConfigIsValid()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""addons"": [ { ""title"": ""default"", ""steamId"": 1 } ],
          ""optionGroups"": [],
          ""actions"": {
            ""operations"": [ { ""operation"": ""delete"", ""source"": ""tmp.txt"" } ]
          }
        }";

        var config = ParseConfig(json);

        Assert.True(ConfigValidator.IsValid(config));
        Assert.NotNull(config.Actions.Launch);
        Assert.Empty(config.Actions.Launch);
        Assert.NotNull(config.Actions.Operations);
        Assert.Single(config.Actions.Operations);
    }

    [Fact]
    public void MissingOperationsSection_DefaultsToEmpty_AndConfigIsValid()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""addons"": [ { ""title"": ""default"", ""steamId"": 1 } ],
          ""optionGroups"": [],
          ""actions"": {
            ""launch"": [ { ""args"": [""-a""] } ]
          }
        }";

        var config = ParseConfig(json);

        Assert.True(ConfigValidator.IsValid(config));
        Assert.NotNull(config.Actions.Operations);
        Assert.Empty(config.Actions.Operations);
        Assert.NotNull(config.Actions.Launch);
        Assert.Single(config.Actions.Launch);
    }

    [Fact]
    public void NoActions_IsInvalid()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""addons"": [ { ""title"": ""default"", ""steamId"": 1 } ],
          ""optionGroups"": [],
          ""actions"": { }
        }";

        var config = ParseConfig(json);
        Assert.False(ConfigValidator.IsValid(config));
    }

    [Fact]
    public void EmptyLaunchAndOperations_IsInvalid()
    {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""addons"": [ { ""title"": ""default"", ""steamId"": 1 } ],
          ""optionGroups"": [],
          ""actions"": { ""launch"": [], ""operations"": [] }
        }";

        var config = ParseConfig(json);
        Assert.False(ConfigValidator.IsValid(config));
    }

      [Fact]
      public void MissingAddons_IsValid()
      {
        var json = @"
        {
          ""game"": { ""title"": ""Test"", ""originalExe"": ""Game.exe"" },
          ""optionGroups"": [],
          ""actions"": { ""launch"": [ { ""args"": [""-a""] } ] }
        }";

        var config = ParseConfig(json);
        Assert.True(ConfigValidator.IsValid(config));
      }
}
