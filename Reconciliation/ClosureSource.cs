using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class ClosureSource(ILogger<ClosureSource> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;

    public async Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken)
    {
        var data = new List<Dictionary<ReconciliationFieldName, object>>();
        var ids = new List<string>();
        foreach (var file in settings.FileLocation)
        {
            logger.GetReconciliationFile(file);
            var preservica = PreservicaExportParser.Parse(file);

            data.AddRange(preservica.Select(p => Filter(p).Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value!))
                .Where(d => !ids.Contains(d[ReconciliationFieldName.ImportLocation] as string))
                .ToList());
            ids.AddRange(data.Select(d => d[ReconciliationFieldName.ImportLocation] as string).ToList());
        }

        return data;
    }

    private Dictionary<ReconciliationFieldName, object?> Filter(Dictionary<string, string> data) =>
        new()
        {
            [ReconciliationFieldName.ImportLocation] = PreservicaExportParser.ToLocation(data["identifier"], settings.Code),
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data["folder"]),
            [ReconciliationFieldName.AccessConditionName] = PreservicaExportParser.ToText(data["closure_type"]),
            [ReconciliationFieldName.SensitivityReviewDuration] = PreservicaExportParser.ToInt(data["closure_period"]),
            [ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate] = PreservicaExportParser.ToDate(data["closure_start_date"]),
            [ReconciliationFieldName.LegislationSectionReference] = PreservicaExportParser.ToTextList(data["foi_exemption_code"]),
            [ReconciliationFieldName.SensitivityReviewDate] = PreservicaExportParser.ToDate(data["foi_exemption_asserted"]),
            [ReconciliationFieldName.IsPublicName] = PreservicaExportParser.ToBool(data["title_public"]),
            [ReconciliationFieldName.SensitivityReviewSensitiveName] = PreservicaExportParser.ToText(data["title_alternate"]),
            [ReconciliationFieldName.IsPublicDescription] = PreservicaExportParser.ToBool(data["description_public"]),
            [ReconciliationFieldName.SensitivityReviewSensitiveDescription] = PreservicaExportParser.ToText(data["description_alternate"])
        };
}
