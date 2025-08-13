using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class Comparer(ILogger<Comparer> logger, IOptions<ReconciliationSettings> reconciliationSettings,
    IStagingReconciliationClient client, IEnumerable<IReconciliationSource> sources) : IReconciliation
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;
    private const string missing = "MISSING IMPORT LOCATION";

    public async Task<ReconciliationSummary> ReconcileAsync(CancellationToken cancellationToken)
    {
        //TODO: use sensitive name in logs
        logger.ReconciliationStarted(settings.MapKind);
        var expected = await GetExpectedDataAsync(cancellationToken);

        ReconciliationSummary summary = new(0, 0, 0, 0, 0);
        IEnumerable<Dictionary<ReconciliationFieldName, object>> page;
        var offset = 0;
        do
        {
            logger.GetReconciliationRecords(offset);
            page = await client.FetchAsync(settings.Code,
                settings.FetchPageSize, offset, cancellationToken);
            offset += settings.FetchPageSize;

            var adjustedStaging = StagingReconciliationParser.Parse(page, settings.Code,
                settings.FilePrefix, settings.MapKind).ToList();
            var pageSummary = CheckRecords(expected, adjustedStaging);
            summary.Update(pageSummary);
        }
        while (page.Any() && page.Count() == settings.FetchPageSize);

        var missingSummary = CheckMissing(expected);
        summary.Update(missingSummary);

        if (summary.HasDifference)
        {
            logger.ReconciliationTotalDiff(summary.AdditionalFilesCount, summary.AdditionalFolderCount,
                summary.MissingFilesCount, summary.MissingFolderCount, summary.DiffCount);
        }

        logger.ReconciliationFinished(settings.MapKind);

        return summary;
    }

    private async Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken)
    {
        var source = settings.MapKind switch
        {
            MapType.Discovery => sources.SingleOrDefault(s => s is DiscoveryRecord),
            MapType.Closure => sources.SingleOrDefault(s => s is PreservicaClosure),
            MapType.Metadata => sources.SingleOrDefault(s => s is PreservicaMetadata),
            _ => null
        };

        if (source is null)
        {
            logger.UnableFindSource();
            throw new MigrationException();
        }

        return await source.GetExpectedDataAsync(cancellationToken);
    }

    private ReconciliationSummary CheckRecords(List<Dictionary<ReconciliationFieldName, object>> expected,
        List<Dictionary<ReconciliationFieldName, object>> staging)
    {
        logger.ComparingRecords();
        var additionalFilesCount = 0;
        var additionalFolderCount = 0;
        var diffCount = 0;
        foreach (var stagingRow in staging)
        {
            var stagingImportLocation = (stagingRow[ReconciliationFieldName.ImportLocation] as string)!;
            var stagingIdentifier = stagingRow[ReconciliationFieldName.Reference] as string ?? stagingImportLocation;
            var expectedRow = expected.SingleOrDefault(p => SelectIdentifier(p).Equals(SelectIdentifier(stagingRow)));

            if (expectedRow is null)
            {
                if (stagingImportLocation.EndsWith('/'))
                {
                    logger.ReconciliationFolderAdditional(stagingIdentifier);
                    additionalFolderCount++;
                }
                else
                {
                    logger.ReconciliationFileAdditional(stagingIdentifier);
                    additionalFilesCount++;
                }
                continue;
            }

            var diffs = ReconciliationEqualityComparer.Check(expectedRow!, stagingRow!);
            if (diffs.Any())
            {
                logger.ReconciliationDiff(stagingIdentifier, diffs);
                diffCount++;
            }
            expected.Remove(expectedRow);
        }

        return new ReconciliationSummary(additionalFilesCount, additionalFolderCount, 0, 0, diffCount);
    }

    private ReconciliationSummary CheckMissing(List<Dictionary<ReconciliationFieldName, object>> expected)
    {
        logger.FindingMissingRecords();
        var missingFilesCount = 0;
        var missingFolderCount = 0;

        foreach (var item in expected)
        {
            var identifier = SelectIdentifier(item);

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

    private string? SelectIdentifier(Dictionary<ReconciliationFieldName, object> item) =>
        settings.MapKind == MapType.Discovery ?
                item[ReconciliationFieldName.Reference] as string :
                item[ReconciliationFieldName.ImportLocation] as string; //TODO: handle null
}
