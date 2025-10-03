using Api;
using System.Text;

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
                ReconciliationFieldName.Id => new(cell.Key, ToId(row, cell.Value as string)),
                ReconciliationFieldName.Reference => new(cell.Key, ToReference(row, cell.Value as string)),
                ReconciliationFieldName.FileFolder => new(cell.Key, ToFileFolder(cell.Value as Uri)),
                ReconciliationFieldName.ImportLocation => new(cell.Key, ToImportLocation(row, cell.Value as string, code, filePrefix)),
                ReconciliationFieldName.VariationName => new(cell.Key, ToVariationName(row, cell.Value as string)),
                ReconciliationFieldName.OriginStartDate => new(cell.Key, ToOriginDate(cell.Value as string)),
                ReconciliationFieldName.OriginEndDate => new(cell.Key, ToOriginDate(cell.Value as string)),
                ReconciliationFieldName.AccessConditionName => new(cell.Key, ToAccessConditon(cell.Value as string)),
                ReconciliationFieldName.SensitivityReviewDuration => new(cell.Key, ToYearDuration(row, cell.Value as TimeSpan?)),
                ReconciliationFieldName.LegislationSectionReference => new(cell.Key, ToLegislationReferences(cell.Value as string)),
                ReconciliationFieldName.SensitivityReviewEndYear => new(cell.Key, cell.Value as int?),
                ReconciliationFieldName.RetentionType => new(cell.Key, ToRetentionType(cell.Value as string)),
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

    private static int? ToOriginDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }
        var dt = date.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (dt.Length == 0)
        {
            return null;
        }
        var sb = new StringBuilder();
        sb.Append(dt[0]);
        if (dt.Length > 1)
        {
            for (var i = 1; i < dt.Length; i++)
            {
                sb.Append(dt[i]);
            }
        }

        return int.TryParse(sb.ToString(), out var origin) ? origin : null;
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
