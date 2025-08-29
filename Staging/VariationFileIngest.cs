using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
    : StagingIngest<DriVariationFile>(sparqlClient, logger, "VariationFileGraph")
{
    private readonly HashSet<string> predicates = [];

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariationFile dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);

        var driId = new LiteralNode(dri.Id);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.VariationDriId, driId).FirstOrDefault()?.Subject;
        if (id is null)
        {
            logger.VariationNotFound(dri.Name, dri.Location); //TODO: sensitive information?
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationDriId, driId);
        graph.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            var xmlBase64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(dri.Xml));
            graph.Assert(id, Vocabulary.VariationDriXml, new LiteralNode(xmlBase64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            var proceed = await ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
            if (!proceed)
            {
                return null;
            }
        }
        logger.RecordBuilt(dri.Id);

        return graph;
    }

    internal override void PostIngest()
    {
        Console.WriteLine("Distinct RDF predicates:");
        foreach (var predicate in predicates.OrderBy(p => p))
        {
            Console.WriteLine(predicate);
        }
    }

    private async Task<bool> ExtractXmlData(IGraph graph, IGraph existing, INode id, string xml, CancellationToken cancellationToken)
    {
        var rdf = BaseIngest.GetRdf(xml);
        if (rdf is null)
        {
            logger.VariationXmlMissingRdf(id.AsValuedNode().AsString());
            return false;
        }

        predicates.UnionWith(rdf.Triples.PredicateNodes.Cast<IUriNode>().Select(p => p.Uri.ToString()).ToHashSet());

        BaseIngest.AssertLiteral(graph, id, rdf, note, Vocabulary.VariationNote);
        BaseIngest.AssertLiteral(graph, id, rdf, formerReferenceDepartment, Vocabulary.VariationPastName);
        BaseIngest.AssertLiteral(graph, id, rdf, physicalCondition, Vocabulary.VariationPhysicalConditionDescription);
        BaseIngest.AssertLiteral(graph, id, rdf, googleId, Vocabulary.VariationReferenceGoogleId);
        BaseIngest.AssertLiteral(graph, id, rdf, googleParentId, Vocabulary.VariationReferenceParentGoogleId);

        var datedNote = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.VariationHasDatedNote).SingleOrDefault()?.Object ?? BaseIngest.NewId;
        if (datedNote is not null)
        {
            var noteDate = existing.GetTriplesWithSubjectPredicate(datedNote, Vocabulary.DatedNoteHasDate).SingleOrDefault()?.Object ?? BaseIngest.NewId;
            AddDatedNote(graph, rdf, id, datedNote, noteDate); //TODO: could be overengineering
        }

        var redacted = rdf.GetTriplesWithPredicate(hasRedactedFile).Select(t => t.Object).Cast<ILiteralNode>();
        foreach (var redactedFile in redacted)
        {
            var partialPath = GetPartialPath(HttpUtility.UrlDecode(redactedFile.Value));
            var redactedVariation = await cacheClient.CacheFetch(CacheEntityKind.VariationByPartialPathAndAsset, [partialPath, options.Value.Code], cancellationToken);
            if (redactedVariation is not null)
            {
                graph.Assert(id, Vocabulary.VariationHasRedactedVariation, redactedVariation);
            }
            else
            {
                logger.RedactedVariationMissing(options.Value.Code, partialPath);
            }
        }

        var alternative = rdf.GetTriplesWithPredicate(hasPresentationManifestationFile).Select(t => t.Object).Cast<ILiteralNode>();
        foreach (var alternativeFile in alternative)
        {
            var partialPath = GetPartialPath(HttpUtility.UrlDecode(alternativeFile.Value));
            var alternativeVariation = await cacheClient.CacheFetch(CacheEntityKind.VariationByPartialPathAndAsset, [partialPath, options.Value.Code], cancellationToken);
            if (alternativeVariation is not null)
            {
                graph.Assert(id, Vocabulary.VariationHasAlternativeVariation, alternativeVariation);
            }
            else
            {
                logger.AlternativeVariationMissing(options.Value.Code, partialPath);
            }
        }

        return true;
    }

    private void AddDatedNote(IGraph graph, IGraph rdf, INode id, INode datedNode, INode noteDate)
    {
        var foundNote = rdf.GetTriplesWithPredicate(archivistNote).FirstOrDefault()?.Object;
        if (foundNote is not null)
        {
            graph.Assert(id, Vocabulary.VariationHasDatedNote, datedNode);
            var info = rdf.GetTriplesWithSubjectPredicate(foundNote, archivistNoteInfo).FirstOrDefault()?.Object as ILiteralNode;
            if (info is not null && !string.IsNullOrWhiteSpace(info.Value))
            {
                graph.Assert(datedNode, Vocabulary.ArchivistNote, new LiteralNode(info.Value)); //TODO: review notes to check if can be better modelled
                var date = rdf.GetTriplesWithSubjectPredicate(foundNote, archivistNoteDate).FirstOrDefault()?.Object as ILiteralNode;
                if (date is not null && !string.IsNullOrWhiteSpace(date.Value))
                {
                    BaseIngest.AssertYearMonthDay(graph, Vocabulary.DatedNoteHasDate, datedNode, noteDate, date.Value, logger);
                }
            }
        }
    }

    private static string GetPartialPath(string path) => path.Substring(path.IndexOf("/content/") + 8);

    private static readonly IUriNode note = new UriNode(new($"{BaseIngest.TnaNamespace}note"));
    private static readonly IUriNode hasRedactedFile = new UriNode(new($"{BaseIngest.TnaNamespace}hasRedactedFile"));
    private static readonly IUriNode hasPresentationManifestationFile = new UriNode(new($"{BaseIngest.TnaNamespace}hasPresentationManifestationFile"));
    private static readonly IUriNode formerReferenceDepartment = new UriNode(new($"{BaseIngest.TnaNamespace}formerReferenceDepartment"));
    private static readonly IUriNode physicalCondition = new UriNode(new($"{BaseIngest.TnaNamespace}physicalCondition"));
    private static readonly IUriNode googleId = new UriNode(new($"{BaseIngest.TnaNamespace}googleId"));
    private static readonly IUriNode googleParentId = new UriNode(new($"{BaseIngest.TnaNamespace}googleParentId"));
    private static readonly IUriNode archivistNote = new UriNode(new($"{BaseIngest.TnaNamespace}archivistNote"));
    private static readonly IUriNode archivistNoteInfo = new UriNode(new($"{BaseIngest.TnaNamespace}archivistNoteInfo"));
    private static readonly IUriNode archivistNoteDate = new UriNode(new($"{BaseIngest.TnaNamespace}archivistNoteDate"));
}
