using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class MetadataSource(ILogger<MetadataSource> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
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
            [ReconciliationFieldName.ImportLocation] = PreservicaExportParser.ToLocation(data["identifier"], settings.Code),
            [ReconciliationFieldName.VariationName] = PreservicaExportParser.ToText(data["file_name"]),
            [ReconciliationFieldName.FileFolder] = PreservicaExportParser.ToText(data["folder"])
        };
}
