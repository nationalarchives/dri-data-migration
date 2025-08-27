using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class AssetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<AssetDeliverableUnitIngest> logger)
    : StagingIngest<DriAssetDeliverableUnit>(sparqlClient, logger, "AssetDeliverableUnitGraph")
{
    private readonly HashSet<string> predicates = [];

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
            var proceed = await ExtractXmlData(graph, existing, id, dri.Xml, dri.Reference, cancellationToken);
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
        foreach (var predicate in predicates)
        {
            Console.WriteLine(predicate);
        }
    }

    private async Task<bool> ExtractXmlData(IGraph graph, IGraph existing,
        INode id, string xml, string assetReference, CancellationToken cancellationToken)
    {
        var rdf = BaseIngest.GetRdf(xml);
        if (rdf is null)
        {
            logger.AssetXmlMissingRdf(id.AsValuedNode().AsString());
            return false;
        }

        predicates.UnionWith(rdf.Triples.PredicateNodes.Cast<IUriNode>().Select(p => p.Uri.ToString()).ToHashSet());

        BaseIngest.AssertLiteral(graph, id, rdf, batchIdentifier, Vocabulary.BatchDriId);
        BaseIngest.AssertLiteral(graph, id, rdf, tdrConsignmentRef, Vocabulary.ConsignmentTdrId);
        BaseIngest.AssertLiteral(graph, id, rdf, description, Vocabulary.AssetDescription);
        BaseIngest.AssertLiteral(graph, id, rdf, relatedMaterial, Vocabulary.AssetRelationDescription);
        BaseIngest.AssertLiteral(graph, id, rdf, physicalDescription, Vocabulary.AssetPhysicalDescription);
        BaseIngest.AssertLiteral(graph, id, rdf, evidenceProvidedBy, Vocabulary.EvidenceProviderName);
        BaseIngest.AssertLiteral(graph, id, rdf, investigation, Vocabulary.InvestigationName);
        BaseIngest.AssertDate(graph, id, rdf, session_date, "dd/MM/yyyy", Vocabulary.CourtSessionDate);
        BaseIngest.AssertDate(graph, id, rdf, hearing_date, "dd/MM/yyyy", Vocabulary.InquiryHearingDate);
        BaseIngest.AssertLiteral(graph, id, rdf, restrictionOnUse, Vocabulary.AssetUseRestrictionDescription);

        AddWebArchive(graph, rdf, id);

        await AddCourtCasesAsync(graph, rdf, id, assetReference, cancellationToken);
        
        await AddWitnessAsync(graph, rdf, id, cancellationToken);

        await BaseIngest.AssertAsync(graph, id, rdf, language, CacheEntityKind.Language,
            Vocabulary.AssetHasLanguage, Vocabulary.LanguageName, cacheClient, cancellationToken);

        var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasRetention).SingleOrDefault()?.Object ?? BaseIngest.NewId;
        graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        await BaseIngest.AssertAsync(graph, retention, rdf, heldBy, CacheEntityKind.FormalBody,
            Vocabulary.RetentionHasFormalBody, Vocabulary.FormalBodyName, cacheClient, cancellationToken);

        var creation = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasCreation).SingleOrDefault()?.Object ?? BaseIngest.NewId;
        graph.Assert(id, Vocabulary.AssetHasCreation, creation);
        await BaseIngest.AssertAsync(graph, creation, rdf, creator, CacheEntityKind.FormalBody,
            Vocabulary.CreationHasFormalBody, Vocabulary.FormalBodyName, cacheClient, cancellationToken);

        var copyrights = rdf.GetTriplesWithPredicate(rights).Select(t => t.Object)
            .Where(o => !string.IsNullOrWhiteSpace(o.ToString())).Cast<IUriNode>();
        foreach (var copyright in copyrights)
        {
            var title = copyright.Uri.Segments.Last().Replace('_', ' ');
            var copyrightId = await cacheClient.CacheFetchOrNew(CacheEntityKind.Copyright, title, Vocabulary.CopyrightTitle, cancellationToken);
            graph.Assert(id, Vocabulary.AssetHasCopyright, copyrightId);

        }

        var legal = rdf.GetTriplesWithPredicate(legalStatus).SingleOrDefault()?.Object;
        if (legal is IUriNode legalUri)
        {
            var statusType = legalUri.Uri.Segments.Last() switch
            {
                "Public_Record(s)" or "Public_record" or "Public_Record" => Vocabulary.PublicRecord,
                "Welsh_Public_Record(s)" => Vocabulary.WelshPublicRecord,
                _ => throw new ArgumentException(legalUri.Uri.ToString())
            };
            graph.Assert(id, Vocabulary.AssetHasLegalStatus, statusType);
        }

        return true;
    }

    private static void AddWebArchive(IGraph graph, IGraph rdf, INode id)
    {
        var foundWebArchive = rdf.GetTriplesWithPredicate(webArchiveUrl).SingleOrDefault()?.Object;
        if (foundWebArchive is ILiteralNode webArchiveNode && !string.IsNullOrWhiteSpace(webArchiveNode.Value))
        {
            graph.Assert(id, Vocabulary.AssetHasUkGovernmentWebArchive, new UriNode(new Uri(webArchiveNode.AsValuedNode().AsString())));
        }
    }

    private async Task AddWitnessAsync(IGraph graph, IGraph rdf, INode id, CancellationToken cancellationToken)
    {
        var found = false;
        var witnessIndex = 1;
        var foundWitness = await FetchWitnessIdAsync(graph, rdf, id, witnessIndex, cancellationToken);
        while (foundWitness is not null)
        {
            found = true;
            witnessIndex++;
            foundWitness = await FetchWitnessIdAsync(graph, rdf, id, witnessIndex, cancellationToken);
        }
        if (found)
        {
            BaseIngest.AssertLiteral(graph, id, rdf, session, Vocabulary.InquirySessionDescription);
        }
    }

    private async Task<IUriNode?> FetchWitnessIdAsync(IGraph graph, IGraph rdf, INode id, int witnessIndex, CancellationToken cancellationToken)
    {
        var foundWitness = rdf.GetTriplesWithPredicate(new UriNode(new($"{BaseIngest.TnaNamespace}witness_list_{witnessIndex}"))).SingleOrDefault()?.Object;
        if (foundWitness is ILiteralNode witnessNode && !string.IsNullOrWhiteSpace(witnessNode.Value))
        {
            var foundDescription = rdf.GetTriplesWithPredicate(new UriNode(new($"{BaseIngest.TnaNamespace}subject_role_{witnessIndex}"))).SingleOrDefault()?.Object as ILiteralNode;
            var witnessId = await cacheClient.CacheFetchOrNew(CacheEntityKind.InquiryAppearanceByWitnessAndDescription, [witnessNode.Value, foundDescription.Value], Vocabulary.InquiryWitnessName, cancellationToken);
            graph.Assert(witnessId, Vocabulary.InquiryWitnessName, new LiteralNode(witnessNode.Value));
            graph.Assert(witnessId, Vocabulary.InquiryWitnessAppearanceDescription, new LiteralNode(foundDescription.Value));
            graph.Assert(id, Vocabulary.InquiryAssetHasInquiryAppearance, witnessId);

            return witnessId;
        }

        return null;
    }

    private async Task AddCourtCasesAsync(IGraph graph, IGraph rdf, INode id, string assetReference, CancellationToken cancellationToken)
    {
        var found = false;
        var caseIndex = 1;
        var courtCase = await FetchCourtCaseIdAsync(graph, rdf, id, caseIndex, assetReference, cancellationToken);
        while (courtCase is not null)
        {
            found = true;
            BaseIngest.AssertLiteral(graph, courtCase, rdf, new UriNode(new($"{BaseIngest.TnaNamespace}case_name_{caseIndex}")), Vocabulary.CourtCaseName);
            BaseIngest.AssertLiteral(graph, courtCase, rdf, new UriNode(new($"{BaseIngest.TnaNamespace}case_summary_{caseIndex}_judgment")), Vocabulary.CourtCaseSummaryJudgment);
            BaseIngest.AssertLiteral(graph, courtCase, rdf, new UriNode(new($"{BaseIngest.TnaNamespace}case_summary_{caseIndex}_reasons_for_judgment")), Vocabulary.CourtCaseSummaryReasonsForJudgment);
            BaseIngest.AssertDate(graph, courtCase, rdf, new UriNode(new($"{BaseIngest.TnaNamespace}hearing_start_date_{caseIndex}")), "dd/MM/yyyy", Vocabulary.CourtCaseHearingStartDate);
            BaseIngest.AssertDate(graph, courtCase, rdf, new UriNode(new($"{BaseIngest.TnaNamespace}hearing_end_date_{caseIndex}")), "dd/MM/yyyy", Vocabulary.CourtCaseHearingEndDate);

            caseIndex++;
            courtCase = await FetchCourtCaseIdAsync(graph, rdf, id, caseIndex, assetReference, cancellationToken);
        }
        if (found)
        {
            BaseIngest.AssertLiteral(graph, id, rdf, session, Vocabulary.CourtSessionDescription);
        }
    }

    private async Task<IUriNode?> FetchCourtCaseIdAsync(IGraph graph, IGraph rdf, INode id, int caseIndex, string assetReference, CancellationToken cancellationToken)
    {
        var foundCase = rdf.GetTriplesWithPredicate(new UriNode(new($"{BaseIngest.TnaNamespace}case_id_{caseIndex}"))).SingleOrDefault()?.Object;
        if (foundCase is ILiteralNode caseNode && !string.IsNullOrWhiteSpace(caseNode.Value))
        {
            var caseId = await cacheClient.CacheFetchOrNew(CacheEntityKind.VariationByPartialPathAndAsset, [caseNode.Value, assetReference], Vocabulary.VariationRelativeLocation, cancellationToken);
            graph.Assert(id, Vocabulary.CourtAssetHasCourtCase, caseId);
        }

        return null;
    }

    private static readonly Uri dctermsNamespace = new("http://purl.org/dc/terms/");

    private static readonly IUriNode batchIdentifier = new UriNode(new($"{BaseIngest.TnaNamespace}batchIdentifier"));
    private static readonly IUriNode tdrConsignmentRef = new UriNode(new($"{BaseIngest.TnaNamespace}tdrConsignmentRef"));
    private static readonly IUriNode relatedMaterial = new UriNode(new($"{BaseIngest.TnaNamespace}relatedMaterial"));
    private static readonly IUriNode legalStatus = new UriNode(new($"{BaseIngest.TnaNamespace}legalStatus"));
    private static readonly IUriNode heldBy = new UriNode(new($"{BaseIngest.TnaNamespace}heldBy"));
    private static readonly IUriNode physicalDescription = new UriNode(new($"{BaseIngest.TnaNamespace}physicalDescription"));
    private static readonly IUriNode investigation = new UriNode(new($"{BaseIngest.TnaNamespace}investigation"));
    private static readonly IUriNode evidenceProvidedBy = new UriNode(new($"{BaseIngest.TnaNamespace}evidenceProvidedBy"));
    private static readonly IUriNode session = new UriNode(new($"{BaseIngest.TnaNamespace}session"));
    private static readonly IUriNode session_date = new UriNode(new($"{BaseIngest.TnaNamespace}session_date"));
    private static readonly IUriNode restrictionOnUse = new UriNode(new($"{BaseIngest.TnaNamespace}restrictionOnUse"));
    private static readonly IUriNode hearing_date = new UriNode(new($"{BaseIngest.TnaNamespace}hearing_date"));
    private static readonly IUriNode webArchiveUrl = new UriNode(new($"{BaseIngest.TnaNamespace}webArchiveUrl"));

    private static readonly IUriNode description = new UriNode(new(dctermsNamespace, "description"));
    private static readonly IUriNode creator = new UriNode(new(dctermsNamespace, "creator"));
    private static readonly IUriNode language = new UriNode(new(dctermsNamespace, "language"));
    private static readonly IUriNode rights = new UriNode(new(dctermsNamespace, "rights"));
}
