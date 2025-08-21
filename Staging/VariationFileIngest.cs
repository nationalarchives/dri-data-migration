using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class VariationFileIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger<VariationFileIngest> logger, IOptions<DriSettings> options)
    : BaseStagingIngest<DriVariationFile>(cache, sparqlClient, logger, "VariationFileGraph")
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
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var rdf = GetRdf(doc);
        if (rdf is null)
        {
            logger.VariationXmlMissingRdf(id.AsValuedNode().AsString());
            return;
        }

            var comment = rdf.GetTriplesWithPredicate(note).SingleOrDefault()?.Object;
            if (comment is ILiteralNode commentNode)
            {
                graph.Assert(id, Vocabulary.VariationNote, new LiteralNode(commentNode.Value));
            }

            var redacted = rdf.GetTriplesWithPredicate(hasRedactedFile).Select(t => t.Object).Cast<ILiteralNode>();
            foreach (var redactedFile in redacted)
            {
                var partialPath = GetPartialPath(HttpUtility.UrlDecode(redactedFile.Value));
                var redactedVariation = await CacheFetch(CacheEntityKind.VariationByAssetAndPartialPath, [partialPath, options.Value.Code], cancellationToken);
                if (redactedVariation is not null) //TODO: handle null
                {
                    graph.Assert(id, Vocabulary.VariationHasRedactedVariation, redactedVariation);
                }
            }

            var alternative = rdf.GetTriplesWithPredicate(hasPresentationManifestationFile).Select(t => t.Object).Cast<ILiteralNode>();
            foreach (var alternativeFile in alternative)
            {
                var partialPath = GetPartialPath(HttpUtility.UrlDecode(alternativeFile.Value));
                var alternativeVariation = await CacheFetch(CacheEntityKind.VariationByAssetAndPartialPath, [partialPath, options.Value.Code], cancellationToken);
                if (alternativeVariation is not null) //TODO: handle null
                {
                    graph.Assert(id, Vocabulary.VariationHasAlternativeVariation, alternativeVariation);
                }
            else
                {
                    "Title" or "Creation Date" or "Encrypted" or "Creator" or "Number of Images" => null,
                    "Creating Application" => Vocabulary.VariationHasCausingSoftware,
                    "Number of Pages" => Vocabulary.NumberOfPages,
                    "Word Count" => Vocabulary.WordCount,
                    "Character Count" => Vocabulary.CharacterCount,
                    _ => null
                };
                var value = properties.Item(i).SelectSingleNode("ex:Value", mgr).InnerText;
                if (predicate is not null && value is not null) //TODO: handle null
                {
                    if ((predicate == Vocabulary.NumberOfPages ||
                        predicate == Vocabulary.WordCount ||
                        predicate == Vocabulary.CharacterCount) && int.TryParse(value, out var n))
                    {
                        graph.Assert(id, predicate, new LongNode(n));
                    }
                    if (predicate == Vocabulary.VariationHasCausingSoftware)
                    {
                        var causingSoftware = await CacheFetchOrNew(CacheEntityKind.CausingSoftware, value, cancellationToken);
                        graph.Assert(id, predicate, causingSoftware);
                        graph.Assert(causingSoftware, Vocabulary.CausingSoftwareName, new LiteralNode(value));
                    }
                }
            }
        }
    }

    private string GetPartialPath(string path) => path.Substring(path.IndexOf("/content/") + 8);

    private IGraph? GetRdf(XmlDocument xml)
    {
        var rdfElement = xml.GetElementsByTagName("rdf:RDF");
        if (rdfElement.Count == 1)
        {
            var rdf = new Graph
            {
                BaseUri = new Uri("http://example.com")
            };
            rdf.NamespaceMap.AddNamespace("tna", new Uri("http://nationalarchives.gov.uk/metadata/tna#"));
            new RdfXmlParser().Load(rdf, new StringReader(rdfElement[0].OuterXml));
            return rdf;
        }
        return null;
    }

    private static readonly Uri tnaNamespace = new("http://nationalarchives.gov.uk/metadata/tna#");

    private static readonly IUriNode note = new UriNode(new($"{tnaNamespace}note"));
    private static readonly IUriNode hasRedactedFile = new UriNode(new($"{tnaNamespace}hasRedactedFile"));
    private static readonly IUriNode hasPresentationManifestationFile = new UriNode(new($"{tnaNamespace}hasPresentationManifestationFile"));

}
