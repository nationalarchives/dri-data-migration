using Api;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Web;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class AssetDeliverableUnitXmlIngest(ILogger logger, ICacheClient cacheClient)
{
    public readonly HashSet<string> Predicates = [];
    private readonly GraphAssert assert = new(logger, cacheClient);
    private readonly DimensionParser dimensionParser = new(logger);
    private readonly DateParser dateParser = new(logger);
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task ExtractXmlData(IGraph graph, IGraph existing,
        IUriNode id, string xml, string assetReference, string filesJson,
        CancellationToken cancellationToken)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var rdf = rdfXmlLoader.GetRdf(doc);
        if (rdf is null)
        {
            logger.AssetXmlMissingRdf(id.AsValuedNode().AsString());
            return;
        }

        Predicates.UnionWith(rdf.Triples.PredicateNodes.Cast<IUriNode>().Select(p => p.Uri.ToString()).ToHashSet());

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.BatchIdentifier] = Vocabulary.BatchDriId,
            [IngestVocabulary.TdrConsignmentRef] = Vocabulary.ConsignmentTdrId,
            [IngestVocabulary.Description] = Vocabulary.AssetDescription,
            [IngestVocabulary.ContentManagementSystemContainer] = Vocabulary.AssetDescription,
            [IngestVocabulary.Summary] = Vocabulary.AssetDescription,
            [IngestVocabulary.AdditionalInformation] = Vocabulary.AssetDescription,
            [IngestVocabulary.AdministrativeBackground] = Vocabulary.AssetSummary,
            [IngestVocabulary.RelatedMaterial] = Vocabulary.AssetRelationDescription,
            [IngestVocabulary.TransRelatedMaterial] = Vocabulary.AssetRelationDescription,
            [IngestVocabulary.PhysicalDescription] = Vocabulary.AssetPhysicalDescription,
            [IngestVocabulary.PhysicalFormat] = Vocabulary.AssetPhysicalDescription,
            [IngestVocabulary.EvidenceProvidedBy] = Vocabulary.EvidenceProviderName, //TODO: check if can be split and turned into entities
            [IngestVocabulary.Investigation] = Vocabulary.InvestigationName,//TODO: check if can be turned into entities
            [IngestVocabulary.RestrictionOnUse] = Vocabulary.AssetUsageRestrictionDescription,
            [IngestVocabulary.FormerReferenceTna] = Vocabulary.AssetPastReference,
            [IngestVocabulary.FormerReferenceDepartment] = Vocabulary.AssetPastReference,
            [IngestVocabulary.Classification] = Vocabulary.AssetTag,
            [IngestVocabulary.InternalDepartment] = Vocabulary.AssetSourceInternalName,
            [IngestVocabulary.FilmMaker] = Vocabulary.FilmProductionCompanyName,
            [IngestVocabulary.FilmName] = Vocabulary.FilmTitle,
            [IngestVocabulary.Photographer] = Vocabulary.PhotographerDescription,
            [IngestVocabulary.PaperNumber] = Vocabulary.PaperNumber,
            [IngestVocabulary.SealOwner] = Vocabulary.SealOwnerName, //TODO: check if can be turned into entities
            [IngestVocabulary.ColourOfOriginalSeal] = Vocabulary.SealColour,
            [IngestVocabulary.SeparatedMaterial] = Vocabulary.AssetConnectedAssetNote,
            [IngestVocabulary.AttachmentFormerReference] = Vocabulary.EmailAttachmentReference
        });
        assert.Date(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.Session_date] = Vocabulary.CourtSessionDate,
            [IngestVocabulary.Hearing_date] = Vocabulary.InquiryHearingDate
        });
        GraphAssert.MultiText(graph, id, rdf, IngestVocabulary.Title, Vocabulary.AssetName);
        assert.Integer(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.StartImageNumber] = Vocabulary.ImageSequenceStart,
            [IngestVocabulary.EndImageNumber] = Vocabulary.ImageSequenceEnd
        });

        await AddVariationRelations(graph, rdf, id, doc, filesJson, cancellationToken);
        AddFilmDuration(graph, rdf, id);
        AddWebArchive(graph, rdf, id);
        await AddCourtCasesAsync(graph, rdf, id, assetReference, cancellationToken);
        await AddWitnessAsync(graph, rdf, id, cancellationToken);
        AddOriginDates(graph, rdf, id, existing);

        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, IngestVocabulary.Language, CacheEntityKind.Language,
            Vocabulary.AssetHasLanguage, Vocabulary.LanguageName, cancellationToken);

        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, IngestVocabulary.Counties, CacheEntityKind.GeographicalPlace,
            Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);
        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, IngestVocabulary.County, CacheEntityKind.GeographicalPlace,
            Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);

        var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasRetention).SingleOrDefault()?.Object ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        await assert.ExistingOrNewWithRelationshipAsync(graph, retention, rdf, IngestVocabulary.HeldBy, CacheEntityKind.FormalBody,
            Vocabulary.RetentionHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

        var creation = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasCreation).SingleOrDefault()?.Object ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.AssetHasCreation, creation);
        await assert.ExistingOrNewWithRelationshipAsync(graph, creation, rdf, IngestVocabulary.Creator, CacheEntityKind.FormalBody,
            Vocabulary.CreationHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

        await AddCopyrightAsync(graph, rdf, id, cancellationToken);
        AddLegalStatus(graph, rdf, id);
        await AddSeal(graph, rdf, existing, id, cancellationToken);
    }

    private async Task AddVariationRelations(IGraph graph, IGraph rdf, IUriNode id,
        XmlDocument doc, string filesJson, CancellationToken cancellationToken)
    {
        var redacted = rdf.GetTriplesWithPredicate(IngestVocabulary.HasRedactedFile).Select(t => t.Object).Cast<ILiteralNode>();
        if (redacted.Any())
        {
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("tna", IngestVocabulary.TnaNamespace.ToString());
            var xmlRedactedFiles = doc.SelectNodes("descendant::tna:hasRedactedFile", namespaceManager);
            var files = JsonSerializer.Deserialize<List<RelationVariation>>(filesJson, jsonSerializerOptions);
            foreach (var redactedFile in redacted)
            {
                var decodedPath = HttpUtility.UrlDecode(redactedFile.Value);
                var variationName = GetPartialPath(decodedPath);
                var variationId = files?.SingleOrDefault(f => f.Name == variationName &&
                    (decodedPath.Contains($"{f.Location}/") || decodedPath.Contains($"{f.UnderscoredLocation}/")))?.Id;
                IUriNode? redactedVariation = null;
                if (variationId is not null)
                {
                    redactedVariation = await cacheClient.CacheFetch(CacheEntityKind.Variation, variationId, cancellationToken);
                }
                if (redactedVariation is not null)
                {
                    graph.Assert(id, Vocabulary.AssetHasVariation, redactedVariation);
                    var foundFile = false;
                    for (int i = 0; i < xmlRedactedFiles.Count; i++)
                    {
                        if (xmlRedactedFiles.Item(i).InnerText.Equals(redactedFile.Value))
                        {
                            foundFile = true;
                            GraphAssert.Integer(graph, redactedVariation, i + 1, Vocabulary.RedactedVariationSequence);
                            break;
                        }
                    }
                    if (!foundFile)
                    {
                        logger.UnableEstablishRedactedVariationSequence(variationName);
                    }
                }
                else
                {
                    logger.RedactedVariationMissing(variationName);
                }
            }
        }
    }

    private void AddFilmDuration(IGraph graph, IGraph rdf, INode id)
    {
        var foundDuration = rdf.GetTriplesWithPredicate(IngestVocabulary.DurationMins).SingleOrDefault()?.Object;
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
        var foundWebArchive = rdf.GetTriplesWithPredicate(IngestVocabulary.WebArchiveUrl).SingleOrDefault()?.Object;
        if (foundWebArchive is ILiteralNode webArchiveNode && !string.IsNullOrWhiteSpace(webArchiveNode.Value))
        {
            graph.Assert(id, Vocabulary.AssetHasUkGovernmentWebArchive, new UriNode(new Uri(webArchiveNode.AsValuedNode().AsString())));
        }
    }

    private void AddOriginDates(IGraph graph, IGraph rdf, INode id, IGraph existing)
    {
        var foundCoverage = rdf.GetTriplesWithPredicate(IngestVocabulary.Coverage).FirstOrDefault()?.Object;
        var startNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginDateStart).SingleOrDefault()?.Object ??
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginApproximateDateStart).SingleOrDefault()?.Object ??
            CacheClient.NewId;
        var endNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginDateEnd).SingleOrDefault()?.Object ??
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginApproximateDateEnd).SingleOrDefault()?.Object ??
            CacheClient.NewId;
        if (foundCoverage is not null)
        {
            var start = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.StartDate).FirstOrDefault()?.Object as ILiteralNode;
            if (start is not null && string.IsNullOrWhiteSpace(start.Value))
            {
                start = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.FullDate).FirstOrDefault()?.Object as ILiteralNode;
            }
            if (start is not null && !string.IsNullOrWhiteSpace(start.Value))
            {
                var startYmd = dateParser.ParseDate(start.Value);
                if (startYmd.DateKind == DateParser.DateType.Date)
                {
                    graph.Assert(id, Vocabulary.AssetHasOriginDateStart, startNode);
                    GraphAssert.YearMonthDay(graph, startNode, startYmd.Year, startYmd.Month, startYmd.Day);
                    var end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.EndDate).FirstOrDefault()?.Object as ILiteralNode;
                    if (end is not null && string.IsNullOrWhiteSpace(end.Value))
                    {
                        end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.FullDate).FirstOrDefault()?.Object as ILiteralNode;
                    }
                    if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
                    {
                        var endYmd = dateParser.ParseDate(end.Value);
                        if (endYmd.DateKind == DateParser.DateType.Date)
                        {
                            graph.Assert(id, Vocabulary.AssetHasOriginDateEnd, endNode);
                            GraphAssert.YearMonthDay(graph, endNode, endYmd.Year, endYmd.Month, endYmd.Day);
                        }
                    }
                }
                else if (startYmd.DateKind == DateParser.DateType.Approximate)
                {
                    graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateStart, startNode);
                    GraphAssert.YearMonthDay(graph, startNode, startYmd.Year, startYmd.Month, startYmd.Day);
                    var end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.EndDate).FirstOrDefault()?.Object as ILiteralNode;
                    if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
                    {
                        var endYmd = dateParser.ParseDate(end.Value);
                        if (endYmd.DateKind == DateParser.DateType.Approximate)
                        {
                            graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateEnd, endNode);
                            GraphAssert.YearMonthDay(graph, endNode, endYmd.Year, endYmd.Month, endYmd.Day);
                        }
                    }
                }
            }
            else
            {
                var dateRangeNode = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.DateRange).FirstOrDefault()?.Object as ILiteralNode;
                if (dateRangeNode is not null && !string.IsNullOrWhiteSpace(dateRangeNode.Value))
                {
                    var yearRange = dateParser.ParseDateRange(null, dateRangeNode.Value);
                    if (yearRange.DateRangeKind == DateParser.DateRangeType.Date)
                    {
                        graph.Assert(id, Vocabulary.AssetHasOriginDateStart, startNode);
                        GraphAssert.YearMonthDay(graph, startNode, yearRange.FirstYear, yearRange.FirstMonth, yearRange.FirstDay);
                        if (yearRange.SecondYear.HasValue)
                        {
                            graph.Assert(id, Vocabulary.AssetHasOriginDateEnd, endNode);
                            GraphAssert.YearMonthDay(graph, endNode, yearRange.SecondYear, yearRange.SecondMonth, yearRange.SecondDay);
                        }
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
            GraphAssert.Text(graph, id, rdf, IngestVocabulary.Session, Vocabulary.InquirySessionDescription);
        }
    }

    private async Task<IUriNode?> FetchWitnessIdAsync(IGraph graph, IGraph rdf, INode id, int witnessIndex, CancellationToken cancellationToken)
    {
        var foundWitness = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}witness_list_{witnessIndex}"))).SingleOrDefault()?.Object;
        if (foundWitness is ILiteralNode witnessNode && !string.IsNullOrWhiteSpace(witnessNode.Value))
        {
            var foundDescription = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}subject_role_{witnessIndex}"))).SingleOrDefault()?.Object as ILiteralNode;
            if (foundDescription is not null)
            {
                var witnessId = await cacheClient.CacheFetchOrNew(CacheEntityKind.InquiryAppearanceByWitnessAndDescription, [witnessNode.Value, foundDescription.Value], Vocabulary.InquiryWitnessName, cancellationToken);
                GraphAssert.Text(graph, witnessId, witnessNode.Value, Vocabulary.InquiryWitnessName); //TODO: check if can be split
                GraphAssert.Text(graph, witnessId, foundDescription.Value, Vocabulary.InquiryWitnessAppearanceDescription);
                graph.Assert(id, Vocabulary.InquiryAssetHasInquiryAppearance, witnessId);

                return witnessId;
            }
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
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}case_name_{caseIndex}"))] = Vocabulary.CourtCaseName,
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}case_summary_{caseIndex}"))] = Vocabulary.CourtCaseSummary,
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}case_summary_{caseIndex}_judgment"))] = Vocabulary.CourtCaseSummaryJudgment,
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}case_summary_{caseIndex}_reasons_for_judgment"))] = Vocabulary.CourtCaseSummaryReasonsForJudgment
            });
            assert.Date(graph, courtCase, rdf, new Dictionary<IUriNode, IUriNode>()
            {
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}hearing_start_date_{caseIndex}"))] = Vocabulary.CourtCaseHearingStartDate,
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}hearing_end_date_{caseIndex}"))] = Vocabulary.CourtCaseHearingEndDate
            });

            caseIndex++;
            courtCase = await FetchCourtCaseIdAsync(graph, rdf, id, caseIndex, assetReference, cancellationToken);
        }
        if (found)
        {
            GraphAssert.Text(graph, id, rdf, IngestVocabulary.Session, Vocabulary.CourtSessionDescription);
        }
    }

    private async Task<IUriNode?> FetchCourtCaseIdAsync(IGraph graph, IGraph rdf, INode id, int caseIndex, string assetReference, CancellationToken cancellationToken)
    {
        var foundCase = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}case_id_{caseIndex}"))).SingleOrDefault()?.Object;
        if (foundCase is ILiteralNode caseNode && !string.IsNullOrWhiteSpace(caseNode.Value))
        {
            var caseId = await cacheClient.CacheFetchOrNew(CacheEntityKind.CourtCaseByCaseAndAsset, [caseNode.Value, assetReference], Vocabulary.CourtCaseReference, cancellationToken);
            graph.Assert(id, Vocabulary.CourtAssetHasCourtCase, caseId);

            return caseId;
        }

        return null;
    }

    private async Task AddSeal(IGraph graph, IGraph rdf, IGraph existing, INode id, CancellationToken cancellationToken)
    {
        await assert.ExistingOrNewWithRelationshipAsync(graph, id, rdf, IngestVocabulary.TypeOfSeal, CacheEntityKind.SealCategory,
            Vocabulary.SealAssetHasSealCategory, Vocabulary.SealCategoryName, cancellationToken);

        var obverseOrReverse = rdf.GetTriplesWithPredicate(IngestVocabulary.Face).SingleOrDefault()?.Object as ILiteralNode;

        var dateNode = rdf.GetTriplesWithPredicate(IngestVocabulary.DateOfOriginalSeal).FirstOrDefault()?.Object as ILiteralNode;
        if (dateNode is not null && !string.IsNullOrWhiteSpace(dateNode.Value))
        {
            var range = dateParser.ParseDateRange(obverseOrReverse?.Value, dateNode.Value);
            if (range.DateRangeKind == DateParser.DateRangeType.Date)
            {
                var startNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasStartDate).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasStartDate, startNode);
                GraphAssert.YearMonthDay(graph, startNode, range.FirstYear, range.FirstMonth, range.FirstDay);
                if (range.SecondYear.HasValue)
                {
                    var endNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasEndDate).SingleOrDefault()?.Object ??
                        CacheClient.NewId;
                    graph.Assert(id, Vocabulary.SealAssetHasEndDate, endNode);
                    GraphAssert.YearMonthDay(graph, endNode, range.SecondYear, range.SecondMonth, range.SecondDay);
                }
            }
            else if (range.DateRangeKind == DateParser.DateRangeType.IdenticalObverseAndReverse)
            {
                var startObverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseStartDate).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasObverseStartDate, startObverseNode);
                GraphAssert.YearMonthDay(graph, startObverseNode, range.FirstYear, range.FirstMonth, range.FirstDay);
                var startReverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseStartDate).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasReverseStartDate, startReverseNode);
                GraphAssert.YearMonthDay(graph, startReverseNode, range.FirstYear, range.FirstMonth, range.FirstDay);
                if (range.SecondYear.HasValue)
                {
                    var endObverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseEndDate).SingleOrDefault()?.Object ??
                        CacheClient.NewId;
                    graph.Assert(id, Vocabulary.SealAssetHasObverseEndDate, endObverseNode);
                    GraphAssert.YearMonthDay(graph, endObverseNode, range.SecondYear, range.SecondMonth, range.SecondDay);
                    var endReverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseEndDate).SingleOrDefault()?.Object ??
                        CacheClient.NewId;
                    graph.Assert(id, Vocabulary.SealAssetHasReverseEndDate, endReverseNode);
                    GraphAssert.YearMonthDay(graph, endReverseNode, range.SecondYear, range.SecondMonth, range.SecondDay);
                }
            }
            else if (range.DateRangeKind == DateParser.DateRangeType.Obverse)
            {
                var startObverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseStartDate).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasObverseStartDate, startObverseNode);
                GraphAssert.YearMonthDay(graph, startObverseNode, range.FirstYear, range.FirstMonth, range.FirstDay);
                if (range.SecondYear.HasValue)
                {
                    var endObverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseEndDate).SingleOrDefault()?.Object ??
                        CacheClient.NewId;
                    graph.Assert(id, Vocabulary.SealAssetHasObverseEndDate, endObverseNode);
                    GraphAssert.YearMonthDay(graph, endObverseNode, range.SecondYear, range.SecondMonth, range.SecondDay);
                }
            }
            else if (range.DateRangeKind == DateParser.DateRangeType.Reverse)
            {
                var startReverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseStartDate).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasReverseStartDate, startReverseNode);
                GraphAssert.YearMonthDay(graph, startReverseNode, range.FirstYear, range.FirstMonth, range.FirstDay);
                if (range.SecondYear.HasValue)
                {
                    var endReverseNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseEndDate).SingleOrDefault()?.Object ??
                        CacheClient.NewId;
                    graph.Assert(id, Vocabulary.SealAssetHasReverseEndDate, endReverseNode);
                    GraphAssert.YearMonthDay(graph, endReverseNode, range.SecondYear, range.SecondMonth, range.SecondDay);
                }
            }
        }
        var dimensionNode = rdf.GetTriplesWithPredicate(IngestVocabulary.Dimensions).SingleOrDefault()?.Object as ILiteralNode;
        if (dimensionNode is not null && !string.IsNullOrWhiteSpace(dimensionNode.Value))
        {
            var dimension = dimensionParser.ParseCentimetre(obverseOrReverse?.Value, dimensionNode.Value);

            if (dimension.DimensionKind == DimensionParser.DimensionType.Fragment)
            {
                graph.Assert(id, Vocabulary.AssetHasDimension, Vocabulary.FragmentDimension);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.ObverseFragment)
            {
                graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, Vocabulary.FragmentDimension);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.ReverseFragment)
            {
                graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, Vocabulary.FragmentDimension);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.ObverseAndReverseFragment)
            {
                graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, Vocabulary.FragmentDimension);
                graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, Vocabulary.FragmentDimension);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.Obverse)
            {
                var sealAssetHasObverseDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, sealAssetHasObverseDimension);
                GraphAssert.Integer(graph, sealAssetHasObverseDimension, dimension.FirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, sealAssetHasObverseDimension, dimension.SecondMm, Vocabulary.SecondDimensionMillimetre);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.Reverse)
            {
                var sealAssetHasReverseDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, sealAssetHasReverseDimension);
                GraphAssert.Integer(graph, sealAssetHasReverseDimension, dimension.FirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, sealAssetHasReverseDimension, dimension.SecondMm, Vocabulary.SecondDimensionMillimetre);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.Dimension)
            {
                var assetHasDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.AssetHasDimension, assetHasDimension);
                GraphAssert.Integer(graph, assetHasDimension, dimension.FirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, assetHasDimension, dimension.SecondMm, Vocabulary.SecondDimensionMillimetre);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.FragmentObverseSecondReverse)
            {
                var sealAssetHasReverseDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, Vocabulary.FragmentDimension);
                graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, sealAssetHasReverseDimension);
                GraphAssert.Integer(graph, sealAssetHasReverseDimension, dimension.SecondFirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, sealAssetHasReverseDimension, dimension.SecondSecondMm, Vocabulary.SecondDimensionMillimetre);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.FirstObverseFragmentReverse)
            {
                var sealAssetHasObverseDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, sealAssetHasObverseDimension);
                GraphAssert.Integer(graph, sealAssetHasObverseDimension, dimension.FirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, sealAssetHasObverseDimension, dimension.SecondMm, Vocabulary.SecondDimensionMillimetre);
                graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, Vocabulary.FragmentDimension);
            }
            else if (dimension.DimensionKind == DimensionParser.DimensionType.IdenticalObverseAndReverse ||
                dimension.DimensionKind == DimensionParser.DimensionType.FirstObverseSecondReverse)
            {
                var sealAssetHasObverseDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasObverseDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                var sealAssetHasReverseDimension = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SealAssetHasReverseDimension).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                graph.Assert(id, Vocabulary.SealAssetHasObverseDimension, sealAssetHasObverseDimension);
                GraphAssert.Integer(graph, sealAssetHasObverseDimension, dimension.FirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, sealAssetHasObverseDimension, dimension.SecondMm, Vocabulary.SecondDimensionMillimetre);
                graph.Assert(id, Vocabulary.SealAssetHasReverseDimension, sealAssetHasReverseDimension);
                GraphAssert.Integer(graph, sealAssetHasReverseDimension, dimension.SecondFirstMm, Vocabulary.FirstDimensionMillimetre);
                GraphAssert.Integer(graph, sealAssetHasReverseDimension, dimension.SecondSecondMm, Vocabulary.SecondDimensionMillimetre);
            }
        }
    }

    private async Task AddCopyrightAsync(IGraph graph, IGraph rdf, INode id, CancellationToken cancellationToken)
    {
        var copyrights = rdf.GetTriplesWithPredicate(IngestVocabulary.Rights).Select(t => t.Object)
            .Where(o => !string.IsNullOrWhiteSpace(o.ToString()));
        foreach (var copyright in copyrights)
        {
            var cTitle = copyright switch
            {
                ILiteralNode literalRight => literalRight.Value,
                IUriNode uriRight => uriRight.Uri.Segments.Last().Replace('_', ' '),
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(cTitle))
            {
                var copyrightId = await cacheClient.CacheFetchOrNew(CacheEntityKind.Copyright, cTitle, Vocabulary.CopyrightTitle, cancellationToken);
                graph.Assert(id, Vocabulary.AssetHasCopyright, copyrightId);
            }
        }
    }

    private void AddLegalStatus(IGraph graph, IGraph rdf, INode id)
    {
        var legal = rdf.GetTriplesWithPredicate(IngestVocabulary.LegalStatus).SingleOrDefault()?.Object;
        if (legal is IUriNode legalUri)
        {
            var statusType = legalUri.Uri.Segments.Last() switch
            {
                "Public_Record(s)" or "Public_record" or "Public_Record" or "PublicRecord" =>
                    Vocabulary.PublicRecord,
                "Welsh_Public_Record(s)" or "Welsh_Public_Record" => Vocabulary.WelshPublicRecord,
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

    private static string GetPartialPath(string path) => path.Substring(path.LastIndexOf('/') + 1);

    private record RelationVariation(string Id, string Location, string Name)
    {
        internal string UnderscoredLocation = Location.Replace(' ', '_');
    }
}
