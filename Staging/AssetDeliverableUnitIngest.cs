using Api;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class AssetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<AssetDeliverableUnitIngest> logger)
    : StagingIngest<DriAssetDeliverableUnit>(sparqlClient, logger, cacheClient, "AssetDeliverableUnitGraph")
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
            var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dri.Xml));
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
        foreach (var predicate in predicates.OrderBy(p => p))
        {
            Console.WriteLine(predicate);
        }
    }

    private async Task<bool> ExtractXmlData(IGraph graph, IGraph existing,
        INode id, string xml, string assetReference, CancellationToken cancellationToken)
    {
        var rdf = RdfXmlLoader.GetRdf(xml, logger);
        if (rdf is null)
        {
            logger.AssetXmlMissingRdf(id.AsValuedNode().AsString());
            return false;
        }

        predicates.UnionWith(rdf.Triples.PredicateNodes.Cast<IUriNode>().Select(p => p.Uri.ToString()).ToHashSet());

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [batchIdentifier] = Vocabulary.BatchDriId,
            [tdrConsignmentRef] = Vocabulary.ConsignmentTdrId,
            [description] = Vocabulary.AssetDescription,
            [summary] = Vocabulary.AssetDescription,
            [administrativeBackground] = Vocabulary.AssetSummary,
            [relatedMaterial] = Vocabulary.AssetRelationDescription,
            [physicalDescription] = Vocabulary.AssetPhysicalDescription,
            [evidenceProvidedBy] = Vocabulary.EvidenceProviderName, //TODO: check if can be split
            [investigation] = Vocabulary.InvestigationName,//TODO: check if can be turned into entities
            [restrictionOnUse] = Vocabulary.AssetUsageRestrictionDescription,
            [formerReferenceTNA] = Vocabulary.AssetPastReference,
            [classification] = Vocabulary.AssetTag,
            [internalDepartment] = Vocabulary.AssetSourceInternalName,
            [filmMaker] = Vocabulary.FilmProductionCompanyName,
            [filmName] = Vocabulary.FilmTitle,
            [photographer] = Vocabulary.PhotographerDescription,
            [paperNumber] = Vocabulary.PaperNumber
        });
        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [session_date] = Vocabulary.CourtSessionDate,
            [hearing_date] = Vocabulary.InquiryHearingDate
        });

        assert.Integer(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [startImageNumber] = Vocabulary.ImageSequenceStart,
            [endImageNumber] = Vocabulary.ImageSequenceEnd
        });

        AddFilmDuration(graph, rdf, id);
        AddWebArchive(graph, rdf, id);
        await AddCourtCasesAsync(graph, rdf, id, assetReference, cancellationToken);
        await AddWitnessAsync(graph, rdf, id, cancellationToken);
        AddOriginDates(graph, rdf, id, existing);

        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, language, CacheEntityKind.Language,
            Vocabulary.AssetHasLanguage, Vocabulary.LanguageName, cancellationToken);

        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, counties, CacheEntityKind.GeographicalPlace,
            Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);

        var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasRetention).SingleOrDefault()?.Object ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        await assert.ExistingOrNewWithRelationshipAsync(graph, retention, rdf, heldBy, CacheEntityKind.FormalBody,
            Vocabulary.RetentionHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

        var creation = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasCreation).SingleOrDefault()?.Object ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.AssetHasCreation, creation);
        await assert.ExistingOrNewWithRelationshipAsync(graph, creation, rdf, creator, CacheEntityKind.FormalBody,
            Vocabulary.CreationHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

        await AddCopyrightAsync(graph, rdf, id, cancellationToken);
        AddLegalStatus(graph, rdf, id);

        return true;
    }

    private void AddFilmDuration(IGraph graph, IGraph rdf, INode id)
    {
        var foundDuration = rdf.GetTriplesWithPredicate(durationMins).SingleOrDefault()?.Object;
        if (foundDuration is ILiteralNode durationNode && !string.IsNullOrWhiteSpace(durationNode.Value))
        {
            if (TimeSpan.TryParseExact(durationNode.Value, "mm\\:ss", CultureInfo.InvariantCulture, out var ts))
            {
                var hours = ts.Hours == 0 ? string.Empty : $"{ts.Hours}H";
                var minutes = ts.Minutes == 0 ? string.Empty : $"{ts.Minutes}M";
                var seconds = ts.Seconds == 0 ? string.Empty : $"{ts.Seconds}S";
                graph.Assert(id, Vocabulary.FilmDuration, new LiteralNode($"PT{hours}{minutes}{seconds}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
            }
            else
            {
                logger.UnrecognizedFilmDurationFormat(durationNode.Value);
            }
        }
    }

    private static void AddWebArchive(IGraph graph, IGraph rdf, INode id)
    {
        var foundWebArchive = rdf.GetTriplesWithPredicate(webArchiveUrl).SingleOrDefault()?.Object;
        if (foundWebArchive is ILiteralNode webArchiveNode && !string.IsNullOrWhiteSpace(webArchiveNode.Value))
        {
            graph.Assert(id, Vocabulary.AssetHasUkGovernmentWebArchive, new UriNode(new Uri(webArchiveNode.AsValuedNode().AsString())));
        }
    }

    private void AddOriginDates(IGraph graph, IGraph rdf, INode id, IGraph existing)
    {
        var foundCoverage = rdf.GetTriplesWithPredicate(coverage).FirstOrDefault()?.Object;
        var dateStart = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginDateStart).SingleOrDefault()?.Object ??
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginApproximateDateStart).SingleOrDefault()?.Object ??
            CacheClient.NewId;
        var dateEnd = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginDateEnd).SingleOrDefault()?.Object ??
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginApproximateDateEnd).SingleOrDefault()?.Object ??
            CacheClient.NewId;
        if (foundCoverage is not null)
        {
            var start = rdf.GetTriplesWithSubjectPredicate(foundCoverage, startDate).FirstOrDefault()?.Object as ILiteralNode;
            if (start is not null && !string.IsNullOrWhiteSpace(start.Value))
            {
                assert.YearMonthDay(graph, Vocabulary.AssetHasOriginDateStart, id, dateStart, start.Value);
                var end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, endDate).FirstOrDefault()?.Object as ILiteralNode;
                if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
                {
                    assert.YearMonthDay(graph, Vocabulary.AssetHasOriginDateEnd, id, dateEnd, end.Value);
                }
            }
            else
            {
                var singleDate = rdf.GetTriplesWithSubjectPredicate(foundCoverage, fullDate).FirstOrDefault()?.Object as ILiteralNode ??
                    rdf.GetTriplesWithSubjectPredicate(foundCoverage, dateRange).FirstOrDefault()?.Object as ILiteralNode;
                if (singleDate is not null && !string.IsNullOrWhiteSpace(singleDate.Value))
                {
                    var yearRangeMatch = DateRegex.YearRange().Match(singleDate.Value);
                    if (yearRangeMatch.Success)
                    {
                        assert.YearMonthDay(graph, Vocabulary.AssetHasOriginApproximateDateStart, id, dateStart, yearRangeMatch.Groups["start"].Value);
                        assert.YearMonthDay(graph, Vocabulary.AssetHasOriginApproximateDateEnd, id, dateEnd, yearRangeMatch.Groups["end"].Value);
                    }
                    else if (singleDate.Value.StartsWith("c ") || singleDate.Value.StartsWith("[c "))
                    {
                        assert.YearMonthDay(graph, Vocabulary.AssetHasOriginApproximateDateStart, id, dateStart, singleDate.Value.Replace("c ", string.Empty));
                    }
                    else
                    {
                        assert.YearMonthDay(graph, Vocabulary.AssetHasOriginDateStart, id, dateStart, singleDate.Value);
                        assert.YearMonthDay(graph, Vocabulary.AssetHasOriginDateEnd, id, dateEnd, singleDate.Value);
                    }
                }
            }
        }
    }

    private async Task AddWitnessAsync(IGraph graph, IGraph rdf, INode id, CancellationToken cancellationToken)
    {
        var found = false;
        var witnessIndex = 1;
        var foundWitness = await FetchWitnessIdAsync(graph, rdf, id, witnessIndex, cancellationToken); //TODO: check if names could be split on ',' and 'and' and turned into entities
        while (foundWitness is not null)
        {
            found = true;
            witnessIndex++;
            foundWitness = await FetchWitnessIdAsync(graph, rdf, id, witnessIndex, cancellationToken);
        }
        if (found)
        {
            GraphAssert.Text(graph, id, rdf, session, Vocabulary.InquirySessionDescription);
        }
    }

    private async Task<IUriNode?> FetchWitnessIdAsync(IGraph graph, IGraph rdf, INode id, int witnessIndex, CancellationToken cancellationToken)
    {
        var foundWitness = rdf.GetTriplesWithPredicate(new UriNode(new($"{Vocabulary.TnaNamespace}witness_list_{witnessIndex}"))).SingleOrDefault()?.Object;
        if (foundWitness is ILiteralNode witnessNode && !string.IsNullOrWhiteSpace(witnessNode.Value))
        {
            var foundDescription = rdf.GetTriplesWithPredicate(new UriNode(new($"{Vocabulary.TnaNamespace}subject_role_{witnessIndex}"))).SingleOrDefault()?.Object as ILiteralNode;
            var witnessId = await cacheClient.CacheFetchOrNew(CacheEntityKind.InquiryAppearanceByWitnessAndDescription, [witnessNode.Value, foundDescription.Value], Vocabulary.InquiryWitnessName, cancellationToken);
            GraphAssert.Text(graph, witnessId, witnessNode.Value, Vocabulary.InquiryWitnessName); //TODO: check if can be split
            GraphAssert.Text(graph, witnessId, foundDescription.Value, Vocabulary.InquiryWitnessAppearanceDescription);
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
            GraphAssert.Text(graph, courtCase, rdf, new Dictionary<IUriNode, IUriNode>()
            {
                [new UriNode(new($"{Vocabulary.TnaNamespace}case_name_{caseIndex}"))] = Vocabulary.CourtCaseName,
                [new UriNode(new($"{Vocabulary.TnaNamespace}case_summary_{caseIndex}_judgment"))] = Vocabulary.CourtCaseSummaryJudgment,
                [new UriNode(new($"{Vocabulary.TnaNamespace}case_summary_{caseIndex}_reasons_for_judgment"))] = Vocabulary.CourtCaseSummaryReasonsForJudgment
            });
            assert.Date(graph, courtCase, rdf, new Dictionary<IUriNode, IUriNode>()
            {
                [new UriNode(new($"{Vocabulary.TnaNamespace}hearing_start_date_{caseIndex}"))] = Vocabulary.CourtCaseHearingStartDate,
                [new UriNode(new($"{Vocabulary.TnaNamespace}hearing_end_date_{caseIndex}"))] = Vocabulary.CourtCaseHearingEndDate
            });

            caseIndex++;
            courtCase = await FetchCourtCaseIdAsync(graph, rdf, id, caseIndex, assetReference, cancellationToken);
        }
        if (found)
        {
            GraphAssert.Text(graph, id, rdf, session, Vocabulary.CourtSessionDescription);
        }
    }

    private async Task<IUriNode?> FetchCourtCaseIdAsync(IGraph graph, IGraph rdf, INode id, int caseIndex, string assetReference, CancellationToken cancellationToken)
    {
        var foundCase = rdf.GetTriplesWithPredicate(new UriNode(new($"{Vocabulary.TnaNamespace}case_id_{caseIndex}"))).SingleOrDefault()?.Object;
        if (foundCase is ILiteralNode caseNode && !string.IsNullOrWhiteSpace(caseNode.Value))
        {
            var caseId = await cacheClient.CacheFetchOrNew(CacheEntityKind.VariationByPartialPathAndAsset, [caseNode.Value, assetReference], Vocabulary.VariationRelativeLocation, cancellationToken);
            graph.Assert(id, Vocabulary.CourtAssetHasCourtCase, caseId);
        }

        return null;
    }

    private async Task AddCopyrightAsync(IGraph graph, IGraph rdf, INode id, CancellationToken cancellationToken)
    {
        var copyrights = rdf.GetTriplesWithPredicate(rights).Select(t => t.Object)
            .Where(o => !string.IsNullOrWhiteSpace(o.ToString())).Cast<IUriNode>();
        foreach (var copyright in copyrights)
        {
            var title = copyright.Uri.Segments.Last().Replace('_', ' ');
            var copyrightId = await cacheClient.CacheFetchOrNew(CacheEntityKind.Copyright, title, Vocabulary.CopyrightTitle, cancellationToken);
            graph.Assert(id, Vocabulary.AssetHasCopyright, copyrightId);
        }
    }

    private void AddLegalStatus(IGraph graph, IGraph rdf, INode id)
    {
        var legal = rdf.GetTriplesWithPredicate(legalStatus).SingleOrDefault()?.Object;
        if (legal is IUriNode legalUri)
        {
            var statusType = legalUri.Uri.Segments.Last() switch
            {
                "Public_Record(s)" or "Public_record" or "Public_Record" or "PublicRecord" =>
                    Vocabulary.PublicRecord,
                "Welsh_Public_Record(s)" => Vocabulary.WelshPublicRecord,
                "Not_Public_Record(s)" => Vocabulary.NotPublicRecord,
                _ => null
            };
            if (statusType is null)
            {
                logger.UnrecognizedLegalStatus(legalUri.Uri.ToString());
            }
            else
            {
                graph.Assert(id, Vocabulary.AssetHasLegalStatus, statusType);
            }
        }
    }

    private static readonly Uri dctermsNamespace = new("http://purl.org/dc/terms/");
    private static readonly Uri transNamespace = new("http://nationalarchives.gov.uk/dri/transcription");

    private static readonly IUriNode batchIdentifier = new UriNode(new($"{Vocabulary.TnaNamespace}batchIdentifier"));
    private static readonly IUriNode tdrConsignmentRef = new UriNode(new($"{Vocabulary.TnaNamespace}tdrConsignmentRef"));
    private static readonly IUriNode relatedMaterial = new UriNode(new($"{Vocabulary.TnaNamespace}relatedMaterial"));
    private static readonly IUriNode legalStatus = new UriNode(new($"{Vocabulary.TnaNamespace}legalStatus"));
    private static readonly IUriNode heldBy = new UriNode(new($"{Vocabulary.TnaNamespace}heldBy"));
    private static readonly IUriNode physicalDescription = new UriNode(new($"{Vocabulary.TnaNamespace}physicalDescription"));
    private static readonly IUriNode investigation = new UriNode(new($"{Vocabulary.TnaNamespace}investigation"));
    private static readonly IUriNode evidenceProvidedBy = new UriNode(new($"{Vocabulary.TnaNamespace}evidenceProvidedBy"));
    private static readonly IUriNode session = new UriNode(new($"{Vocabulary.TnaNamespace}session"));
    private static readonly IUriNode session_date = new UriNode(new($"{Vocabulary.TnaNamespace}session_date"));
    private static readonly IUriNode restrictionOnUse = new UriNode(new($"{Vocabulary.TnaNamespace}restrictionOnUse"));
    private static readonly IUriNode hearing_date = new UriNode(new($"{Vocabulary.TnaNamespace}hearing_date"));
    private static readonly IUriNode webArchiveUrl = new UriNode(new($"{Vocabulary.TnaNamespace}webArchiveUrl"));
    private static readonly IUriNode startDate = new UriNode(new($"{Vocabulary.TnaNamespace}startDate"));
    private static readonly IUriNode endDate = new UriNode(new($"{Vocabulary.TnaNamespace}endDate"));
    private static readonly IUriNode formerReferenceTNA = new UriNode(new($"{Vocabulary.TnaNamespace}formerReferenceTNA"));
    private static readonly IUriNode classification = new UriNode(new($"{Vocabulary.TnaNamespace}classification"));
    private static readonly IUriNode summary = new UriNode(new($"{Vocabulary.TnaNamespace}summary"));
    private static readonly IUriNode internalDepartment = new UriNode(new($"{Vocabulary.TnaNamespace}internalDepartment"));
    private static readonly IUriNode durationMins = new UriNode(new($"{Vocabulary.TnaNamespace}durationMins"));
    private static readonly IUriNode filmMaker = new UriNode(new($"{Vocabulary.TnaNamespace}filmMaker"));
    private static readonly IUriNode filmName = new UriNode(new($"{Vocabulary.TnaNamespace}filmName"));
    private static readonly IUriNode photographer = new UriNode(new($"{Vocabulary.TnaNamespace}photographer"));
    private static readonly IUriNode fullDate = new UriNode(new($"{Vocabulary.TnaNamespace}fullDate"));
    private static readonly IUriNode dateRange = new UriNode(new($"{Vocabulary.TnaNamespace}dateRange"));
    private static readonly IUriNode administrativeBackground = new UriNode(new($"{Vocabulary.TnaNamespace}administrativeBackground"));

    private static readonly IUriNode description = new UriNode(new(dctermsNamespace, "description"));
    private static readonly IUriNode creator = new UriNode(new(dctermsNamespace, "creator"));
    private static readonly IUriNode language = new UriNode(new(dctermsNamespace, "language"));
    private static readonly IUriNode rights = new UriNode(new(dctermsNamespace, "rights"));
    private static readonly IUriNode coverage = new UriNode(new(dctermsNamespace, "coverage"));

    //Local names of predicates are constructed by concatenation with the last path segment due to the lack of end forward slash in the XML namespace declaration.
    private static readonly IUriNode paperNumber = new UriNode(new($"{transNamespace}paperNumber"));
    private static readonly IUriNode counties = new UriNode(new($"{transNamespace}counties"));
    private static readonly IUriNode startImageNumber = new UriNode(new($"{transNamespace}startImageNumber"));
    private static readonly IUriNode endImageNumber = new UriNode(new($"{transNamespace}endImageNumber"));
}
