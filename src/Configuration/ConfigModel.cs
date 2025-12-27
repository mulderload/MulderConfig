using Newtonsoft.Json;

namespace MulderConfig.src.Configuration;

public class ConfigModel
{
    public required Game Game { get; set; }
    public List<Addon>? Addons { get; set; }
    public required List<OptionGroup> OptionGroups { get; set; }
    public required ActionRoot Actions { get; set; }
}

public class Game
{
    [JsonProperty("name")]
    public required string Name { get; set; }

    public required string OriginalExe { get; set; }
}

public class Addon
{
    public required string Title { get; set; }
    public int? SteamId { get; set; }
}

public class OptionGroup
{
    public required string Name { get; set; }
    public required string Type { get; set; } // "radioGroup" | "checkboxGroup"
    public List<Radio>? Radios { get; set; }
    public List<Checkbox>? Checkboxes { get; set; }
}

public class Radio
{
    public required string Value { get; set; }
    public List<WhenGroup>? DisabledWhen { get; set; }
}

public class Checkbox
{
    public required string Value { get; set; }
    public List<WhenGroup>? DisabledWhen { get; set; }
}

public class WhenGroup : Dictionary<string, string>
{
}

public class ActionRoot
{
    public required List<LaunchAction> Launch { get; set; }
    public required List<OperationAction> Operations { get; set; }
}

public class LaunchAction
{
    public List<WhenGroup>? When { get; set; }

    // Atomic override: if present, it defines both exe name + working directory
    public ExecSpec? Exec { get; set; }

    // Cumulative: appended to the final args in JSON order
    public List<string>? Args { get; set; }
}

public class ExecSpec
{
    public required string Name { get; set; }
    public required string WorkDir { get; set; }
}

public class OperationAction
{
    public List<WhenGroup>? When { get; set; }
    public required string Operation { get; set; }

    public string? Source { get; set; }
    public string? Target { get; set; }

    public List<string>? Files { get; set; }
    public string? Pattern { get; set; }
    public string? Search { get; set; }
    public string? Replacement { get; set; }
}
