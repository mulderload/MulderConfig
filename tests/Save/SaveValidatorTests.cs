using System;
using System.Collections.Generic;
using MulderConfig.src.Configuration;
using MulderConfig.src.Save;
using Xunit;

namespace MulderConfig.tests.Save;

public class SaveValidatorTests
{
    private static ConfigModel MakeConfig(params OptionGroup[] groups)
    {
        return new ConfigModel
        {
            Game = new Game { Title = "Test", OriginalExe = "Game.exe" },
            Addons = new List<Addon> { new() { Title = "default" } },
            OptionGroups = new List<OptionGroup>(groups),
            Actions = new ActionRoot { Launch = new List<LaunchAction>(), Operations = new List<OperationAction>() }
        };
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidRadioChoice()
    {
        var config = MakeConfig(
            new OptionGroup
            {
                Name = "Renderer",
                Type = "radioGroup",
                Radios = new List<Radio>
                {
                    new() { Value = "DX9" },
                    new() { Value = "DX11" },
                }
            });

        var saved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Renderer"] = "DX9"
        };

        Assert.True(SaveValidator.IsValid(config, saved));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenGroupDoesNotExist()
    {
        var config = MakeConfig(
            new OptionGroup { Name = "Renderer", Type = "radioGroup", Radios = new List<Radio> { new() { Value = "DX9" } } });

        var saved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["OldGroup"] = "Whatever"
        };

        Assert.False(SaveValidator.IsValid(config, saved));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenRadioValueDoesNotExist()
    {
        var config = MakeConfig(
            new OptionGroup
            {
                Name = "Renderer",
                Type = "radioGroup",
                Radios = new List<Radio> { new() { Value = "DX9" } }
            });

        var saved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Renderer"] = "DX12"
        };

        Assert.False(SaveValidator.IsValid(config, saved));
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidCheckboxChoices()
    {
        var config = MakeConfig(
            new OptionGroup
            {
                Name = "Mods",
                Type = "checkboxGroup",
                Checkboxes = new List<Checkbox>
                {
                    new() { Value = "A" },
                    new() { Value = "B" },
                    new() { Value = "C" },
                }
            });

        var saved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Mods"] = new List<string> { "A", "C" }
        };

        Assert.True(SaveValidator.IsValid(config, saved));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenCheckboxValueDoesNotExist()
    {
        var config = MakeConfig(
            new OptionGroup
            {
                Name = "Mods",
                Type = "checkboxGroup",
                Checkboxes = new List<Checkbox> { new() { Value = "A" } }
            });

        var saved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Mods"] = new List<string> { "A", "B" }
        };

        Assert.False(SaveValidator.IsValid(config, saved));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForWrongTypes()
    {
        var config = MakeConfig(
            new OptionGroup
            {
                Name = "Renderer",
                Type = "radioGroup",
                Radios = new List<Radio> { new() { Value = "DX9" } }
            },
            new OptionGroup
            {
                Name = "Mods",
                Type = "checkboxGroup",
                Checkboxes = new List<Checkbox> { new() { Value = "A" } }
            });

        Assert.False(SaveValidator.IsValid(config, new Dictionary<string, object> { ["Renderer"] = new List<string> { "DX9" } }));
        Assert.False(SaveValidator.IsValid(config, new Dictionary<string, object> { ["Mods"] = "A" }));
    }
}
