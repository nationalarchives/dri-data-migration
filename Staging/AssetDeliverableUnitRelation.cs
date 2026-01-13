using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rdf;
using System.Net.Http.Json;
using VDS.RDF;

namespace Staging;

internal class AssetDeliverableUnitRelation(HttpClient httpClient,
    ILogger<AssetDeliverableUnitRelation> logger, IOptions<StagingSettings> settings) : IAssetDeliverableUnitRelation
{
    public async Task AddAssetRelationAsync(IGraph graph, IGraph rdf, IUriNode id, ICacheClient cacheClient, CancellationToken cancellationToken)
    {
        var relatedId = rdf.GetSingleText(IngestVocabulary.RelatedIaid);
        if (!string.IsNullOrWhiteSpace(relatedId))
        {
            GraphAssert.Text(graph, id, relatedId, Vocabulary.AssetRelationIdentifier);
            var reference = cacheClient.CacheFetch(CacheEntityKind.AssetRelation, relatedId) as string;
            if (reference is null)
            {
                reference = await GetReferenceAsync(relatedId, cancellationToken);
                if (reference is not null)
                {
                    cacheClient.CacheCreate(CacheEntityKind.AssetRelation, relatedId, reference);
                }
            }
            if (reference is not null)
            {
                GraphAssert.Text(graph, id, reference, Vocabulary.AssetRelationReference);
            }
        }
    }

    private async Task<string?> GetReferenceAsync(string relatedId, CancellationToken cancellationToken)
    {
        var uri = new Uri(settings.Value.DetailRecordUri, relatedId);
        try
        {
            var response = await httpClient.GetFromJsonAsync<RecordDetailResponse>(uri, cancellationToken);
            if (response?.CitableReference is null)
            {
                logger.RelatedIdNotFound(relatedId);
            }
            return response?.CitableReference;
        }
        catch (Exception e)
        {
            logger.RelatedIdNotResolved(relatedId);
            logger.RelatedIdResolutionFailed(e);
        }

        return null;
    }

    internal class RecordDetailResponse
    {
        public string? CitableReference { get; set; }
    }
}
