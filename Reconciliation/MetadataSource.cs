using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class MetadataSource(ILogger<MetadataSource> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
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

    private Dictionary<ReconciliationFieldName, object?> Filter(Dictionary<string, string> data) =>
        new()
        {
            [ReconciliationFieldName.Location] = PreservicaExportParser.ToLocation(data["identifier"], settings.Code),
            [ReconciliationFieldName.Name] = PreservicaExportParser.ToName(data["folder"], data["file_name"]),
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data, "folder"),
            [ReconciliationFieldName.ModifiedAt] = PreservicaExportParser.ToDate(data["folder"], data["date_last_modified"])
        };
}
