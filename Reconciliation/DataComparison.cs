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

        var summary = new ReconciliationSummary();
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
            logger.ReconciliationTotalDiff(summary.AdditionalFiles.Count, summary.AdditionalFolders.Count,
                summary.MissingFiles.Count, summary.MissingFolders.Count, summary.DiffDetails.Count);
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
        var additionalFolders = new List<string>();
        var additionalFiles = new List<string>();
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
                        additionalFolders.Add(stagingIdentifier);
                    }
                    else
                    {
                        logger.ReconciliationFileAdditional();
                        additionalFiles.Add(stagingIdentifier);
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
                    recordDiffs.Add(new ReconciliationSummary.Diff(stagingIdentifier, recordDiffDetails));
                }
                expected.Remove(expectedRow);
            }
        }

        return new ReconciliationSummary(recordDiffs, additionalFiles: additionalFiles, additionalFolders: additionalFolders);
    }

    private ReconciliationSummary CheckMissing(List<Dictionary<ReconciliationFieldName, object>> expected)
    {
        logger.FindingMissingRecords();
        var missingFiles = new List<string>();
        var missingFolders = new List<string>();

        foreach (var item in expected)
        {
            var identifier = SelectIdentifier(item);
            var isFolder = item.TryGetValue(ReconciliationFieldName.FileFolder, out var fileFolder) ? 
                fileFolder as string == "folder": identifier?.EndsWith('/') == true;
            using (logger.BeginScope(("RecordId", identifier)))
            {
                if (isFolder)
                {
                    logger.ReconciliationFolderNotFound();
                    missingFolders.Add(identifier!);
                }
                else
                {
                    logger.ReconciliationFileNotFound();
                    missingFiles.Add(identifier!);
                }
            }
        }

        return new ReconciliationSummary(missingFiles: missingFiles, missingFolders: missingFolders);
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
        if (summary.AdditionalFiles.Count > 0)
        {
            logger.AdditionalFilesCount(summary.AdditionalFiles.Count);
            foreach (var missing in summary.AdditionalFiles)
            {
                logger.AdditionalRecord(missing);
            }
        }
        if (summary.AdditionalFolders.Count > 0)
        {
            logger.AdditionalFoldersCount(summary.AdditionalFolders.Count);
            foreach (var missing in summary.AdditionalFolders)
            {
                logger.AdditionalRecord(missing);
            }
        }
    }
}
