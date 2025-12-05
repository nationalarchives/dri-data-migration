using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

public class RecordRetrieval(ILogger<RecordRetrieval> logger, IExportSparqlClient sparqlClient) : IRecordRetrieval
{
    private readonly string sparqlList = new EmbeddedResource(
        typeof(RecordRetrieval).Assembly, $"{typeof(RecordRetrieval).Namespace}.Sparql")
        .GetSparql("List");
    private readonly string sparqlJson = new EmbeddedResource(
        typeof(RecordRetrieval).Assembly, $"{typeof(RecordRetrieval).Namespace}.Sparql")
        .GetSparql("ExportJson");
    private readonly string sparqlXml = new EmbeddedResource(
        typeof(RecordRetrieval).Assembly, $"{typeof(RecordRetrieval).Namespace}.Sparql")
        .GetSparql("ExportXml");

    public async Task<IEnumerable<IUriNode>?> GetListAsync(string code, CancellationToken cancellationToken)
    {
        logger.GetRecordList();

        var list = await sparqlClient.GetResultSetAsync(sparqlList, code, cancellationToken);

        return list?.Results.Select(r => r.Value("asset")).Cast<IUriNode>();
    }

    public async Task<IEnumerable<RecordOutput>> GetRecordAsync(IUriNode id, CancellationToken cancellationToken)
    {
        var asset = await sparqlClient.GetGraphAsync(sparqlJson, id.Uri, cancellationToken);

        return MapRecords(asset, id);
    }

    public async Task<IEnumerable<XmlWrapper>> GetXmlAsync(IUriNode id, CancellationToken cancellationToken)
    {
        var graph = await sparqlClient.GetGraphAsync(sparqlXml, id.Uri, cancellationToken);

        return MapXmls(graph, id);
    }

    private IEnumerable<RecordOutput> MapRecords(IGraph asset, IUriNode subject)
    {
        var variationSequences = GetVariationSequences(asset, subject);
        var variationGroups = variationSequences.GroupBy(s => s.Sequence, s => s.Variation);

        foreach (var variationGroup in variationGroups)
        {
            var variations = variationGroup.Select(v => v).ToList();
            RecordOutput? recordOutput = null;
            try
            {
                recordOutput = RecordMapper.Map(asset, variations, variationGroup.Key);
            }
            catch (Exception e)
            {
                logger.UnableRecordMap(subject.Uri);
                logger.RecordMappingProblem(e);
                continue;
            }
            if (recordOutput is not null)
            {
                yield return recordOutput;
            }
        }
    }

    private List<XmlWrapper> MapXmls(IGraph asset, IUriNode subject)
    {
        var variationSequences = GetVariationSequences(asset, subject);
        var xmls = new List<XmlWrapper>();

        foreach (var variationSequence in variationSequences)
        {
            try
            {
                var xml = XmlMapper.Map(asset, variationSequence.Variation,
                    variationSequence.Sequence);
                xmls.AddRange(xml);
            }
            catch (Exception e)
            {
                logger.UnableXmlMap(variationSequence.Variation.Uri);
                logger.XmlMappingProblem(e);
            }
        }

        return xmls;
    }

    private static List<VariationSequence> GetVariationSequences(IGraph graph, IUriNode subject)
    {
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

    private sealed record VariationSequence(long? Sequence, IUriNode Variation);
}
