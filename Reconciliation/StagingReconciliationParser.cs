using Api;

namespace Reconciliation;

internal static class StagingReconciliationParser
{
    private const string folder = "folder";
    private const string file = "file";

    internal static IEnumerable<Dictionary<ReconciliationFieldName, object>> Parse(
        IEnumerable<Dictionary<ReconciliationFieldName, object>> page, string code, string prefix, MapType mapType) =>
        page.Select(r => Adjust(r, code, prefix)).Where(r => mapType != MapType.Discovery || r[ReconciliationFieldName.FileFolder] as string == file);

    private static Dictionary<ReconciliationFieldName, object> Adjust(Dictionary<ReconciliationFieldName, object> row, string code, string filePrefix) =>
        row.Select(cell => Match(cell, row, code, filePrefix)).Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value!);

    private static KeyValuePair<ReconciliationFieldName, object?> Match(KeyValuePair<ReconciliationFieldName, object> cell,
        Dictionary<ReconciliationFieldName, object> row, string code, string filePrefix) =>
            cell.Key switch
            {
                ReconciliationFieldName.FileFolder => new(cell.Key, ToFileFolder(cell.Value as Uri)),
                ReconciliationFieldName.ImportLocation => new(cell.Key, ToImportLocation(row, cell.Value as string, code, filePrefix)),
                ReconciliationFieldName.VariationName => new(cell.Key, ToVariationName(row, cell.Value as string)),
                ReconciliationFieldName.AccessConditionName => new(cell.Key, ToAccessConditon(cell.Value as string)),
                ReconciliationFieldName.SensitivityReviewDuration => new(cell.Key, ToYearDuration(row, cell.Value as TimeSpan?)),
                ReconciliationFieldName.LegislationSectionReference => new(cell.Key, ToLegislationReferences(cell.Value as string)),
                ReconciliationFieldName.SensitivityReviewEndYear => new(cell.Key, cell.Value as int?),
                ReconciliationFieldName.RetentionType => new(cell.Key, ToRetentionType(cell.Value as string)),
                _ => new KeyValuePair<ReconciliationFieldName, object?>(cell.Key, cell.Value),
            };

    private static string? ToFileFolder(Uri? subject) =>
        subject == Vocabulary.Subset.Uri ? folder :
            subject == Vocabulary.Variation.Uri ? file : null;

    private static string? ToImportLocation(Dictionary<ReconciliationFieldName, object> row, string? importLocation, string code, string filePrefix)
    {
        if (row.TryGetValue(ReconciliationFieldName.FileFolder, out var fileFolder))
        {
            var replaced = importLocation?.Replace(code, filePrefix);
            if (!string.IsNullOrWhiteSpace(replaced) && replaced.Last() != '/' &&
                fileFolder.ToString() == Vocabulary.Subset.Uri.ToString())
            {
                replaced = $"{replaced}/";
            }

            return replaced;
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
        return null;
    }

    private static string? ToAccessConditon(string? accessConditionName) => accessConditionName?.Replace(' ', '_');

    private static int ToYearDuration(Dictionary<ReconciliationFieldName, object> row, TimeSpan? duration) =>
        row.TryGetValue(ReconciliationFieldName.SensitivityReviewEndYear, out var endYear) && endYear is not null ? (int)endYear :
            duration is null ? 0 : (int)Math.Floor(duration.Value.TotalDays / 365);

    private static string[] ToLegislationReferences(string? references) => string.IsNullOrWhiteSpace(references) ? ["open"] : references.Split(',', StringSplitOptions.RemoveEmptyEntries);

    private static string? ToRetentionType(string? accessConditionName) =>
        accessConditionName switch
        {
            "retained by department under section 3.4" => "retained_under_3.4",
            string acn when acn.Contains("retained") => acn.Replace(' ', '_'),
            _ => accessConditionName
        };
}
