namespace MulderConfig.src.Save;

public class SaveModel
{
    public Dictionary<string, Dictionary<string, object>> AddonSelections { get; set; }
        = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
}
