using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Reconciliation;

public class DataComparison(ILogger<DataComparison> logger, IOptions<ReconciliationSettings> reconciliationSettings,
    IStagingReconciliationClient client, IEnumerable<IReconciliationSource> sources) : IDataComparison
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;

    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        //TODO: use sensitive name in logs
        logger.ReconciliationStarted(settings.MapKind, settings.Code);
        var expected = await GetExpectedDataAsync(cancellationToken);
        logger.ReconciliationRecordCount(expected.Count);

        var summary = new ReconciliationSummary(0, 0, 0, 0, 0);
        List<Dictionary<ReconciliationFieldName, object>> page;
        var offset = 0;
        do
        {
            logger.GetReconciliationRecords(offset);
            page = (await client.FetchAsync(settings.MapKind, settings.Code,
                settings.FetchPageSize, offset, cancellationToken)).ToList();
            offset += settings.FetchPageSize;

            var adjustedStaging = StagingReconciliationParser.Parse(page, settings.MapKind).ToList();
            var pageSummary = CheckRecords(expected, adjustedStaging);
            summary.Update(pageSummary);
        }
        while (page.Any());

        var missingSummary = CheckMissing(expected);
        summary.Update(missingSummary);

        if (summary.HasDifference)
        {
            logger.ReconciliationTotalDiff(summary.AdditionalFilesCount, summary.AdditionalFolderCount,
                summary.MissingFilesCount, summary.MissingFolderCount, summary.DiffCount);
        }
        else
        {
            logger.ReconciliationNoDiff();
        }

        logger.ReconciliationFinished();
    }

    private async Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken)
    {
        var source = settings.MapKind switch
        {
            ReconciliationMapType.Discovery => sources.SingleOrDefault(s => s is DiscoverySource),
            ReconciliationMapType.Closure => sources.SingleOrDefault(s => s is ClosureSource),
            ReconciliationMapType.Metadata => sources.SingleOrDefault(s => s is MetadataSource),
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
        logger.ComparingRecords(staging.Count);
        var additionalFilesCount = 0;
        var additionalFolderCount = 0;
        var diffCount = 0;
        foreach (var stagingRow in staging)
        {
            var stagingIdentifier = GetStagingIdentifier(stagingRow);
            using (logger.BeginScope(("RecordId", stagingIdentifier)))
            {
                var expectedRow = expected.SingleOrDefault(p => SelectIdentifier(p).Equals(SelectIdentifier(stagingRow)));
                var isFolder = stagingRow.ContainsKey(ReconciliationFieldName.FileFolder) &&
                    stagingRow[ReconciliationFieldName.FileFolder] as string == "folder";
                if (expectedRow is null)
                {
                    if (isFolder)
                    {
                        logger.ReconciliationFolderAdditional();
                        additionalFolderCount++;
                    }
                    else
                    {
                        logger.ReconciliationFileAdditional();
                        additionalFilesCount++;
                    }
                    continue;
                }

                var diffs = ReconciliationEqualityComparer.Check(expectedRow!, stagingRow!);
                if (diffs.Any())
                {
                    foreach (var diff in diffs)
                    {
                        var actualValue = stagingRow.TryGetValue(diff, out var value) ? value : "NOT FOUND";
                        logger.ReconciliationDiffDetails(diff, expectedRow[diff], actualValue);
                    }
                    diffCount++;
                }
                expected.Remove(expectedRow);
            }
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
            using (logger.BeginScope(("RecordId", identifier)))
            {
                if (identifier?.EndsWith('/') == true)
                {
                    logger.ReconciliationFolderNotFound();
                    missingFolderCount++;
                }
                else
                {
                    logger.ReconciliationFileNotFound();
                    missingFilesCount++;
                }
            }
        }

        return new ReconciliationSummary(0, 0, missingFilesCount, missingFolderCount, 0);
    }

    private static string GetStagingIdentifier(Dictionary<ReconciliationFieldName, object> stagingRow)
    {
        if (stagingRow.TryGetValue(ReconciliationFieldName.Reference, out var reference) &&
            reference is string r)
        {
            return r;
        }
        if (stagingRow.TryGetValue(ReconciliationFieldName.Location, out var location) &&
            location is string l)
        {
            return l;
        }

        return string.Empty;
    }

    private string SelectIdentifier(Dictionary<ReconciliationFieldName, object> item) =>
        settings.MapKind == ReconciliationMapType.Discovery ?
                (item[ReconciliationFieldName.Id] as string)! :
                (item[ReconciliationFieldName.Location] as string)!;
}
