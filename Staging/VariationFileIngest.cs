using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class VariationFileIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<VariationFileIngest> logger, IOptions<DriSettings> options)
    : BaseStagingIngest<DriVariationFile>(sparqlClient, logger, "VariationFileGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariationFile dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);

        var driId = new LiteralNode(dri.Id);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.VariationDriId, driId).FirstOrDefault()?.Subject;
        if (id is null)
        {
            logger.VariationNotFound(dri.Id);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationDriId, driId);
        graph.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            var xmlBase64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(dri.Xml));
            graph.Assert(id, Vocabulary.VariationDriXml, new LiteralNode(xmlBase64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            await ExtractXmlData(graph, id, dri.Xml, cancellationToken);
        }
        logger.RecordBuilt(dri.Id);

        return graph;
    }

    private async Task ExtractXmlData(IGraph graph, INode id, string xml, CancellationToken cancellationToken)
    {
        var rdf = BaseIngest.GetRdf(xml);
        if (rdf is null)
        {
            logger.VariationXmlMissingRdf(id.AsValuedNode().AsString());
            return;
        }

        BaseIngest.AssertLiteral(graph, id, rdf, note, Vocabulary.VariationNote);

        var redacted = rdf.GetTriplesWithPredicate(hasRedactedFile).Select(t => t.Object).Cast<ILiteralNode>();
        foreach (var redactedFile in redacted)
        {
            var partialPath = GetPartialPath(HttpUtility.UrlDecode(redactedFile.Value));
            var redactedVariation = await cacheClient.CacheFetch(CacheEntityKind.VariationByPartialPathAndAsset, [partialPath, options.Value.Code], cancellationToken);
            if (redactedVariation is not null) //TODO: handle null
            {
                graph.Assert(id, Vocabulary.VariationHasRedactedVariation, redactedVariation);
            }
            else
            {
            }
        }

        var alternative = rdf.GetTriplesWithPredicate(hasPresentationManifestationFile).Select(t => t.Object).Cast<ILiteralNode>();
        foreach (var alternativeFile in alternative)
        {
            var partialPath = GetPartialPath(HttpUtility.UrlDecode(alternativeFile.Value));
            var alternativeVariation = await cacheClient.CacheFetch(CacheEntityKind.VariationByPartialPathAndAsset, [partialPath, options.Value.Code], cancellationToken);
            if (alternativeVariation is not null) //TODO: handle null
            {
                graph.Assert(id, Vocabulary.VariationHasAlternativeVariation, alternativeVariation);
            }
            else
            {
            }
        }
    }

    private static string GetPartialPath(string path) => path.Substring(path.IndexOf("/content/") + 8);

    private static readonly IUriNode note = new UriNode(new($"{BaseIngest.TnaNamespace}note"));
    private static readonly IUriNode hasRedactedFile = new UriNode(new($"{BaseIngest.TnaNamespace}hasRedactedFile"));
    private static readonly IUriNode hasPresentationManifestationFile = new UriNode(new($"{BaseIngest.TnaNamespace}hasPresentationManifestationFile"));
}
