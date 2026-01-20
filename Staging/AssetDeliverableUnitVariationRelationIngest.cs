using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Text.Json;
using System.Web;
using System.Xml;
using VDS.RDF;

namespace Staging;

internal class AssetDeliverableUnitVariationRelationIngest(ILogger logger, ICacheClient cacheClient)
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal async Task AddVariationRelationsAsync(IGraph graph, IGraph rdf, IUriNode id,
        XmlDocument doc, string filesJson, CancellationToken cancellationToken)
    {
        var files = JsonSerializer.Deserialize<List<RelationVariation>>(filesJson, jsonSerializerOptions);
        await BuildAsync(graph, rdf, id, doc, files, IngestVocabulary.HasRedactedFile, Vocabulary.RedactedVariationSequence, cancellationToken);
        await BuildAsync(graph, rdf, id, doc, files, IngestVocabulary.HasPresentationManifestationFile, Vocabulary.PresentationVariationSequence, cancellationToken);
    }

    private async Task BuildAsync(IGraph graph, IGraph rdf, IUriNode id, XmlDocument doc, List<RelationVariation>? files,
        IUriNode relationshipPredicate, IUriNode sequencePredicate, CancellationToken cancellationToken)
    {
        var variations = rdf.GetTriplesWithPredicate(relationshipPredicate).Select(t => t.Object).Cast<ILiteralNode>();
        if (variations.Any())
        {
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("tna", IngestVocabulary.TnaNamespace.ToString());
            var xmlFiles = doc.SelectNodes($"descendant::tna:{relationshipPredicate.Uri.Fragment.Trim('#')}", namespaceManager);
            if (xmlFiles is null)
            {
                return;
            }
            foreach (var variation in variations)
            {
                var variationId = GetVariationId(variation.Value, files);
                IUriNode? relatedVariation = null;
                if (variationId is not null)
                {
                    relatedVariation = await cacheClient.CacheFetch(CacheEntityKind.Variation, variationId, cancellationToken);
                }
                if (relatedVariation is not null)
                {
                    var foundFile = AssertRelation(graph, id, relatedVariation, xmlFiles, variation.Value, sequencePredicate); ;
                    if (!foundFile)
                    {
                        logger.UnableEstablishRelatedVariationSequence(variation.Value);
                    }
                }
                else
                {
                    logger.RelatedVariationMissing(variation.Value);
                }
            }
        }
    }

    private static bool AssertRelation(IGraph graph, IUriNode id, IUriNode relatedVariation,
        XmlNodeList xmlFiles, string variation, IUriNode sequencePredicate)
    {
        graph.Assert(id, Vocabulary.AssetHasVariation, relatedVariation);
        var foundFile = false;
        for (int i = 0; i < xmlFiles.Count; i++)
        {
            if (xmlFiles.Item(i)?.InnerText.Equals(variation) == true)
            {
                foundFile = true;
                GraphAssert.Integer(graph, relatedVariation, i + 1, sequencePredicate);
                break;
            }
        }

        return foundFile;
    }

    private static string? GetVariationId(string variation, List<RelationVariation>? files)
    {
        var decodedPath = HttpUtility.UrlDecode(variation);
        var variationName = GetPartialPath(decodedPath);

        return files?.SingleOrDefault(f => f.Name == variationName && HasPathPartialMatch(decodedPath, f.Location))?.Id;
    }

    private static string GetPartialPath(string path) => path.Substring(path.LastIndexOf('/') + 1);

    private sealed record RelationVariation(string Id, string Location, string Name);

    private static bool HasPathPartialMatch(string fullPath, string partialPath)
    {
        var fullPathSegments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return partialPath.Split('/', StringSplitOptions.RemoveEmptyEntries).All(p =>
            fullPathSegments.Contains(p) || fullPathSegments.Contains(p.Replace(' ', '_')));
    }
}
