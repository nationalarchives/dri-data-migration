using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class MetadataSource(ILogger<MetadataSource> logger, IOptions<ReconciliationSettings> reconciliationSettings) : ExcelSource(logger), IReconciliationSource
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;

    public Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken) =>
        GetDataAsync(settings.FileLocation, "metadata");

    internal override Dictionary<ReconciliationFieldName, object?> Filter(Dictionary<string, string> data)
    {
        var isFolder = data["folder"] == "folder";
        return new()
        {
            [ReconciliationFieldName.Location] = PreservicaExportParser.ToLocation(data["identifier"], settings.Code),
            [ReconciliationFieldName.Name] = isFolder ? null : data["file_name"],
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data, "folder"),
            [ReconciliationFieldName.ModifiedAt] = isFolder ? null : PreservicaExportParser.ToDate(data, "date_last_modified"),
            [ReconciliationFieldName.CoveringDateEnd] = isFolder ? null : PreservicaExportParser.ToIntDate(data, "end_date")
        };
    }
}
