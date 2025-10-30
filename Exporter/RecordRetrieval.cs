using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

public class RecordRetrieval(ILogger<RecordRetrieval> logger, IOptions<ExportSettings> options,
    IExportSparqlClient sparqlClient) : IRecordRetrieval
{
    private readonly ExportSettings settings = options.Value;
    private readonly string sparql = new EmbeddedResource(
        typeof(RecordRetrieval).Assembly, $"{typeof(RecordRetrieval).Namespace}.Sparql")
        .GetSparql("Export");

    public async Task<IEnumerable<RecordOutput>> GetAsync(int offset, CancellationToken cancellationToken)
    {
        logger.GetRecords(offset);

        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset }
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.AssetDriId)
            .Select(t => t.Subject as IUriNode)
            .SelectMany(s => MapRecords(graph, s!));
    }

    private IEnumerable<RecordOutput> MapRecords(IGraph graph, IUriNode subject)
    {
        logger.MappingRecord(subject.Uri);

        var variations = graph.GetUriNodes(subject, Vocabulary.AssetHasVariation);
        //ValueTuple used as a workaround for a null key
        Dictionary<ValueTuple<long?>, List<IUriNode>> variationGroups = [];
        foreach (var variation in variations)
        {
            var redactedVariationSequence = graph.GetSingleLiteral(variation, Vocabulary.RedactedVariationSequence)
                ?.AsValuedNode().AsInteger();
            var key = ValueTuple.Create(redactedVariationSequence);
            if (!variationGroups.ContainsKey(key))
            {
                variationGroups.Add(key, []);
            }
            variationGroups[key].Add(variation);
        }

        foreach (var variationGroup in variationGroups)
        {
            yield return RecordMapper.Map(graph, subject, variationGroup.Value,
                variationGroup.Key.Item1);
        }
    }
}
