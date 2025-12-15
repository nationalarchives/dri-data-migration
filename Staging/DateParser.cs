using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Staging;

internal partial class DateParser(ILogger logger)
{
    internal YearMonthDay ParseDate(string dateText)
    {
        var trimmedDate = dateText.Trim();
        if (trimmedDate.StartsWith('[') && trimmedDate.IndexOf(']') == trimmedDate.Length - 1)
        {
            trimmedDate = trimmedDate.Remove(trimmedDate.Length - 1, 1).Remove(0, 1);
        }
        var dateType = trimmedDate.StartsWith("c ") ? DateType.Approximate : DateType.Date;
        if (dateType == DateType.Approximate)
        {
            trimmedDate = trimmedDate.Remove(0, 2);
        }

        if (TryParseDate(trimmedDate, out var singleDate, true))
        {
            return new(dateType, singleDate!.Year, singleDate!.Month, singleDate!.Day);
        }

        logger.UnrecognizedYearMonthDayFormat(dateText);
        return new(DateType.None);
    }

    internal DateRange ParseDateRange(string? obverseOrReverseText, string dateText)
    {
        var trimmedDate = dateText.Trim();
        if (trimmedDate.StartsWith('[') && trimmedDate.IndexOf(']') == trimmedDate.Length - 1)
        {
            trimmedDate = trimmedDate.Remove(trimmedDate.Length - 1, 1).Remove(0, 1);
        }
        if (trimmedDate.Contains("Unknown"))
        {
            return new DateRange(DateRangeType.None);
        }
        var dateType = trimmedDate.StartsWith("c ") ? DateRangeType.Approximate : DateRangeType.Date;
        if (dateType == DateRangeType.Approximate)
        {
            trimmedDate = trimmedDate.Remove(0, 2);
        }

        bool? isObverse = null;
        if (!string.IsNullOrWhiteSpace(obverseOrReverseText))
        {
            isObverse = obverseOrReverseText switch
            {
                "Obverse" => true,
                "Reverse" => false,
                _ => null
            };
            if (isObverse is null)
            {
                logger.UnrecognizedFaceFormat(obverseOrReverseText);
            }
        }

        var dateRangeType = isObverse == true ? DateRangeType.Obverse :
                isObverse == false ? DateRangeType.Reverse :
                dateType;

        if (TryParseDate(trimmedDate, out var singleDate, true))
        {
            return new DateRange(dateRangeType, singleDate!.Year, singleDate!.Month, singleDate!.Day);
        }

        var singleList = new List<Tuple<Regex, DateRangeType>>([
            Tuple.Create(ObverseSingleYear(), DateRangeType.Obverse),
            Tuple.Create(ReverseSingleYear(), DateRangeType.Reverse),
            Tuple.Create(ObverseAndReverseSingleYear(), DateRangeType.IdenticalObverseAndReverse),
            ]);
        foreach (var single in singleList)
        {
            var match = single.Item1.Match(trimmedDate);
            if (match.Success &&
                int.TryParse(match.Groups["startYear"].Value, out var startYear))
            {
                return new DateRange(single.Item2, startYear);
            }
        }

        var rangeList = new List<Tuple<Regex, DateRangeType>>([
            Tuple.Create(YearRange(), dateRangeType),
            Tuple.Create(ObverseYearRange(), DateRangeType.Obverse),
            Tuple.Create(ReverseYearRange(), DateRangeType.Reverse),
            Tuple.Create(ObverseAndReverseYearRange(), DateRangeType.IdenticalObverseAndReverse)
            ]);
        foreach (var range in rangeList)
        {
            var match = range.Item1.Match(trimmedDate);
            if (match.Success &&
                int.TryParse(match.Groups["startYear"].Value, out var startYear) &&
                int.TryParse(match.Groups["endYear"].Value, out var endYear))
            {
                return new DateRange(range.Item2, startYear, null, null, endYear);
            }
        }

        logger.UnrecognizedYearMonthDayFormat(dateText);
        return new DateRange(DateRangeType.None);
    }

    internal record Ymd(int? Year = null, int? Month = null, int? Day = null);
    
    internal record YearMonthDay(DateType DateKind, int? Year = null, int? Month = null, int? Day = null);

    internal record DateRange(DateRangeType DateRangeKind, int? FirstYear = null, int? FirstMonth = null, int? FirstDay = null, int? SecondYear = null, int? SecondMonth = null, int? SecondDay = null);

    [GeneratedRegex("^(?<startYear>\\d{4})-(?<endYear>\\d{4})$")]
    public static partial Regex YearRange();

    [GeneratedRegex("^Obverse:\\s*(?<startYear>\\d{4})$")]
    public static partial Regex ObverseSingleYear();

    [GeneratedRegex("^Obverse:\\s*(?<startYear>\\d{4})-(?<endYear>\\d{4})$")]
    public static partial Regex ObverseYearRange();

    [GeneratedRegex("^Reverse:\\s*(?<startYear>\\d{4})$")]
    public static partial Regex ReverseSingleYear();

    [GeneratedRegex("^Reverse:\\s*(?<startYear>\\d{4})-(?<endYear>\\d{4})$")]
    public static partial Regex ReverseYearRange();

    [GeneratedRegex("^Obverse and Reverse:\\s*(?<startYear>\\d{4})$")]
    public static partial Regex ObverseAndReverseSingleYear();

    [GeneratedRegex("^Obverse and Reverse:\\s*(?<startYear>\\d{4})-(?<endYear>\\d{4})$")]
    public static partial Regex ObverseAndReverseYearRange();

    public enum DateType
    {
        None,
        Date,
        Approximate
    }

    public enum DateRangeType
    {
        None,
        Date,
        Approximate,
        IdenticalObverseAndReverse,
        Obverse,
        Reverse
    }

    internal bool TryParseDate(string? date, out Ymd? dt, bool suppressWarning = false)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            dt = default;
            return false;
        }
        date = date.Replace(" Sept ", " Sep ");
        date = date.EndsWith(" Sept") ? date.Replace(" Sept", " Sep") : date;
        if (DateTimeOffset.TryParseExact(date, "yyyy MMM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var ym1))
        {
            dt = new(ym1.Year, ym1.Month);
            return true;
        }
        if (DateTimeOffset.TryParseExact(date, "yyyy MMMM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var ym2))
        {
            dt = new(ym2.Year, ym2.Month);
            return true;
        }
        if (DateTimeOffset.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt1))
        {
            dt = new(dt1.Year, dt1.Month, dt1.Day);
            return true;
        }
        if (DateTimeOffset.TryParseExact(date, "yyyy MMM d", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt2))
        {
            dt = new(dt2.Year, dt2.Month, dt2.Day);
            return true;
        }
        if (DateTimeOffset.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt3))
        {
            dt = new(dt3.Year, dt3.Month, dt3.Day);
            return true;
        }
        if (int.TryParse(date, out var singleYear))
        {
            dt = new(singleYear);
            return true;
        }

        if (!suppressWarning)
        {
            logger.UnrecognizedDateFormat(date);
        }
        dt = null;
        return false;
    }
}
