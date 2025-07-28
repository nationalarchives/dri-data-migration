using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchestration;

public class Reconciliation(ILogger<Reconciliation> logger, IOptions<ReconciliationSettings> reconciliationSettings,
    IStagingReconciliationClient client) : IReconciliation
{
    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        logger.ReconciliationStarted(reconciliationSettings.Value.MapKind, reconciliationSettings.Value.FileLocation);

        var map = PreservicaExportMap.GetMap(reconciliationSettings.Value.MapKind);
        //TODO: handle null

        var reconciliation = new StagingReconciliationParser(client);
        var staging = (await reconciliation.ParseAsync(reconciliationSettings.Value.Code,
            reconciliationSettings.Value.FilePrefix, reconciliationSettings.Value.FetchPageSize, cancellationToken))
            .ToList();

        var preservica = new PreservicaExportParser(reconciliationSettings.Value.FileLocation, map);
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

        logger.ReconciliationFinished(reconciliationSettings.Value.MapKind);
    }
}
