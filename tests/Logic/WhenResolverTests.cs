using System.Collections.Generic;
using MulderConfig.src.Logic;
using MulderConfig.src.Configuration;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MulderConfig.tests.Logic;

public class WhenResolverTests
{
    private static List<WhenGroup> ParseWhen(string json, string prop = "when")
    {
        var token = JToken.Parse(json)[prop];
        return token!.ToObject<List<WhenGroup>>()!;
    }

    private static Dictionary<string, object> Sel(params (string key, object val)[] items)
    {
        var d = new Dictionary<string, object>();
        foreach (var (k, v) in items) d[k] = v;
        return d;
    }

    [Fact]
    public void And_AllConditionsMatch_Succeeds()
    {
        // AND: all conditions must match
        var json = @"{ ""when"": [ { ""Renderer"": ""DX9"", ""HDR"": ""Enabled"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DX9"), ("HDR", "Enabled"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void And_OneConditionMiss_Fails()
    {
        // AND: if one condition does not match => fail
        var json = @"{ ""when"": [ { ""Renderer"": ""DX9"", ""HDR"": ""Enabled"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DX11"), ("HDR", "Enabled"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void And_NoConditionMatches_Fails()
    {
        // AND: if all conditions mismatch => fail
        var json = @"{ ""when"": [ { ""Renderer"": ""DX9"", ""HDR"": ""Enabled"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DX11"), ("HDR", "Disabled"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void Or_AllGroupMatch_Succeeds()
    {
        // OR: all groupes match => succeeds
        var json = @"{ ""when"": [ { ""Resolution"": ""2560x1440"" }, { ""Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "2560x1440"), ("Renderer", "DXVK"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void Or_OneGroupMatches_Succeeds()
    {
        // OR: all groupes match => succeeds
        var json = @"{ ""when"": [ { ""Resolution"": ""2560x1440"" }, { ""Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DXVK"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void Or_NoGroupMatches_Fails()
    {
        // OR: aucun groupe ne matche => false
        var json = @"{ ""when"": [ { ""Resolution"": ""2560x1440"" }, { ""Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"), ("Renderer", "D3D9"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void OrOfAndGroups_MixedExample_Succeeds()
    {
        // (Resolution contains 1920x AND Renderer == DXVK)  OR  (FOV Modifier != "None")
        var json = @"
        {
            ""when"": [
            { ""*Resolution"": ""1920x"", ""Renderer"": ""DXVK"" },
            { ""!FOV Modifier"": ""None"" }
            ]
        }";
        var when = ParseWhen(json);

        // The first group fails (renderer != DXVK), but the second succeeds => OR => true
        var selected = Sel(("Resolution", "1920x1080"), ("Renderer", "D3D9"), ("FOV Modifier", "lower"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void NotEquals_Succeeds()
    {
        var json = @"{ ""when"": [ { ""!Renderer"": ""DXVK"" } ] }";

        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DX9"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void NotEquals_Fails()
    {
        var json = @"{ ""when"": [ { ""!Resolution"": ""1920x1080"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void Contains_Succeeds()
    {
        var json = @"{ ""when"": [ { ""*Resolution"": ""1920x"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void Contains_Fails()
    {
        var json = @"{ ""when"": [ { ""*Resolution"": ""2560x"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void NotContains_Succeeds()
    {
        var json = @"{ ""when"": [ { ""!*Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DX9"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void NotContains_Fails()
    {
        var json = @"{ ""when"": [ { ""!*Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "Vulkan DXVK Wrapper"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void EmptyExpected_MatchesNothingSelected()
    {
        var json = @"{ ""when"": [ { ""Switchable Mods"": """" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Switchable Mods", new List<string>()));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void List_Contains_Succeeds()
    {
        var json = @"{ ""when"": [ { ""*Switchable Mods"": ""NV"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Switchable Mods", new List<string> { "NVHR", "DXVK" }));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void List_NotContains_Fails()
    {
        var json = @"{ ""when"": [ { ""!*Switchable Mods"": ""Vulkan"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Switchable Mods", new List<string> { "NVHR", "Vulkan DXVK Wrapper" }));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void MissingKey_Equals_Fails()
    {
        var json = @"{ ""when"": [ { ""Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void MissingKey_Contains_Fails()
    {
        var json = @"{ ""when"": [ { ""*Renderer"": ""DX"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.False(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void MissingKey_NotEquals_Succeeds()
    {
        var json = @"{ ""when"": [ { ""!Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void MissingKey_NotContains_Succeeds()
    {
        var json = @"{ ""when"": [ { ""!*Renderer"": ""DXVK"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Resolution", "1920x1080"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void CaseInsensitive_Equals_And_Contains_Work()
    {
        var json = @"{ ""when"": [ { ""Renderer"": ""dxvk"", ""*Resolution"": ""1920X"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DXVK"), ("Resolution", "1920x1080"));

        Assert.True(WhenResolver.Match(when, selected));
    }

    [Fact]
    public void CaseInsensitive_NotEquals_And_NotContains_Work()
    {
        var json = @"{ ""when"": [ { ""!Renderer"": ""dxvk"", ""!*Resolution"": ""(21/9)"" } ] }";
        var when = ParseWhen(json);
        var selected = Sel(("Renderer", "DX9"), ("Resolution", "1920x1080 (16/9)"));

        Assert.True(WhenResolver.Match(when, selected));
    }
}
