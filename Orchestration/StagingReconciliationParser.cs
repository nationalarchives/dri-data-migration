using AngleSharp.Common;
using Api;

namespace Orchestration;

public class StagingReconciliationParser(IStagingReconciliationClient reconciliationClient, string code, string filePrefix)
{
    private const string folder = "folder";
    private const string file = "file";

    public async Task<IEnumerable<Dictionary<ReconciliationFieldNames, object>>> ParseAsync()
    {
        int offset = 0;
        int pageSize = 1000;
        IEnumerable<Dictionary<ReconciliationFieldNames, object>> page;
        var rows = new List<Dictionary<ReconciliationFieldNames, object>>();
        do
        {
            page = await reconciliationClient.FetchAsync(code, pageSize, offset);
            offset += pageSize;

            rows.AddRange(page.Select(Adjust));

        } while (page.Any() && page.Count() == pageSize);

        return rows;
    }

    private Dictionary<ReconciliationFieldNames, object> Adjust(Dictionary<ReconciliationFieldNames, object> row) =>
        row.Select(cell =>
            cell.Key switch
            {
                ReconciliationFieldNames.FileFolder => new(cell.Key, ToFileFolder(cell.Value as Uri)),
                ReconciliationFieldNames.ImportLocation => new(cell.Key, ToImportLocation(row[ReconciliationFieldNames.FileFolder] as Uri, cell.Value as string)),
                ReconciliationFieldNames.VariationName => new(cell.Key, ToVariationName(row[ReconciliationFieldNames.FileFolder] as Uri, cell.Value as string)),
                ReconciliationFieldNames.AccessConditionName => new(cell.Key, ToAccessConditon(cell.Value as string)),
                ReconciliationFieldNames.SensitivityReviewDuration => new(cell.Key, ToYearDuration(row, cell.Value as string)),
                ReconciliationFieldNames.LegislationSectionReference => new(cell.Key, ToLegislationReferences(cell.Value as string)),
                ReconciliationFieldNames.SensitivityReviewEndYear => new(cell.Key, null),
                ReconciliationFieldNames.RetentionType => new(cell.Key, ToRetentionType(cell.Value as string)),
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

    private static int? ToYearDuration(Dictionary<ReconciliationFieldNames, object> row, string? duration) =>
        row.TryGetValue(ReconciliationFieldNames.SensitivityReviewEndYear, out var endYear) && endYear is not null ? (int)endYear :
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
