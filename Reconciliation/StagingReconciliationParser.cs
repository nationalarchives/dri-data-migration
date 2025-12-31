using Api;
using System.Text;

namespace Reconciliation;

internal static class StagingReconciliationParser
{
    private const string folder = "folder";
    private const string file = "file";

    internal static IEnumerable<Dictionary<ReconciliationFieldName, object>> Parse(
        IEnumerable<Dictionary<ReconciliationFieldName, object>> page, ReconciliationMapType mapType) =>
        page.Select(r => Adjust(r));

    private static Dictionary<ReconciliationFieldName, object> Adjust(Dictionary<ReconciliationFieldName, object> row) =>
        row.Select(cell => Match(cell, row)).Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value!);

    private static KeyValuePair<ReconciliationFieldName, object?> Match(KeyValuePair<ReconciliationFieldName, object> cell,
        Dictionary<ReconciliationFieldName, object> row) =>
            cell.Key switch
            {
                ReconciliationFieldName.Id => new(cell.Key, ToId(row, cell.Value as string)),
                ReconciliationFieldName.Reference => new(cell.Key, ToReference(row, cell.Value as string)),
                ReconciliationFieldName.FileFolder => new(cell.Key, ToFileFolder(cell.Value as Uri)),
                ReconciliationFieldName.Location => new(cell.Key, ToImportLocation(row, cell.Value as string)),
                ReconciliationFieldName.Name => new(cell.Key, ToVariationName(row, cell.Value as string)),
                ReconciliationFieldName.CoveringDateStart => new(cell.Key, ToOriginDate(cell.Value as string)),
                ReconciliationFieldName.CoveringDateEnd => new(cell.Key, ToOriginDate(cell.Value as string)),
                ReconciliationFieldName.AccessConditionName => new(cell.Key, ToAccessConditon(cell.Value as string)),
                ReconciliationFieldName.ClosurePeriod => new(cell.Key, ToYearDuration(row, cell.Value as TimeSpan?)),
                ReconciliationFieldName.FoiExemptionReference => new(cell.Key, ToLegislationReferences(cell.Value as string)),
                ReconciliationFieldName.ClosureEndYear => new(cell.Key, cell.Value as int?),
                ReconciliationFieldName.RetentionType => new(cell.Key, ToRetentionType(cell.Value as string)),
                ReconciliationFieldName.ClosureStatus => new(cell.Key, ToClosureStatus(row, cell.Value as string)),
                _ => new KeyValuePair<ReconciliationFieldName, object?>(cell.Key, cell.Value),
            };

    private static string? ToId(Dictionary<ReconciliationFieldName, object> row, string? driId) =>
        !string.IsNullOrWhiteSpace(driId) ?
        row.TryGetValue(ReconciliationFieldName.RedactedVariationSequence, out var redactedSequence) ?
        $"{Guid.Parse(driId.ToString()):N}_{redactedSequence}" : Guid.Parse(driId.ToString()).ToString("N") : null;

    private static string? ToReference(Dictionary<ReconciliationFieldName, object> row, string? reference) =>
        !string.IsNullOrWhiteSpace(reference) ?
        row.TryGetValue(ReconciliationFieldName.RedactedVariationSequence, out var redactedSequence) ?
        $"{reference}/{redactedSequence}" : reference : null;

    private static string? ToFileFolder(Uri? subject) =>
        subject == Vocabulary.Subset.Uri ? folder :
            subject == Vocabulary.Variation.Uri ? file : null;

    private static string? ToImportLocation(Dictionary<ReconciliationFieldName, object> row, string? importLocation)
    {
        if (row.TryGetValue(ReconciliationFieldName.FileFolder, out var fileFolder))
        {
            if (!string.IsNullOrWhiteSpace(importLocation) && importLocation.Last() != '/' &&
                fileFolder.ToString() == Vocabulary.Subset.Uri.ToString())
            {
                importLocation = $"{importLocation}/";
            }

            return importLocation;
        }
        return null;
    }

    private static string? ToVariationName(Dictionary<ReconciliationFieldName, object> row, string? variationName)
    {
        if (row.TryGetValue(ReconciliationFieldName.FileFolder, out var fileFolder))
        {
            return fileFolder is not null && fileFolder.ToString() == Vocabulary.Subset.Uri.ToString() ?
                variationName?.Split('/').Last() : variationName;
        }
        return variationName;
    }

    private static int? ToOriginDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }
        var dt = date.Split(['-', 'T'], StringSplitOptions.RemoveEmptyEntries);
        if (dt.Length == 0)
        {
            return null;
        }
        var sb = new StringBuilder();
        sb.Append(dt[0]);
        if (dt.Length > 1)
        {
            for (var i = 1; i < Math.Min(dt.Length, 3); i++)
            {
                sb.Append(dt[i]);
            }
        }

        return int.TryParse(sb.ToString(), out var origin) ? origin : null;
    }

    private static string? ToAccessConditon(string? accessConditionName) => accessConditionName?.Replace(' ', '_');

    private static int ToYearDuration(Dictionary<ReconciliationFieldName, object> row, TimeSpan? duration) =>
        row.TryGetValue(ReconciliationFieldName.ClosureEndYear, out var endYear) && endYear is not null ? (int)endYear :
            duration is null ? 0 : (int)Math.Floor(duration.Value.TotalDays / 365);

    private static string[] ToLegislationReferences(string? references) => string.IsNullOrWhiteSpace(references) ? ["open"] : references.Split(',', StringSplitOptions.RemoveEmptyEntries);

    private static string? ToRetentionType(string? accessConditionName) =>
        accessConditionName switch
        {
            "retained by department under section 3.4" => "retained_under_3.4",
            "temporarily retained by department" => "temporarily_retained",
            string acn when acn.Contains("retained") => acn.Replace(' ', '_'),
            _ => accessConditionName
        };

    private static string? ToClosureStatus(Dictionary<ReconciliationFieldName, object> row, string? accessConditionCode)
    {
        if (accessConditionCode is null)
        {
            return null;
        }
        var isPublicDescription = (bool)row.GetValueOrDefault(ReconciliationFieldName.IsPublicDescription, true);

        return accessConditionCode switch
        {
            "A" or "I" => "O",
            "F" or "C" or "U" or "V" or "W" => isPublicDescription ? "D" : "C",
            _ => null
        };
    }
}
