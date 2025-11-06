using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class MetadataSource(ILogger<MetadataSource> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
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
            [ReconciliationFieldName.VariationName] = PreservicaExportParser.ToText(data["file_name"]),
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data["folder"])
        };
}
