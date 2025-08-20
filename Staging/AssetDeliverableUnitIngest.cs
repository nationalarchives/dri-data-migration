using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

public class AssetDeliverableUnitIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger<AssetDeliverableUnitIngest> logger)
    : BaseStagingIngest<DriAssetDeliverableUnit>(cache, sparqlClient, logger, "AssetDeliverableUnitGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriAssetDeliverableUnit dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);

        var driId = new LiteralNode(dri.Id);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.AssetDriId, driId).FirstOrDefault()?.Subject;
        if (id is null)
        {
            logger.AssetNotFound(dri.Id);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AssetDriId, driId);
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            var xmlBase64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(dri.Xml));
            graph.Assert(id, Vocabulary.AssetDriXml, new LiteralNode(xmlBase64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            await ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
        }
        logger.RecordBuilt(dri.Id);

        return graph;
    }

    private async Task ExtractXmlData(IGraph graph, IGraph existing,
        INode id, string xml, CancellationToken cancellationToken)
    {
        var rdf = GetRdf(xml);
        if (rdf is not null) //TODO: handle null
        {
            var batch = rdf.GetTriplesWithPredicate(batchIdentifier).SingleOrDefault()?.Object;
            if (batch is ILiteralNode batchNode)
            {
                graph.Assert(id, Vocabulary.BatchDriId, new LiteralNode(batchNode.Value));
            }

            var consignment = rdf.GetTriplesWithPredicate(tdrConsignmentRef).SingleOrDefault()?.Object;
            if (consignment is ILiteralNode consignmentNode)
            {
                graph.Assert(id, Vocabulary.ConsignmentTdrId, new LiteralNode(consignmentNode.Value));
            }

            var descr = rdf.GetTriplesWithPredicate(description).SingleOrDefault()?.Object;
            if (descr is ILiteralNode descrNode)
            {
                graph.Assert(id, Vocabulary.AssetDescription, new LiteralNode(descrNode.Value));
            }

            await AssertAsync(graph, id, rdf, language, CacheEntityKind.Language,
                Vocabulary.AssetHasLanguage, Vocabulary.LanguageName, cancellationToken);

            var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasRetention).SingleOrDefault()?.Object ?? NewId;
            graph.Assert(id, Vocabulary.AssetHasRetention, retention);
            await AssertAsync(graph, retention, rdf, heldBy, CacheEntityKind.FormalBody,
                Vocabulary.RetentionHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

            var creation = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasCreation).SingleOrDefault()?.Object ?? NewId;
            graph.Assert(id, Vocabulary.AssetHasCreation, creation);
            await AssertAsync(graph, creation, rdf, creator, CacheEntityKind.FormalBody,
                Vocabulary.CreationHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);


            await AssertAsync(graph, id, rdf, rights, CacheEntityKind.Copyright,
                Vocabulary.AssetHasCopyright, Vocabulary.CopyrightTitle, cancellationToken);

            var legal = rdf.GetTriplesWithPredicate(legalStatus).SingleOrDefault()?.Object;
            if (legal is IUriNode legalUri)
            {
                var statusType = legalUri.Uri.Segments.Last() switch
                {
                    "Public_Record(s)" => Vocabulary.PublicRecord,
                    _ => throw new ArgumentException(legalUri.Uri.ToString())
                };
                graph.Assert(id, Vocabulary.AssetHasLegalStatus, statusType);
            }
        }
    }

    private async Task AssertAsync(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, CacheEntityKind cacheEntityKind,
        IUriNode immediatePredicate, IUriNode? foundPredicate,
        CancellationToken cancellationToken)
    {
        var node = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        var name = node switch
        {//TODO: handle default
            ILiteralNode literalNode => literalNode.Value,
            IUriNode uriNode => uriNode.Uri.Segments.Last().Replace('_',' ')
        };

        var nodeId = await CacheFetchOrNew(cacheEntityKind, name, cancellationToken);
        graph.Assert(id, immediatePredicate, nodeId);
        if (foundPredicate is not null)
        {
            graph.Assert(nodeId, foundPredicate, new LiteralNode(name));
        }
    }

    private IGraph? GetRdf(string xml)
    {
        var du = new XmlDocument();
        du.LoadXml(xml);
        var rdfElement = du.GetElementsByTagName("rdf:RDF");
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
    private static readonly Uri dctermsNamespace = new("http://purl.org/dc/terms/");

    private static readonly IUriNode batchIdentifier = new UriNode(new($"{tnaNamespace}batchIdentifier"));
    private static readonly IUriNode tdrConsignmentRef = new UriNode(new($"{tnaNamespace}tdrConsignmentRef"));
    private static readonly IUriNode legalStatus = new UriNode(new($"{tnaNamespace}legalStatus"));
    private static readonly IUriNode heldBy = new UriNode(new($"{tnaNamespace}heldBy"));

    private static readonly IUriNode description = new UriNode(new(dctermsNamespace, "description"));
    private static readonly IUriNode creator = new UriNode(new(dctermsNamespace, "creator"));
    private static readonly IUriNode language = new UriNode(new(dctermsNamespace, "language"));
    private static readonly IUriNode rights = new UriNode(new(dctermsNamespace, "rights"));
}
