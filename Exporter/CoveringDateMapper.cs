using Api;
using VDS.RDF;

namespace Exporter;

internal static class CoveringDateMapper
{
    internal static CoveringDate GetDate(IGraph asset, DateTimeOffset? assetModifiedAt)
    {
        var assetHasOriginDateStart = YmdMapper.GetYmd(asset, null, Vocabulary.AssetHasOriginDateStart);
        var assetHasOriginDateEnd = YmdMapper.GetYmd(asset, null, Vocabulary.AssetHasOriginDateEnd);
        var assetHasOriginApproximateDateStart = YmdMapper.GetYmd(asset, null, Vocabulary.AssetHasOriginApproximateDateStart);
        var assetHasOriginApproximateDateEnd = YmdMapper.GetYmd(asset, null, Vocabulary.AssetHasOriginApproximateDateEnd);

        return new()
        {
            FullStart = AdjustDate(true, assetHasOriginDateStart) ??
                AdjustDate(true, assetHasOriginApproximateDateStart) ??
                assetModifiedAt?.ToString("yyyy-MM-dd"),
            Start = (assetHasOriginDateStart ?? assetHasOriginApproximateDateStart)?.ToTextDate(),
            FullEnd = AdjustDate(false, assetHasOriginDateEnd) ??
                AdjustDate(false, assetHasOriginApproximateDateEnd) ??
                assetModifiedAt?.ToString("yyyy-MM-dd"),
            End = (assetHasOriginDateEnd ?? assetHasOriginApproximateDateEnd)?.ToTextDate(),
            Text = assetHasOriginDateStart?.Verbatim ?? assetHasOriginApproximateDateStart?.Verbatim
        };
    }

    private static string? AdjustDate(bool isStart, Ymd? date)
    {
        if (date is null)
        {
            return null;
        }
        if (date.Month is null)
        {
            var rest = isStart ? "01-01" : "12-31";
            return $"{date.Year}-{rest}";
        }
        if (date.Day is null)
        {
            var lastDay = new DateTime((int)date.Year!, (int)date.Month!, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(1).AddDays(-1).Day;
            var rest = isStart ? "01" : $"{lastDay}";
            return $"{date.Year}-{date.Month:D2}-{rest}";
        }

        return date.ToTextDate();
    }


}