using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Diagnostics.Metrics;
using System.Text.Json;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

public class VariationFileIngest(ICacheClient cacheClient, ISparqlClient sparqlClient,
    ILogger<VariationFileIngest> logger, IMeterFactory meterFactory) :
    StagingIngest<DriVariationFile>(sparqlClient, logger, meterFactory, "VariationFileGraph")
{
    private readonly VariationFileXmlIngest xmlIngest = new(logger, cacheClient);
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariationFile dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.VariationDriId, driId);
        if (id is null)
        {
            logger.VariationNotFound(dri.Name); //TODO: sensitive information?
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationDriId, driId);
        graph.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        GraphAssert.Text(graph, id, dri.ManifestationId, Vocabulary.VariationDriManifestationId);
        GraphAssert.Integer(graph, id, dri.FileSize, Vocabulary.VariationSizeBytes);
        AddChecksums(graph, existing, id, dri.Checksums);
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.VariationDriXml);
            await xmlIngest.ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
        }

        return graph;
    }

    private void AddChecksums(IGraph graph, IGraph existing, IUriNode id, string? checksums)
    {
        if (checksums is null)
        {
            return;
        }
        var hashes = JsonSerializer.Deserialize<List<HashInfo>>(checksums, jsonSerializerOptions);
        if (hashes is null)
        {
            return;
        }
        foreach (var hash in hashes)
        {
            var alg = hash.Alg switch
            {
                "MD5" => Vocabulary.MD5,
                "SHA-1" => Vocabulary.SHA1,
                "SHA-256" => Vocabulary.SHA256,
                "SHA-512" => Vocabulary.SHA512,
                _ => null
            };
            if (alg is null)
            {
                logger.HashFunctionNotResolved(hash.Alg);
                continue;
            }
            var dataIntegrityId = existing.GetTriplesWithPredicateObject(Vocabulary.VariationDataIntegrityCalculationHasHashFunction, alg).SingleOrDefault()?.Subject ?? CacheClient.NewId;
            graph.Assert(id, Vocabulary.VariationHasVariationDataIntegrityCalculation, dataIntegrityId);
            graph.Assert(dataIntegrityId, Vocabulary.VariationDataIntegrityCalculationHasHashFunction, alg);
            GraphAssert.Text(graph, dataIntegrityId, hash.Checksum, Vocabulary.Checksum);
        }
    }

    private sealed record HashInfo(string Alg, string Checksum);
}
