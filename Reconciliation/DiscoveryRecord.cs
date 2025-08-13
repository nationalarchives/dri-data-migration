﻿using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;

namespace Reconciliation;

public class DiscoveryRecord(HttpClient httpClient, ILogger<DiscoveryRecord> logger, IOptions<ReconciliationSettings> reconciliationSettings) : IReconciliationSource
{
    private readonly ReconciliationSettings settings = reconciliationSettings.Value;

    public async Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken)
    {
        logger.GetDiscoveryRecords(settings.SearchRecordUri);
        List<Dictionary<ReconciliationFieldName, object>> parsed = [];

        var batchStartMark = "*";
        do
        {
            var response = await GetRecordsAsync(settings.SearchRecordUri, settings.Code, batchStartMark, cancellationToken);
            if (response is not null)
            {
                batchStartMark = response.NextBatchMark;
                parsed.AddRange(Filter(response.Records));
            }
            else
            {
                batchStartMark = null;
            }
        } while (!string.IsNullOrWhiteSpace(batchStartMark));

        return parsed;
    }

    private async Task<DiscoverySearchRecordsResponse?> GetRecordsAsync(Uri searchRecordsUri, string code, string batchStartMark, CancellationToken cancellationToken)
    {
        logger.GetDiscoveryRecordsPage(batchStartMark);
        var builder = new UriBuilder(searchRecordsUri)
        {
            Query = $"sps.recordSeries={code}&sps.batchStartMark={batchStartMark}&sps.resultsPageSize=1000&sps.sortByOption=REFERENCE_ASCENDING&sps.catalogueLevels=Level6"
        };

        return await httpClient.GetFromJsonAsync<DiscoverySearchRecordsResponse>(builder.Uri, cancellationToken);
    }

    private static IEnumerable<Dictionary<ReconciliationFieldName, object>> Filter(DiscoverySearchRecordsResponse.Record[] records) =>
        records.Select(r => new Dictionary<ReconciliationFieldName, object?>()
        {
            //[ReconciliationFieldName.Id] = ToId(r.Id),
            //[ReconciliationFieldName.VariationName] = r.Title,
            [ReconciliationFieldName.Reference] = r.Reference?.Replace(' ', '/'),
            [ReconciliationFieldName.AccessConditionCode] = r.ClosureType,
            [ReconciliationFieldName.SensitivityReviewDuration] = ToDuration(r.ClosureType, r.ClosureCode),
            [ReconciliationFieldName.SensitivityReviewEndYear] = ToEndYear(r.ClosureType, r.ClosureCode),
            //[ReconciliationFieldName.SensitivityReviewRestrictionReviewDate] = ToDate(r.OpeningDate),
            //[ReconciliationFieldName.IsPublicName] = r.ClosureStatus,
            //[ReconciliationFieldName.IsPublicDescription] = r.ClosureStatus,
            //[ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate] = ToDate(r.StartDate),
        }).Select(d => d.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value!));

    private static readonly string[] YearDuration = ["D", "U"];
    private static Guid? ToId(string? txt) => Guid.TryParse(txt, out Guid v) ? v : null;
    private static int? ToDuration(string? closureType, string? txt) =>
        int.TryParse(txt, out int v) && closureType is not null ? !YearDuration.Contains(closureType) ? v : null : null;
    private static int? ToEndYear(string? closureType, string? txt) =>
        int.TryParse(txt, out int v) && closureType is not null ? YearDuration.Contains(closureType) ? v : null : null;
    private static DateTimeOffset? ToDate(string? txt) =>
        DateTimeOffset.TryParseExact(txt, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var v) ? v : null;
}
