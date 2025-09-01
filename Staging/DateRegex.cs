using System.Text.RegularExpressions;

namespace Staging;

internal partial class DateRegex
{
    [GeneratedRegex("^\\[?(?<start>\\d{4})-(?<end>\\d{4})\\]?$")]
    public static partial Regex YearRange();
}
