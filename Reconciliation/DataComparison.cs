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
        if (settings.MapKind == ReconciliationMapType.Metadata)
        {
            logger.MetadataReconciliationInfo();
        }
        else if (settings.MapKind == ReconciliationMapType.Closure)
        {
            logger.ClosureReconciliationInfo();
        }
        else
        {
            logger.DiscoveryReconciliationInfo();
        }
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
        PrintReconciliationSummary(summary);
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
        var recordDiffs = new List<ReconciliationSummary.Diff>();
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

                var diffFields = ReconciliationEqualityComparer.Check(expectedRow!, stagingRow!);
                if (diffFields.Any())
                {
                    var recordDiffDetails = new List<ReconciliationSummary.DiffDetail>();
                    foreach (var diffField in diffFields)
                    {
                        var actualValue = stagingRow.TryGetValue(diffField, out var value) ? value : "NOT FOUND";
                        logger.ReconciliationDiffDetails(diffField, expectedRow[diffField], actualValue);
                        recordDiffDetails.Add(new ReconciliationSummary.DiffDetail(diffField, expectedRow[diffField], actualValue));
                    }
                    diffCount++;
                    recordDiffs.Add(new ReconciliationSummary.Diff(stagingIdentifier, recordDiffDetails));
                }
                expected.Remove(expectedRow);
            }
        }

        return new ReconciliationSummary(additionalFilesCount, additionalFolderCount, 0, 0, diffCount, recordDiffs);
    }

    private ReconciliationSummary CheckMissing(List<Dictionary<ReconciliationFieldName, object>> expected)
    {
        logger.FindingMissingRecords();
        var missingFilesCount = 0;
        var missingFolderCount = 0;
        var missingFiles = new List<string>();
        var missingFolders = new List<string>();

        foreach (var item in expected)
        {
            var identifier = SelectIdentifier(item);
            using (logger.BeginScope(("RecordId", identifier)))
            {
                if (identifier?.EndsWith('/') == true)
                {
                    logger.ReconciliationFolderNotFound();
                    missingFolderCount++;
                    missingFolders.Add(identifier);
                }
                else
                {
                    logger.ReconciliationFileNotFound();
                    missingFilesCount++;
                    missingFiles.Add(identifier ?? "NOT FOUND");
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

    private void PrintReconciliationSummary(ReconciliationSummary summary)
    {
        if (summary.DiffDetails.Count > 0)
        {
            logger.DiffCount(summary.DiffDetails.Count);
            foreach (var record in summary.DiffDetails)
            {
                logger.DiffRecord(record.Id);
                foreach (var diff in record.Details)
                {
                    logger.DiffDetails(diff.Field, diff.Expected, diff.Actual);
                }
            }
        }
        if (summary.MissingFiles.Count > 0)
        {
            logger.MissingFilesCount(summary.MissingFiles.Count);
            foreach (var missing in summary.MissingFiles)
            {
                logger.MissingRecord(missing);
            }
        }
        if (summary.MissingFolders.Count > 0)
        {
            logger.MissingFoldersCount(summary.MissingFolders.Count);
            foreach (var missing in summary.MissingFolders)
            {
                logger.MissingRecord(missing);
            }
        }
    }
}
