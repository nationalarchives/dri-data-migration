using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchestration;

public class Reconciliation(ILogger<Reconciliation> logger, IStagingReconciliationClient client, IOptions<StagingSettings> settings)
{
    public async Task ReconcileAsync(string code, string prefix, string fileLocation, PreservicaExportMap.MapType mapType)
    {
        logger.ReconciliationStarted(mapType, fileLocation);

        var map = PreservicaExportMap.GetMap(mapType);
        //TODO: handle null

        var reconciliation = new StagingReconciliationParser(client);
        var staging = (await reconciliation.ParseAsync(code, prefix, settings.Value.FetchPageSize)).ToList();

        var preservica = new PreservicaExportParser(fileLocation, map);
        var excel = preservica.Parse();

        var totalDiff = 0;
        foreach (var excelRow in excel)
        {
            if (excelRow is null)
            {
                continue;
            }

            var identifier = excelRow[ReconciliationFieldName.ImportLocation] as string;
            //TODO: handle null and not existing

            var stagingRow = staging.FirstOrDefault(item => item[ReconciliationFieldName.ImportLocation].Equals(identifier));
            if (stagingRow is null)
            {
                logger.ReconciliationNotFound(identifier);
                totalDiff++;
                continue;
            }
            var diffs = ReconciliationEqualityComparer.Check(excelRow!, stagingRow!);
            if (diffs.Any())
            {
                logger.ReconciliationDiff(identifier, diffs);
                totalDiff++;
            }
            staging.Remove(stagingRow);
        }
        foreach (var item in staging)
        {
            var additionalId = item[ReconciliationFieldName.ImportLocation] as string;
            //TODO: handle null and not existing

            logger.ReconciliationAdditional(additionalId);
            totalDiff++;
        }

        logger.ReconciliationFinished(mapType);
    }
}
