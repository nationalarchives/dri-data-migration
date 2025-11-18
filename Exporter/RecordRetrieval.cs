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
    private readonly string sparqlJson = new EmbeddedResource(
        typeof(RecordRetrieval).Assembly, $"{typeof(RecordRetrieval).Namespace}.Sparql")
        .GetSparql("ExportJson");
    private readonly string sparqlXml = new EmbeddedResource(
        typeof(RecordRetrieval).Assembly, $"{typeof(RecordRetrieval).Namespace}.Sparql")
        .GetSparql("ExportXml");

    public async Task<IEnumerable<RecordOutput>> GetRecordAsync(int offset, CancellationToken cancellationToken)
    {
        logger.GetRecords(offset);

        var graph = await sparqlClient.GetGraphAsync(sparqlJson, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset }
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.AssetDriId)
            .Select(t => t.Subject as IUriNode)
            .SelectMany(s => MapRecords(graph, s!));
    }

    public async Task<IEnumerable<XmlWrapper>> GetXmlAsync(int offset, CancellationToken cancellationToken)
    {
        logger.GetXmls(offset);

        var graph = await sparqlClient.GetGraphAsync(sparqlXml, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset }
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.AssetReference)
            .Select(t => t.Subject as IUriNode)
            .SelectMany(s => MapXmls(graph, s!));
    }

    private IEnumerable<RecordOutput> MapRecords(IGraph graph, IUriNode subject)
    {
        logger.MappingRecord(subject.Uri);

        var variationSequences = GetVariationSequences(graph, subject);
        var variationGroups = variationSequences.GroupBy(s => s.Sequence, s => s.Variation);

        foreach (var variationGroup in variationGroups)
        {
            var variations = variationGroup.Select(v => v).ToList();
            yield return RecordMapper.Map(graph, subject, variations, variationGroup.Key);
        }
    }

    private List<XmlWrapper> MapXmls(IGraph graph, IUriNode subject)
    {
        logger.MappingRecord(subject.Uri);

        var variationSequences = GetVariationSequences(graph, subject);
        var xmls = new List<XmlWrapper>();

        foreach (var variationSequence in variationSequences)
        {
            xmls.AddRange(XmlMapper.Map(graph, subject, variationSequence.Variation,
                variationSequence.Sequence));
        }

        return xmls;
    }

    private List<VariationSequence> GetVariationSequences(IGraph graph, IUriNode subject)
    {
        logger.MappingRecord(subject.Uri);

        var variations = graph.GetUriNodes(subject, Vocabulary.AssetHasVariation);
        List<VariationSequence> variationSequences = [];
        foreach (var variation in variations)
        {
            var redactedVariationSequence = graph.GetSingleLiteral(variation, Vocabulary.RedactedVariationSequence)
                ?.AsValuedNode().AsInteger();
            variationSequences.Add(new(redactedVariationSequence, variation));
        }

        return variationSequences;
    }

    private record VariationSequence(long? Sequence, IUriNode Variation);
}
