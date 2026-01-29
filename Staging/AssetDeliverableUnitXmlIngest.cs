using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Globalization;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

internal class AssetDeliverableUnitXmlIngest(ILogger logger, ICacheClient cacheClient, IAssetDeliverableUnitRelation assetDeliverableUnitRelation)
{
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);
    private readonly AssetDeliverableUnitOriginDateIngest dateIngest = new(logger);
    private readonly AssetDeliverableUnitSealIngest sealIngest = new(logger, cacheClient);
    private readonly AssetDeliverableUnitVariationRelationIngest variationRelationIngest = new(logger, cacheClient);
    private readonly DateParser dateParser = new(logger);


    public async Task ExtractXmlData(IGraph graph, IGraph existing,
        IUriNode id, string xml, string filesJson,
        CancellationToken cancellationToken)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var rdf = rdfXmlLoader.GetRdf(doc);
        if (rdf is null)
        {
            logger.AssetXmlMissingRdf(id.Uri);
            return;
        }

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.BatchIdentifier] = Vocabulary.BatchDriId,
            [IngestVocabulary.TdrConsignmentRef] = Vocabulary.ConsignmentTdrReference,
            [IngestVocabulary.TdrFileReference] = Vocabulary.FileTdrReference,
            [IngestVocabulary.TdrParentReference] = Vocabulary.ParentTdrReference,
            [IngestVocabulary.TdrUuid] = Vocabulary.AssetTdrId,
            [IngestVocabulary.Description] = Vocabulary.AssetDescription,
            [IngestVocabulary.ContentManagementSystemContainer] = Vocabulary.AssetDescription,
            [IngestVocabulary.Summary] = Vocabulary.AssetDescription,
            [IngestVocabulary.AdditionalInformation] = Vocabulary.AssetDescription,
            [IngestVocabulary.ItemDescription] = Vocabulary.AssetDescription,
            [IngestVocabulary.AdministrativeBackground] = Vocabulary.AssetSummary,
            [IngestVocabulary.RelatedMaterial] = Vocabulary.AssetRelationDescription,
            [IngestVocabulary.TransRelatedMaterial] = Vocabulary.AssetRelationDescription,
            [IngestVocabulary.PhysicalDescription] = Vocabulary.AssetPhysicalDescription,
            [IngestVocabulary.PhysicalFormat] = Vocabulary.AssetPhysicalDescription,
            [IngestVocabulary.RestrictionOnUse] = Vocabulary.AssetUsageRestrictionDescription,
            [IngestVocabulary.FormerReferenceTna] = Vocabulary.AssetPastReference,
            [IngestVocabulary.FormerReferenceDepartment] = Vocabulary.AssetPreviousReference,
            [IngestVocabulary.Classification] = Vocabulary.AssetTag,
            [IngestVocabulary.InternalDepartment] = Vocabulary.AssetSourceInternalName,
            [IngestVocabulary.FilmMaker] = Vocabulary.FilmProductionCompanyName,
            [IngestVocabulary.FilmName] = Vocabulary.FilmTitle,
            [IngestVocabulary.Photographer] = Vocabulary.PhotographerDescription,
            [IngestVocabulary.PaperNumber] = Vocabulary.PaperNumber,
            [IngestVocabulary.PoorLawUnionNumber] = Vocabulary.PoorLawUnionNumber,
            [IngestVocabulary.SealOwner] = Vocabulary.SealOwnerName, //TODO: check if can be turned into entities
            [IngestVocabulary.ColourOfOriginalSeal] = Vocabulary.SealColour,
            [IngestVocabulary.SeparatedMaterial] = Vocabulary.AssetConnectedAssetNote,
            [IngestVocabulary.AttachmentFormerReference] = Vocabulary.EmailAttachmentReference,
            [IngestVocabulary.CuratedDateNote] = Vocabulary.AssetAlternativeModifiedAtNote
        });
        GraphAssert.Date(dateParser, graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.Session_date] = Vocabulary.CourtSessionDate,
            [IngestVocabulary.Hearing_date] = Vocabulary.InquiryHearingDate
        });
        GraphAssert.Integer(logger, graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.StartImageNumber] = Vocabulary.ImageSequenceStart,
            [IngestVocabulary.EndImageNumber] = Vocabulary.ImageSequenceEnd
        });

        var modified = rdf.GetSingleText(IngestVocabulary.Modified);
        if (!string.IsNullOrEmpty(modified))
        {
            if (DateTimeOffset.TryParse(modified, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var modifiedDT))
            {
                GraphAssert.DateTime(graph, id, modifiedDT, Vocabulary.AssetModifiedAt);
            }
            else
            {
                logger.UnrecognizedDateFormat(modified);
            }
        }

        AddNames(graph, doc, id);
        await assetDeliverableUnitRelation.AddAssetRelationAsync(graph, rdf, id, cacheClient, cancellationToken);
        await variationRelationIngest.AddVariationRelationsAsync(graph, rdf, id, doc, filesJson, cancellationToken);
        AddFilmDuration(graph, rdf, id);
        AddWebArchive(graph, rdf, id);
        AddCourtCases(graph, rdf, id, existing);
        await AddWitnessAsync(graph, rdf, id, existing, cancellationToken);
        AddModifiedDate(graph, rdf, id, existing);

        dateIngest.AddOriginDates(graph, rdf, id, existing);

        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.EvidenceProvidedBy, CacheEntityKind.EvidenceProvider, Vocabulary.InquiryAssetHasEvidenceProvider,
            Vocabulary.InquiryEvidenceProviderName, cancellationToken);
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.Investigation, CacheEntityKind.Investigation, Vocabulary.InquiryAssetHasInquiryInvestigation,
            Vocabulary.InquiryInvestigationName, [";"], cancellationToken);
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.Language, CacheEntityKind.Language, Vocabulary.AssetHasLanguage,
            Vocabulary.LanguageName, cancellationToken);
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.Counties, CacheEntityKind.GeographicalPlace,
            Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, id, rdf,
            IngestVocabulary.County, CacheEntityKind.GeographicalPlace,
            Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName, cancellationToken);

        var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasRetention).SingleOrDefault()?.Object ?? CacheClient.NewId;
        var retentionFormalBody = await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, retention, rdf,
            IngestVocabulary.HeldBy, CacheEntityKind.FormalBody,
            Vocabulary.RetentionHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);
        if (retentionFormalBody is not null)
        {
            graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        }

        var creation = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasCreation).SingleOrDefault()?.Object ?? CacheClient.NewId;
        var creationFormalBody = await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, creation, rdf,
            IngestVocabulary.Creator, CacheEntityKind.FormalBody,
            Vocabulary.CreationHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);
        if (creationFormalBody is not null)
        {
            graph.Assert(id, Vocabulary.AssetHasCreation, creation);
        }

        await AddCopyrightAsync(graph, rdf, id, cancellationToken);
        AddLegalStatus(graph, rdf, id);
        await sealIngest.AddSealAsync(graph, rdf, existing, id, cancellationToken);
        await AddPersonAsync(graph, rdf, id, existing, cancellationToken);
    }

    private static void AddNames(IGraph graph, XmlDocument doc, IUriNode id)
    {
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("dcterms", IngestVocabulary.DctermsNamespace.ToString());
        var titles = doc.SelectNodes("descendant::dcterms:title", namespaceManager);
        if (titles is null)
        {
            return;
        }
        for (int i = 0; i < titles.Count; i++)
        {
            var title = titles.Item(i)?.InnerText;
            if (title is null)
            {
                continue;
            }
            if (i == 0)
            {
                GraphAssert.Text(graph, id, title, Vocabulary.AssetName);
            }
            else
            {
                GraphAssert.Text(graph, id, title, Vocabulary.AssetAlternativeName);
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

    private async Task AddWitnessAsync(IGraph graph, IGraph rdf, INode id, IGraph existing, CancellationToken cancellationToken)
    {
        var found = false;
        var witnessIndex = 1;
        var foundWitness = await FetchWitnessIdAsync(graph, rdf, id, existing, witnessIndex, cancellationToken);
        while (foundWitness is not null)
        {
            found = true;
            witnessIndex++;
            foundWitness = await FetchWitnessIdAsync(graph, rdf, id, existing, witnessIndex, cancellationToken);
        }
        if (found)
        {
            GraphAssert.Text(graph, id, rdf, IngestVocabulary.Session, Vocabulary.InquirySessionDescription);
        }
    }

    private async Task<IUriNode?> FetchWitnessIdAsync(IGraph graph, IGraph rdf, INode id, IGraph existing, int witnessIndex, CancellationToken cancellationToken)
    {
        var witnessNamePredicate = new UriNode(new($"{IngestVocabulary.TnaNamespace}witness_list_{witnessIndex}"));
        var foundWitness = rdf.GetTriplesWithPredicate(witnessNamePredicate).SingleOrDefault()?.Object;
        if (foundWitness is ILiteralNode witnessNode && !string.IsNullOrWhiteSpace(witnessNode.Value))
        {
            var foundDescription = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}subject_role_{witnessIndex}"))).SingleOrDefault()?.Object as ILiteralNode;
            if (foundDescription is not null)
            {
                var witnessAppearanceId = existing.GetTriplesWithPredicateObject(Vocabulary.InquiryAppearanceSequence, new LongNode(witnessIndex)).SingleOrDefault()?.Subject as IUriNode
                    ?? CacheClient.NewId;
                GraphAssert.Integer(graph, witnessAppearanceId, witnessIndex, Vocabulary.InquiryAppearanceSequence);
                await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, witnessAppearanceId, rdf,
                    witnessNamePredicate, CacheEntityKind.Witness, Vocabulary.InquiryAppearanceHasInquiryWitness,
                    Vocabulary.InquiryWitnessName, [",", " and "], cancellationToken);
                GraphAssert.Text(graph, witnessAppearanceId, foundDescription.Value, Vocabulary.InquiryWitnessAppearanceDescription);
                graph.Assert(id, Vocabulary.InquiryAssetHasInquiryAppearance, witnessAppearanceId);

                return witnessAppearanceId;
            }
        }

        return null;
    }

    private void AddCourtCases(IGraph graph, IGraph rdf, INode id, IGraph existing)
    {
        var found = false;
        var caseIndex = 1;
        var courtCase = FetchCourtCaseId(graph, rdf, id, existing, caseIndex);
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
            GraphAssert.Date(dateParser, graph, courtCase, rdf, new Dictionary<IUriNode, IUriNode>()
            {
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}hearing_start_date_{caseIndex}"))] = Vocabulary.CourtCaseHearingStartDate,
                [new UriNode(new($"{IngestVocabulary.TnaNamespace}hearing_end_date_{caseIndex}"))] = Vocabulary.CourtCaseHearingEndDate
            });

            caseIndex++;
            courtCase = FetchCourtCaseId(graph, rdf, id, existing, caseIndex);
        }
        if (found)
        {
            GraphAssert.Text(graph, id, rdf, IngestVocabulary.Session, Vocabulary.CourtSessionDescription);
        }
    }

    private static IUriNode? FetchCourtCaseId(IGraph graph, IGraph rdf, INode id, IGraph existing, int caseIndex)
    {
        var foundCase = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}case_id_{caseIndex}"))).SingleOrDefault()?.Object;
        if (foundCase is ILiteralNode caseNode && !string.IsNullOrWhiteSpace(caseNode.Value))
        {
            var caseReference = new LiteralNode(caseNode.Value);
            var caseId = existing.GetTriplesWithPredicateObject(Vocabulary.CourtCaseReference, caseReference).SingleOrDefault()?.Subject as IUriNode
                    ?? CacheClient.NewId;
            graph.Assert(id, Vocabulary.CourtAssetHasCourtCase, caseId);
            GraphAssert.Integer(graph, caseId, caseIndex, Vocabulary.CourtCaseSequence);
            graph.Assert(caseId, Vocabulary.CourtCaseReference, caseReference);

            return caseId;
        }

        return null;
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
                "Welsh_Public_Record(s)" or "Welsh_Public_Record" or "Welsh_public_record" => Vocabulary.WelshPublicRecord,
                "Not_Public_Record(s)" or "Not_Public_Record" => Vocabulary.NotPublicRecord,
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

    private async Task AddPersonAsync(IGraph graph, IGraph rdf, INode id, IGraph existing, CancellationToken cancellationToken)
    {
        var surname = rdf.GetTriplesWithPredicate(IngestVocabulary.Surname).SingleOrDefault()?.Object;
        if (surname is ILiteralNode surnameNode && !string.IsNullOrWhiteSpace(surnameNode.Value))
        {
            var person = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasPerson).SingleOrDefault()?.Object as IUriNode
                ?? CacheClient.NewId;
            graph.Assert(id, Vocabulary.AssetHasPerson, person);
            GraphAssert.Text(graph, person, rdf, new Dictionary<IUriNode, IUriNode>
            {
                [IngestVocabulary.Surname] = Vocabulary.PersonFamilyName,
                [IngestVocabulary.Forenames] = Vocabulary.PersonGivenName,
                [IngestVocabulary.OfficialNumber] = Vocabulary.SeamanServiceNumber
            });

            var birth = rdf.GetTriplesWithPredicate(IngestVocabulary.BirthDate).SingleOrDefault()?.Object;
            if (birth is not null)
            {
                var birthDate = rdf.GetTriplesWithSubjectPredicate(birth, IngestVocabulary.TransDate).SingleOrDefault()?.Object as ILiteralNode;
                if (birthDate is not null && dateParser.TryParseDate(birthDate.Value, out var birthDt))
                {
                    var dob = existing.GetTriplesWithSubjectPredicate(person, Vocabulary.PersonHasDateOfBirth).SingleOrDefault()?.Object as IUriNode
                        ?? CacheClient.NewId;
                    graph.Assert(person, Vocabulary.PersonHasDateOfBirth, dob);
                    GraphAssert.YearMonthDay(graph, dob, birthDt!.Year, birthDt!.Month, birthDt!.Day);
                }
            }

            var placeOfBirth = rdf.GetTriplesWithPredicate(IngestVocabulary.PlaceOfBirth).SingleOrDefault()?.Object as ILiteralNode;
            if (placeOfBirth is ILiteralNode placeOfBirthNode && !string.IsNullOrWhiteSpace(placeOfBirthNode.Value))
            {
                var birthAddress = await cacheClient.CacheFetchOrNew(CacheEntityKind.GeographicalPlace,
                    placeOfBirth.Value, Vocabulary.GeographicalPlaceName, cancellationToken);
                graph.Assert(person, Vocabulary.PersonHasBirthAddress, birthAddress);
            }
        }
    }

    private void AddModifiedDate(IGraph graph, IGraph rdf, INode id, IGraph existing)
    {
        var curatedDate = rdf.GetSingleText(IngestVocabulary.CuratedDate);
        if (!string.IsNullOrEmpty(curatedDate))
        {
            if (DateTimeOffset.TryParse(curatedDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var curatedDT))
            {
                GraphAssert.DateTime(graph, id, curatedDT, Vocabulary.AssetAlternativeModifiedAt);
            }
            else
            {
                var range = dateParser.ParseDateRange(null, curatedDate);
                if (range.DateRangeKind == DateParser.DateRangeType.Date)
                {
                    var startNode = existing.GetSingleUriNode(Vocabulary.AssetHasAlternativeModifiedDateStart) ?? CacheClient.NewId;
                    graph.Assert(id, Vocabulary.AssetHasAlternativeModifiedDateStart, startNode);
                    GraphAssert.YearMonthDay(graph, startNode, range.FirstYear, range.FirstMonth, range.FirstDay);
                    if (range.SecondYear.HasValue)
                    {
                        var endNode = existing.GetSingleUriNode(Vocabulary.AssetHasAlternativeModifiedDateEnd) ?? CacheClient.NewId;
                        graph.Assert(id, Vocabulary.AssetHasAlternativeModifiedDateEnd, endNode);
                        GraphAssert.YearMonthDay(graph, endNode, range.SecondYear, range.SecondMonth, range.SecondDay);
                    }
                }
                else
                {
                    logger.UnrecognizedDateFormat(curatedDate);
                }
            }
        }
    }
}
