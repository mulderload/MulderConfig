using System.Collections.Generic;
using MulderConfig.src.Configuration;
using Xunit;

namespace MulderConfig.tests.Configuration;

public class ConfigValidatorTests
{
    private static ConfigModel MinimalValidConfig()
    {
        return new ConfigModel
        {
            Game = new Game { Title = "Test", OriginalExe = "Game.exe" },
            Addons = new List<Addon> { new() { Title = "default" } },
            OptionGroups = new List<OptionGroup>
            {
                new()
                {
                    Name = "Renderer",
                    Type = "radioGroup",
                    Radios = new List<Radio> { new() { Value = "DX9" } }
                }
            },
            Actions = new ActionRoot
            {
                Launch = new List<LaunchAction>
                {
                    new()
                    {
                        Exec = new ExecSpec { Name = "Game.exe", WorkDir = ".\\" },
                        Args = new List<string> { "-a" }
                    }
                },
                Operations = new List<OperationAction>
                {
                    new()
                    {
                        Operation = "delete",
                        Source = "tmp.txt"
                    }
                }
            }
        };
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForMinimalValidConfig()
    {
        Assert.True(ConfigValidator.IsValid(MinimalValidConfig()));
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenAddonListMissing()
    {
        var cfg = MinimalValidConfig();
        cfg.Addons = null;
        Assert.True(ConfigValidator.IsValid(cfg));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForUnknownGroupType()
    {
        var cfg = MinimalValidConfig();
        cfg.OptionGroups[0].Type = "dropdown";
        Assert.False(ConfigValidator.IsValid(cfg));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenRadioGroupHasNoRadios()
    {
        var cfg = MinimalValidConfig();
        cfg.OptionGroups[0].Radios = new List<Radio>();
        Assert.False(ConfigValidator.IsValid(cfg));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenLaunchExecMissingFields()
    {
        var cfg = MinimalValidConfig();
        cfg.Actions.Launch[0].Exec = new ExecSpec { Name = "", WorkDir = ".\\" };
        Assert.False(ConfigValidator.IsValid(cfg));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenOperationMissingRequiredFields()
    {
        var cfg = MinimalValidConfig();
        cfg.Actions.Operations = new List<OperationAction>
        {
            new()
            {
                Operation = "rename",
                Source = "a.dll",
                Target = null
            }
        };

        Assert.False(ConfigValidator.IsValid(cfg));
    }

}
