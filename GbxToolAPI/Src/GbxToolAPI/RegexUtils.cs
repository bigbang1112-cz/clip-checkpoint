using System.Text.RegularExpressions;

namespace GbxToolAPI;

public partial class RegexUtils
{
    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])")]
    private static partial Regex RegexPascalCaseToKebabCase();

    public static string PascalCaseToKebabCase(string str)
    {
        return RegexPascalCaseToKebabCase().Replace(str, "-$1").Trim().ToLower();
    }
}
