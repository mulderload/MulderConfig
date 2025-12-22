using System.Collections.Generic;
using MulderLauncher.Models;
using MulderLauncher.Services;
using Newtonsoft.Json.Linq;
using Xunit;

namespace TestProject.Tests
{
    public class WhenResolverJsonTests
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
        public void Json_When_Contains_Succeeds()
        {
            var json = @"{ ""when"": [ { ""*Resolution"": ""1920x"" } ] }";
            var when = ParseWhen(json);
            var selected = Sel(("Resolution", "1920x1080"));

            Assert.True(WhenResolver.Match(when, selected));
        }

        [Fact]
        public void Json_When_NotEquals_Fails()
        {
            var json = @"{ ""when"": [ { ""!Resolution"": ""1920x1080"" } ] }";
            var when = ParseWhen(json);
            var selected = Sel(("Resolution", "1920x1080"));

            Assert.False(WhenResolver.Match(when, selected));
        }

        [Fact]
        public void Json_When_List_NotContains_Succeeds()
        {
            var json = @"{ ""when"": [ { ""!*Switchable Mods"": ""Vulkan"" } ] }";
            var when = ParseWhen(json);
            var selected = Sel(("Switchable Mods", new List<string> { "NVHR", "DXVK" }));

            Assert.True(WhenResolver.Match(when, selected));
        }

        [Fact]
        public void Json_When_EmptyExpected_MatchesNothingSelected()
        {
            var json = @"{ ""when"": [ { ""Switchable Mods"": """" } ] }";
            var when = ParseWhen(json);
            var selected = Sel(("Switchable Mods", new List<string>()));

            Assert.True(WhenResolver.Match(when, selected));
        }
    }
}
