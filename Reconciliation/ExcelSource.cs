using Microsoft.Extensions.Logging;

namespace Reconciliation;

public abstract class ExcelSource(ILogger logger)
{
    internal Task<List<Dictionary<ReconciliationFieldName, object>>> GetDataAsync(
        IEnumerable<string> fileLocation, string filePattern)
    {
        var data = new List<Dictionary<ReconciliationFieldName, object>>();
        var ids = new List<string>();
        foreach (var location in fileLocation)
        {
            var files = new string[] { location };
            if (Directory.Exists(location))
            {
                files = Directory.GetFiles(location, $"*{filePattern}*.csv", SearchOption.AllDirectories);
            }
            foreach (var file in files)
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
        }

        return Task.FromResult(data);
    }

    internal abstract Dictionary<ReconciliationFieldName, object?> Filter(Dictionary<string, string> data);
}
