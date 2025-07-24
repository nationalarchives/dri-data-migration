using Api;

namespace Orchestration;

public class StagingReconciliationParser(IStagingReconciliationClient reconciliationClient, string code, string filePrefix)
{
    private const string folder = "folder";
    private const string file = "file";

    public async Task<IEnumerable<Dictionary<ReconciliationFieldName, object>>> ParseAsync()
    {
        int offset = 0;
        int pageSize = 1000;
        IEnumerable<Dictionary<ReconciliationFieldName, object>> page;
        var rows = new List<Dictionary<ReconciliationFieldName, object>>();
        do
        {
            page = await reconciliationClient.FetchAsync(code, pageSize, offset);
            offset += pageSize;

            rows.AddRange(page.Select(Adjust));

        } while (page.Any() && page.Count() == pageSize);

        return rows;
    }

    private Dictionary<ReconciliationFieldName, object> Adjust(Dictionary<ReconciliationFieldName, object> row) =>
        row.Select(cell =>
            cell.Key switch
            {
                ReconciliationFieldName.FileFolder => new(cell.Key, ToFileFolder(cell.Value as Uri)),
                ReconciliationFieldName.ImportLocation => new(cell.Key, ToImportLocation(row[ReconciliationFieldName.FileFolder] as Uri, cell.Value as string)),
                ReconciliationFieldName.VariationName => new(cell.Key, ToVariationName(row[ReconciliationFieldName.FileFolder] as Uri, cell.Value as string)),
                ReconciliationFieldName.AccessConditionName => new(cell.Key, ToAccessConditon(cell.Value as string)),
                ReconciliationFieldName.SensitivityReviewDuration => new(cell.Key, ToYearDuration(row, cell.Value as string)),
                ReconciliationFieldName.LegislationSectionReference => new(cell.Key, ToLegislationReferences(cell.Value as string)),
                ReconciliationFieldName.SensitivityReviewEndYear => new(cell.Key, null),
                ReconciliationFieldName.RetentionType => new(cell.Key, ToRetentionType(cell.Value as string)),
                _ => cell
            })
            .Where(kv => kv.Value is not null)
            .ToDictionary();

    private static string? ToFileFolder(Uri? subject) =>
        subject == Vocabulary.Subset.Uri ? folder :
            subject == Vocabulary.Variation.Uri ? file : null;

    private string? ToImportLocation(Uri? fileFolder, string? importLocation)
    {
        var replaced = importLocation?.Replace(code, filePrefix);
        if (replaced is not null && fileFolder == Vocabulary.Subset.Uri && replaced.Last() != '/')
        {
            replaced = $"{replaced}/";
        }

        return replaced;
    }

    private static string? ToVariationName(Uri? fileFolder, string? variationName) =>
        fileFolder is not null && fileFolder == Vocabulary.Subset.Uri ? variationName?.Split('/').Last() : variationName;

    private static string? ToAccessConditon(string? accessConditionName) => accessConditionName?.Replace(' ', '_');

    private static int? ToYearDuration(Dictionary<ReconciliationFieldName, object> row, string? duration) =>
        row.TryGetValue(ReconciliationFieldName.SensitivityReviewEndYear, out var endYear) && endYear is not null ? (int)endYear :
            duration is null ? null : XsdDurationYear(duration);

    private static int XsdDurationYear(string duration) => (int)Math.Floor(System.Xml.XmlConvert.ToTimeSpan(duration).TotalDays / 365);

    private static string[] ToLegislationReferences(string? references) => string.IsNullOrWhiteSpace(references) ? ["open"] : references.Split(',', StringSplitOptions.RemoveEmptyEntries);

    private static string? ToRetentionType(string? accessConditionName) =>
        accessConditionName switch
        {
            "retained by department under section 3.4" => "retained_under_3.4",
            (string acn) when acn.Contains("retained") => acn.Replace(' ', '_'),
            _ => accessConditionName
        };
}
