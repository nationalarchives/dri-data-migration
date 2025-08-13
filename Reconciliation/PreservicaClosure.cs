using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class PreservicaClosure(ILogger<PreservicaClosure> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;

    public async Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken)
    {
        logger.GetReconciliationFile(settings.FileLocation);
        var preservica = PreservicaExportParser.Parse(settings.FileLocation);

        return preservica.Select(p => Filter(p).Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value!)).ToList();
    }

    private Dictionary<ReconciliationFieldName, object?> Filter(Dictionary<string, string> data) =>
        new()
        {
            [ReconciliationFieldName.ImportLocation] = PreservicaExportParser.ToLocation(data["identifier"]),
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data["folder"]),
            [ReconciliationFieldName.AccessConditionName] = PreservicaExportParser.ToText(data["closure_type"]),
            [ReconciliationFieldName.SensitivityReviewDuration] = PreservicaExportParser.ToInt(data["closure_period"]),
            [ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate] = PreservicaExportParser.ToDate(data["closure_start_date"]),
            [ReconciliationFieldName.LegislationSectionReference] = PreservicaExportParser.ToTextList(data["foi_exemption_code"]),
            [ReconciliationFieldName.SensitivityReviewDate] = PreservicaExportParser.ToDate(data["foi_exemption_asserted"]),
            [ReconciliationFieldName.IsPublicName] = PreservicaExportParser.ToBool(data["title_public"]),
            [ReconciliationFieldName.SensitivityReviewSensitiveName] = PreservicaExportParser.ToText(data["title_alternate"])
        };
}
