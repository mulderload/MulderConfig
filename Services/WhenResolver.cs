using System.Linq;
using MulderLauncher.Models;

namespace MulderLauncher.Services
{
    public static class WhenResolver
    {
        public static bool Match(List<WhenGroup> groups, IReadOnlyDictionary<string, object?> selected)
        {
            if (groups == null || groups.Count == 0)
                return true; // vide = toujours vrai

            return groups.Any(group => IsGroupMatch(group, selected));
        }

        // ---- helpers ----------------------------------------------------

        private static bool IsGroupMatch(WhenGroup group, IReadOnlyDictionary<string, object?> selected)
        {
            foreach (var kvp in group)
            {
                var rawKey = kvp.Key;
                var expected = kvp.Value ?? string.Empty;

                var (op, key) = ParseKey(rawKey);

                selected.TryGetValue(key, out var selectedValue); // peut être null

                // Cas spécial : expected vide, sans opérateur => "rien sélectionné"
                if (op == Op.Equals && string.IsNullOrEmpty(expected))
                {
                    if (IsNothingSelected(selectedValue))
                        continue;

                    return false;
                }

                if (selectedValue == null)
                    return false;

                if (!IsValueMatch(selectedValue, expected, op))
                    return false;
            }

            return true;
        }

        private static bool IsNothingSelected(object? selectedValue)
        {
            if (selectedValue == null)
                return true;
            if (selectedValue is List<string> list && list.Count == 0)
                return true;
            return false;
        }

        private static bool IsValueMatch(object selectedValue, string expected, Op op)
        {
            if (selectedValue is List<string> list)
            {
                return op switch
                {
                    Op.Contains => list.Any(v => Contains(v, expected)),
                    Op.NotContains => !list.Any(v => Contains(v, expected)),
                    Op.NotEquals => !list.Contains(expected, StringComparer.OrdinalIgnoreCase),
                    _ => list.Contains(expected, StringComparer.OrdinalIgnoreCase),
                };
            }

            var actual = selectedValue.ToString() ?? string.Empty;
            return op switch
            {
                Op.Contains => Contains(actual, expected),
                Op.NotContains => !Contains(actual, expected),
                Op.NotEquals => !EqualsIgnoreCase(actual, expected),
                _ => EqualsIgnoreCase(actual, expected),
            };
        }

        private static (Op op, string key) ParseKey(string rawKey)
        {
            if (rawKey.StartsWith("!*"))
                return (Op.NotContains, rawKey.TrimStart('!', '*'));
            if (rawKey.StartsWith("*"))
                return (Op.Contains, rawKey.TrimStart('*'));
            if (rawKey.StartsWith("!"))
                return (Op.NotEquals, rawKey.TrimStart('!'));
            return (Op.Equals, rawKey);
        }

        private static bool Contains(string actual, string expected) =>
            actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool EqualsIgnoreCase(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private enum Op { Equals, NotEquals, Contains, NotContains }
    }
}