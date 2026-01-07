using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class ClosureSource(ILogger<ClosureSource> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;

    public Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken)
    {
        var data = new List<Dictionary<ReconciliationFieldName, object>>();
        var ids = new List<string>();
        foreach (var file in settings.FileLocation)
        {
            logger.GetReconciliationFile(file);
            var preservica = PreservicaExportParser.Parse(file);
            var filteredData = preservica.Select(p => Filter(p)
                .Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value!))
                .Where(d =>
                {
                    if (!d.TryGetValue(ReconciliationFieldName.Location, out var location) ||
                        location is not string id)
                    {
                        return false;
                    }
                    return !ids.Contains(id);
                })
                .ToList();
            data.AddRange(filteredData);
            ids.AddRange(filteredData.Select(d => (d[ReconciliationFieldName.Location] as string)!).ToList());
        }

        return Task.FromResult(data);
    }

    private Dictionary<ReconciliationFieldName, object?> Filter(Dictionary<string, string> data)
    {
        var isFolder = data["folder"] == "folder";
        return new()
        {
            [ReconciliationFieldName.Location] = PreservicaExportParser.ToLocation(data["identifier"], settings.Code),
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data, "folder"),
            [ReconciliationFieldName.AccessConditionName] = isFolder ? null : PreservicaExportParser.ToText(data, "closure_type"),
            [ReconciliationFieldName.RetentionType] = isFolder ? null : PreservicaExportParser.ToText(data, "retention_type"),
            [ReconciliationFieldName.ClosurePeriod] = isFolder ? null : PreservicaExportParser.ToInt(data, "closure_period"),
            [ReconciliationFieldName.ClosureStartDate] = isFolder ? null : PreservicaExportParser.ToDate(data, "closure_start_date"),
            [ReconciliationFieldName.FoiExemptionReference] = isFolder ? null : PreservicaExportParser.ToTextList(data, "foi_exemption_code"),
            [ReconciliationFieldName.FoiAssertedDate] = isFolder ? null : PreservicaExportParser.ToDate(data, "foi_exemption_asserted"),
            [ReconciliationFieldName.InstrumentNumber] = isFolder ? null : PreservicaExportParser.ToText(data, "RI_number"),
            [ReconciliationFieldName.InstrumentSignedDate] = isFolder ? null : PreservicaExportParser.ToText(data, "RI_signed_date"),
            [ReconciliationFieldName.GroundForRetentionCode] = isFolder ? null : PreservicaExportParser.ToText(data, "retention_justification"),
            [ReconciliationFieldName.IsPublicName] = PreservicaExportParser.ToBool(data, "title_public"),
            [ReconciliationFieldName.SensitiveName] = PreservicaExportParser.ToText(data, "title_alternate"),
            [ReconciliationFieldName.IsPublicDescription] = PreservicaExportParser.ToBool(data, "description_public"),
            [ReconciliationFieldName.SensitiveDescription] = PreservicaExportParser.ToText(data, "description_alternate")
        };
    }
}
