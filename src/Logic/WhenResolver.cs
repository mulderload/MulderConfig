using MulderConfig.src.Configuration;

namespace MulderConfig.src.Logic;


public static class WhenResolver
{
    public static bool Match(List<WhenGroup> groups, IReadOnlyDictionary<string, object?> selected)
    {
        if (groups == null || groups.Count == 0)
            return true; // Empty `when` means "always apply".

        return groups.Any(group => IsGroupMatch(group, selected));
    }

    private static bool IsGroupMatch(WhenGroup group, IReadOnlyDictionary<string, object?> selected)
    {
        foreach (var kvp in group)
        {
            var rawKey = kvp.Key;
            var expected = kvp.Value ?? string.Empty;

            var (op, key) = ParseKey(rawKey);

            bool hasKey = selected.TryGetValue(key, out var selectedValue);

            // Special case: expected is "" and operator is Equals => "nothing selected".
            // This is mainly for checkbox groups where an empty list means "no selection".
            if (op == ConditionOperator.Equals && string.IsNullOrEmpty(expected))
            {
                if (IsNullOrEmptySelection(selectedValue))
                    continue;

                return false;
            }

            // Missing key (or null value):
            // - Equals / Contains => cannot match
            // - NotEquals / NotContains => considered true ("different" / "does not contain")
            if (!hasKey || selectedValue == null)
            {
                if (op == ConditionOperator.NotEquals || op == ConditionOperator.NotContains)
                    continue;

                return false;
            }

            if (!IsValueMatch(selectedValue, expected, op))
                return false;
        }

        return true;
    }

    private static bool IsNullOrEmptySelection(object? selectedValue)
    {
        if (selectedValue == null)
            return true;
        if (selectedValue is List<string> list && list.Count == 0)
            return true;
        return false;
    }

    private static bool IsValueMatch(object selectedValue, string expected, ConditionOperator op)
    {
        if (selectedValue is List<string> list)
        {
            return op switch
            {
                ConditionOperator.Contains => list.Any(v => ContainsIgnoreCase(v, expected)),
                ConditionOperator.NotContains => !list.Any(v => ContainsIgnoreCase(v, expected)),
                ConditionOperator.NotEquals => !list.Contains(expected, StringComparer.OrdinalIgnoreCase),
                _ => list.Contains(expected, StringComparer.OrdinalIgnoreCase),
            };
        }

        var actual = selectedValue.ToString() ?? string.Empty;
        return op switch
        {
            ConditionOperator.Contains => ContainsIgnoreCase(actual, expected),
            ConditionOperator.NotContains => !ContainsIgnoreCase(actual, expected),
            ConditionOperator.NotEquals => !EqualsIgnoreCase(actual, expected),
            _ => EqualsIgnoreCase(actual, expected),
        };
    }

    private static (ConditionOperator op, string key) ParseKey(string rawKey)
    {
        if (rawKey.StartsWith("!*"))
            return (ConditionOperator.NotContains, rawKey.TrimStart('!', '*'));
        if (rawKey.StartsWith("*"))
            return (ConditionOperator.Contains, rawKey.TrimStart('*'));
        if (rawKey.StartsWith("!"))
            return (ConditionOperator.NotEquals, rawKey.TrimStart('!'));
        return (ConditionOperator.Equals, rawKey);
    }

    private static bool ContainsIgnoreCase(string actual, string expected) =>
        actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool EqualsIgnoreCase(string a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private enum ConditionOperator
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
    }
}
