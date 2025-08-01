using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchestration;

public class Reconciliation(ILogger<Reconciliation> logger, IOptions<ReconciliationSettings> reconciliationSettings,
    IStagingReconciliationClient client) : IReconciliation
{
    private const string missing = "MISSING IMPORT LOCATION";

    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {//TODO: use sensitive name in logs
        logger.ReconciliationStarted(reconciliationSettings.Value.MapKind);
        var preservica = GetPreservicaData().ToList();

        ReconciliationSummary summary = new(0, 0, 0, 0, 0);
        IEnumerable<Dictionary<ReconciliationFieldName, object>> page;
        int offset = 0;
        do
        {
            logger.GetReconciliationRecords(offset);
            page = await client.FetchAsync(reconciliationSettings.Value.Code,
                reconciliationSettings.Value.FetchPageSize, offset, cancellationToken);
            offset += reconciliationSettings.Value.FetchPageSize;

            var adjustedStaging = StagingReconciliationParser.Parse(page, reconciliationSettings.Value.Code,
                reconciliationSettings.Value.FilePrefix).ToList();
            var pageSummary = CheckRecords(preservica, adjustedStaging);
            summary.Update(pageSummary);
        }
        while (page.Any() && page.Count() == reconciliationSettings.Value.FetchPageSize);

        var missingSummary = CheckMissing(preservica);
        summary.Update(missingSummary);

        if (summary.HasDifference)
        {
            logger.ReconciliationTotalDiff(summary.AdditionalFilesCount, summary.AdditionalFolderCount,
                summary.MissingFilesCount, summary.MissingFolderCount, summary.DiffCount);
        }

        logger.ReconciliationFinished(reconciliationSettings.Value.MapKind);
    }

    private IEnumerable<Dictionary<ReconciliationFieldName, object>> GetPreservicaData()
    {
        var map = PreservicaExportMap.GetMap(reconciliationSettings.Value.MapKind);
        if (map is null)
        {
            logger.InvalidMapType();
            throw new MigrationException();
        }
        logger.GetReconciliationFile(reconciliationSettings.Value.FileLocation);
        var preservica = PreservicaExportParser.Parse(reconciliationSettings.Value.FileLocation, map);

        return preservica.Where(p => p != null).Select(p => p!);
    }

    private ReconciliationSummary CheckRecords(List<Dictionary<ReconciliationFieldName, object>> preservica,
        List<Dictionary<ReconciliationFieldName, object>> staging)
    {
        var additionalFilesCount = 0;
        var additionalFolderCount = 0;
        var diffCount = 0;
        foreach (var stagingRow in staging)
        {
            var stagingIdentifier = stagingRow[ReconciliationFieldName.ImportLocation] as string;
            var preservicaRow = preservica.SingleOrDefault(p => p[ReconciliationFieldName.ImportLocation].Equals(stagingIdentifier));

            if (preservicaRow is null)
            {
                if (stagingIdentifier?.EndsWith('/') == true)
                {
                    logger.ReconciliationFolderAdditional(stagingIdentifier);
                    additionalFolderCount++;
                }
                else
                {
                    logger.ReconciliationFileAdditional(stagingIdentifier ?? missing);
                    additionalFilesCount++;
                }
                continue;
            }

            var identifier = preservicaRow[ReconciliationFieldName.ImportLocation] as string;

            var diffs = ReconciliationEqualityComparer.Check(preservicaRow!, stagingRow!);
            if (diffs.Any())
            {
                logger.ReconciliationDiff(identifier ?? missing, diffs);
                diffCount++;
            }
            preservica.Remove(preservicaRow);
        }

        return new ReconciliationSummary(additionalFilesCount, additionalFolderCount, 0, 0, diffCount);
    }

    private ReconciliationSummary CheckMissing(List<Dictionary<ReconciliationFieldName, object>> preservica)
    {
        var missingFilesCount = 0;
        var missingFolderCount = 0;

        foreach (var item in preservica)
        {
            var identifier = item[ReconciliationFieldName.ImportLocation] as string;

            if (identifier?.EndsWith('/') == true)
            {
                logger.ReconciliationFolderNotFound(identifier);
                missingFolderCount++;
            }
            else
            {
                logger.ReconciliationFileNotFound(identifier ?? missing);
                missingFilesCount++;
            }
        }

        return new ReconciliationSummary(0, 0, missingFilesCount, missingFolderCount, 0);
    }

    private sealed class ReconciliationSummary
    {
        public int AdditionalFilesCount { get; set; }
        public int AdditionalFolderCount { get; set; }
        public int MissingFilesCount { get; set; }
        public int MissingFolderCount { get; set; }
        public int DiffCount { get; set; }

        internal ReconciliationSummary(int additionalFilesCount, int additionalFolderCount,
            int missingFilesCount, int missingFolderCount, int diffCount)
        {
            AdditionalFilesCount = additionalFilesCount;
            AdditionalFolderCount = additionalFolderCount;
            MissingFilesCount = missingFilesCount;
            MissingFolderCount = missingFolderCount;
            DiffCount = diffCount;
        }

        internal void Update(ReconciliationSummary summary)
        {
            AdditionalFilesCount += summary.AdditionalFilesCount;
            AdditionalFolderCount += summary.AdditionalFolderCount;
            MissingFilesCount += summary.MissingFilesCount;
            MissingFolderCount += summary.MissingFolderCount;
            DiffCount += summary.DiffCount;
        }

        internal bool HasDifference => AdditionalFilesCount > 0 || AdditionalFolderCount > 0 ||
            MissingFilesCount > 0 || MissingFolderCount > 0 || DiffCount > 0;
    }
}
