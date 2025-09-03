using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Staging;

public partial class DimensionParser(ILogger logger)
{
    public Dimension ParseCentimetre(string? obverseOrReverseText, string dimensionText)
    {
        var trimmedDimension = dimensionText.Trim();
        if (trimmedDimension == "Fragment")
        {
            return new Dimension(DimensionType.Fragment);
        }
        if (trimmedDimension == "Reverse: Fragment")
        {
            return new Dimension(DimensionType.ReverseFragment);
        }
        if (trimmedDimension == "Obverse: Fragment")
        {
            return new Dimension(DimensionType.ObverseFragment);
        }
        if (trimmedDimension == "Obverse and Reverse: Fragment")
        {
            return new Dimension(DimensionType.ObverseAndReverseFragment);
        }
        if (trimmedDimension == "Unknown")
        {
            return new Dimension(DimensionType.None);
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
        var dimensionType = isObverse == true ? DimensionType.Obverse :
                isObverse == false ? DimensionType.Reverse :
                DimensionType.Dimension;

        var singleMatch = Single().Match(trimmedDimension);
        if (singleMatch.Success)
        {
            return ParseSingle(singleMatch, dimensionType, "one", trimmedDimension);
        }
        var singleReverseMatch = SingleReverse().Match(trimmedDimension);
        if (singleReverseMatch.Success)
        {
            return ParseSingle(singleReverseMatch, DimensionType.Reverse, "one", trimmedDimension);
        }
        var singleObverseMatch = SingleObverse().Match(trimmedDimension);
        if (singleObverseMatch.Success)
        {
            return ParseSingle(singleObverseMatch, DimensionType.Obverse, "one", trimmedDimension);
        }

        var singleObverseAndReverseMatch = SingleObverseAndReverse().Match(trimmedDimension);
        if (singleObverseAndReverseMatch.Success)
        {
            var g1= ParseSingle(singleObverseAndReverseMatch, DimensionType.IdenticalObverseAndReverse, "one", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            return new Dimension(DimensionType.IdenticalObverseAndReverse, g1.FirstMm, null, g1.FirstMm);
        }

        var doubleMatch = Double().Match(trimmedDimension);
        if (doubleMatch.Success)
        {
            return ParseDouble(doubleMatch, dimensionType, "one", "two", trimmedDimension);
        }
        var doubleObverseMatch = DoubleObverse().Match(trimmedDimension);
        if (doubleObverseMatch.Success)
        {
            return ParseDouble(doubleObverseMatch, DimensionType.Obverse, "one", "two", trimmedDimension);
        }
        var doubleObverseAndReverseMatch = DoubleObverseAndReverse().Match(trimmedDimension);
        if (doubleObverseAndReverseMatch.Success)
        {
            var g1 = ParseDouble(doubleObverseAndReverseMatch, DimensionType.IdenticalObverseAndReverse, "one", "two", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            return new Dimension(DimensionType.IdenticalObverseAndReverse, g1.FirstMm, g1.SecondMm, g1.FirstMm, g1.SecondMm);
        }

        var singleObverseAndSingleReverseMatch = SingleObverseAndSingleReverse().Match(trimmedDimension);
        if (singleObverseAndSingleReverseMatch.Success)
        {
            var g1 = ParseSingle(singleObverseAndSingleReverseMatch, DimensionType.FirstObverseSecondReverse, "one", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            var g2 = ParseSingle(singleObverseAndSingleReverseMatch, DimensionType.FirstObverseSecondReverse, "two", trimmedDimension);
            if (g2.DimensionKind == DimensionType.None)
            {
                return g2;
            }
            return new Dimension(DimensionType.FirstObverseSecondReverse, g1.FirstMm, null, g2.FirstMm, null);
        }

        var singleObverseAndFragmentReverseMatch = SingleObverseAndFragmentReverse().Match(trimmedDimension);
        if (singleObverseAndFragmentReverseMatch.Success)
        {
            var g1 = ParseSingle(singleObverseAndFragmentReverseMatch, DimensionType.FirstObverseSecondReverse, "one", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            return new Dimension(DimensionType.FirstObverseFragmentReverse, g1.FirstMm, null, null, null);
        }
        var doubleObverseAndFragmentReverseMatch = DoubleObverseAndFragmentReverse().Match(trimmedDimension);
        if (doubleObverseAndFragmentReverseMatch.Success)
        {
            var g1 = ParseDouble(doubleObverseAndFragmentReverseMatch, DimensionType.FirstObverseSecondReverse, "one", "two", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            return new Dimension(DimensionType.FirstObverseFragmentReverse, g1.FirstMm, g1.SecondMm, null, null);
        }

        var fragmentObverseAndSingleReverseMatch = FragmentObverseAndSingleReverse().Match(trimmedDimension);
        if (fragmentObverseAndSingleReverseMatch.Success)
        {
            var g1 = ParseSingle(fragmentObverseAndSingleReverseMatch, DimensionType.FragmentObverseSecondReverse, "one", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            return new Dimension(DimensionType.FragmentObverseSecondReverse, null, null, g1.FirstMm, null);
        }

        var fragmentObverseAndDoubleReverseMatch = FragmentObverseAndDoubleReverse().Match(trimmedDimension);
        if (fragmentObverseAndDoubleReverseMatch.Success)
        {
            var g1 = ParseDouble(fragmentObverseAndDoubleReverseMatch, DimensionType.FragmentObverseSecondReverse, "one", "two", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            return new Dimension(DimensionType.FragmentObverseSecondReverse, null, null, g1.FirstMm, g1.SecondMm);
        }

        var singleObverseAndDoubleReverseMatch = SingleObverseAndDoubleReverse().Match(trimmedDimension);
        if (singleObverseAndDoubleReverseMatch.Success)
        {
            var g1 = ParseSingle(singleObverseAndDoubleReverseMatch, DimensionType.FirstObverseSecondReverse, "one", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            var g2 = ParseDouble(singleObverseAndDoubleReverseMatch, DimensionType.FirstObverseSecondReverse, "two", "three", trimmedDimension);
            if (g2.DimensionKind == DimensionType.None)
            {
                return g2;
            }
            return new Dimension(DimensionType.FirstObverseSecondReverse, g1.FirstMm, null, g2.FirstMm, g2.SecondMm);
        }
        var doubleObverseAndSingleReverseMatch = DoubleObverseAndSingleReverse().Match(trimmedDimension);
        if (doubleObverseAndSingleReverseMatch.Success)
        {
            var g1 = ParseDouble(doubleObverseAndSingleReverseMatch, DimensionType.FirstObverseSecondReverse, "one", "two", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            var g2 = ParseSingle(doubleObverseAndSingleReverseMatch, DimensionType.FirstObverseSecondReverse, "three", trimmedDimension);
            if (g2.DimensionKind == DimensionType.None)
            {
                return g2;
            }
            return new Dimension(DimensionType.FirstObverseSecondReverse, g1.FirstMm, g1.SecondMm, g2.FirstMm, null);
        }
        var doubleObverseAndDoubleReverseMatch = DoubleObverseAndDoubleReverse().Match(trimmedDimension);
        if (doubleObverseAndDoubleReverseMatch.Success)
        {
            var g1 = ParseDouble(doubleObverseAndDoubleReverseMatch, DimensionType.FirstObverseSecondReverse, "one", "two", trimmedDimension);
            if (g1.DimensionKind == DimensionType.None)
            {
                return g1;
            }
            var g2 = ParseDouble(doubleObverseAndDoubleReverseMatch, DimensionType.FirstObverseSecondReverse, "three", "four", trimmedDimension);
            if (g2.DimensionKind == DimensionType.None)
            {
                return g2;
            }
            return new Dimension(DimensionType.FirstObverseSecondReverse, g1.FirstMm, g1.SecondMm, g2.FirstMm, g2.SecondMm);
        }

        logger.UnableParseDimension(dimensionText);
        return new Dimension(DimensionType.None);
    }

    private Dimension ParseSingle(Match match, DimensionType dimensionType, string groupName, string dimensionText)
    {
        if (double.TryParse(match.Groups[groupName].Value, out var cm))
        {
            var mm = (int)(cm * 10);
            return new Dimension(dimensionType, mm);
        }

        logger.UnableParseDimension(dimensionText);
        return new Dimension(DimensionType.None);
    }

    private Dimension ParseDouble(Match match, DimensionType dimensionType,
        string groupNameFirst, string groupNameSecond, string dimensionText)
    {
        var g1 = ParseSingle(match, dimensionType, groupNameFirst, dimensionText);
        if (g1.DimensionKind == DimensionType.None)
        {
            return g1;
        }

        var g2 = ParseSingle(match, dimensionType, groupNameSecond, dimensionText);
        if (g2.DimensionKind == DimensionType.None)
        {
            return g2;
        }

        return new Dimension(dimensionType, g1.FirstMm, g2.FirstMm);
    }

    public enum DimensionType
    {
        None,
        Dimension,
        IdenticalObverseAndReverse,
        FragmentObverseSecondReverse,
        FirstObverseSecondReverse,
        FirstObverseFragmentReverse,
        Obverse,
        Reverse,
        Fragment,
        ReverseFragment,
        ObverseFragment,
        ObverseAndReverseFragment,
    }

    public record Dimension(DimensionType DimensionKind, int? FirstMm = null, int? SecondMm = null, int? SecondFirstMm = null, int? SecondSecondMm = null);

    public const string regexOneText = "(?<one>\\d+\\.?\\d*)\\.?\\s*(cm)?";
    public const string regexTwoText = "(?<two>\\d+\\.?\\d*)\\.?\\s*(cm)?";
    public const string regexThreeText = "(?<three>\\d+\\.?\\d*)\\.?\\s*(cm)?";
    public const string regexFourText = "(?<four>\\d+\\.?\\d*)\\.?\\s*(cm)?";

    [GeneratedRegex($"^{regexOneText}$")]
    public static partial Regex Single();

    [GeneratedRegex($"^Obverse and Reverse:\\s*{regexOneText}$")]
    public static partial Regex SingleObverseAndReverse();

    [GeneratedRegex($"^{regexOneText}\\s*[xX]\\s*{regexTwoText}$")]
    public static partial Regex Double();

    [GeneratedRegex($"^Reverse:\\s*{regexOneText}$")]
    public static partial Regex SingleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}$")]
    public static partial Regex SingleObverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*[xX]\\s*{regexTwoText}$")]
    public static partial Regex DoubleObverse();

    [GeneratedRegex($"^Obverse and Reverse:\\s*{regexOneText}\\s*[xX]\\s*{regexTwoText}$")]
    public static partial Regex DoubleObverseAndReverse();

    [GeneratedRegex($"^Obverse:\\s*[Ff]ragment\\s*(<lb/>)?Reverse:\\s*{regexOneText}$")]
    public static partial Regex FragmentObverseAndSingleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*(<lb/>)?Reverse:\\s*{regexTwoText}$")]
    public static partial Regex SingleObverseAndSingleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*(<lb/>)?Reverse:\\s*[Ff]ragment$")]
    public static partial Regex SingleObverseAndFragmentReverse();

    [GeneratedRegex($"^Obverse:\\s*[Ff]ragment\\s*(<lb/>)?Reverse:\\s*{regexOneText}\\s*[xX]\\s*{regexTwoText}$")]
    public static partial Regex FragmentObverseAndDoubleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*(<lb/>)?Reverse:\\s*{regexTwoText}\\s*[xX]\\s*{regexThreeText}$")]
    public static partial Regex SingleObverseAndDoubleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*[xX]\\s*{regexTwoText}\\s*(<lb/>)?Reverse:\\s*{regexThreeText}$")]
    public static partial Regex DoubleObverseAndSingleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*[xX]\\s*{regexTwoText}\\s*(<lb/>)?Reverse:\\s*{regexThreeText}\\s*[xX]\\s*{regexFourText}$")]
    public static partial Regex DoubleObverseAndDoubleReverse();

    [GeneratedRegex($"^Obverse:\\s*{regexOneText}\\s*[xX]\\s*{regexTwoText}\\s*(<lb/>)?Reverse:\\s*[Ff]ragment$")]
    public static partial Regex DoubleObverseAndFragmentReverse();
}
