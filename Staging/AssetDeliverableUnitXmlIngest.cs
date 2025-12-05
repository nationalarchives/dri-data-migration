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
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);
    private readonly AssetDeliverableUnitOriginDateIngest dateIngest = new(logger);
    private readonly AssetDeliverableUnitSealIngest sealIngest = new(logger, cacheClient);
    private readonly DateParser dateParser = new(logger);
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task ExtractXmlData(IGraph graph, IGraph existing,
        IUriNode id, string xml, string filesJson,
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

        GraphAssert.Text(graph, id, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.BatchIdentifier] = Vocabulary.BatchDriId,
            [IngestVocabulary.TdrConsignmentRef] = Vocabulary.ConsignmentTdrId,
            [IngestVocabulary.Description] = Vocabulary.AssetDescription,
            [IngestVocabulary.ContentManagementSystemContainer] = Vocabulary.AssetDescription,
            [IngestVocabulary.Summary] = Vocabulary.AssetDescription,
            [IngestVocabulary.AdditionalInformation] = Vocabulary.AssetDescription,
            [IngestVocabulary.ItemDescription] = Vocabulary.AssetDescription,
            [IngestVocabulary.AdministrativeBackground] = Vocabulary.AssetSummary,
            [IngestVocabulary.RelatedMaterial] = Vocabulary.AssetRelationDescription,
            [IngestVocabulary.TransRelatedMaterial] = Vocabulary.AssetRelationDescription,
            [IngestVocabulary.RelatedIaid] = Vocabulary.AssetRelationIdentifier,
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
        var curatedDate = rdf.GetSingleText(IngestVocabulary.CuratedDate);
        if (!string.IsNullOrEmpty(curatedDate))
        {
            if (DateTimeOffset.TryParse(curatedDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var curatedDT))
            {
                GraphAssert.DateTime(graph, id, curatedDT, Vocabulary.AssetAlternativeModifiedAt);
            }
            else
            {
                logger.UnrecognizedDateFormat(curatedDate);
            }
        }
        
        AddNames(graph, doc, id);
        await AddVariationRelationsAsync(graph, rdf, id, doc, filesJson, cancellationToken);
        AddFilmDuration(graph, rdf, id);
        AddWebArchive(graph, rdf, id);
        AddCourtCases(graph, rdf, id, existing);
        AddWitness(graph, rdf, id, existing);

        dateIngest.AddOriginDates(graph, rdf, id, existing);

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
        graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, retention, rdf,
            IngestVocabulary.HeldBy, CacheEntityKind.FormalBody,
            Vocabulary.RetentionHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

        var creation = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasCreation).SingleOrDefault()?.Object ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.AssetHasCreation, creation);
        await GraphAssert.ExistingOrNewWithRelationshipAsync(cacheClient, graph, creation, rdf,
            IngestVocabulary.Creator, CacheEntityKind.FormalBody,
            Vocabulary.CreationHasFormalBody, Vocabulary.FormalBodyName, cancellationToken);

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

    private async Task AddVariationRelationsAsync(IGraph graph, IGraph rdf, IUriNode id,
        XmlDocument doc, string filesJson, CancellationToken cancellationToken)
    {
        var redacted = rdf.GetTriplesWithPredicate(IngestVocabulary.HasRedactedFile).Select(t => t.Object).Cast<ILiteralNode>();
        if (redacted.Any())
        {
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("tna", IngestVocabulary.TnaNamespace.ToString());
            var xmlRedactedFiles = doc.SelectNodes("descendant::tna:hasRedactedFile", namespaceManager);
            if (xmlRedactedFiles is null)
            {
                return;
            }
            var files = JsonSerializer.Deserialize<List<RelationVariation>>(filesJson, jsonSerializerOptions);
            foreach (var redactedFile in redacted)
            {
                var decodedPath = HttpUtility.UrlDecode(redactedFile.Value);
                var variationName = GetPartialPath(decodedPath);
                var variationId = files?.SingleOrDefault(f =>
                    f.Name == variationName && HasPathPartialMatch(decodedPath, f.Location))?.Id;
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
                        if (xmlRedactedFiles.Item(i)?.InnerText.Equals(redactedFile.Value) == true)
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

    private void AddWitness(IGraph graph, IGraph rdf, INode id, IGraph existing)
    {
        var found = false;
        var witnessIndex = 1;
        var foundWitness = FetchWitnessId(graph, rdf, id, existing, witnessIndex); //TODO: check if names could be split on ',' and 'and' and turned into entities
        while (foundWitness is not null)
        {
            found = true;
            witnessIndex++;
            foundWitness = FetchWitnessId(graph, rdf, id, existing, witnessIndex);
        }
        if (found)
        {
            GraphAssert.Text(graph, id, rdf, IngestVocabulary.Session, Vocabulary.InquirySessionDescription);
        }
    }

    private static IUriNode? FetchWitnessId(IGraph graph, IGraph rdf, INode id, IGraph existing, int witnessIndex)
    {
        var foundWitness = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}witness_list_{witnessIndex}"))).SingleOrDefault()?.Object;
        if (foundWitness is ILiteralNode witnessNode && !string.IsNullOrWhiteSpace(witnessNode.Value))
        {
            var foundDescription = rdf.GetTriplesWithPredicate(new UriNode(new($"{IngestVocabulary.TnaNamespace}subject_role_{witnessIndex}"))).SingleOrDefault()?.Object as ILiteralNode;
            if (foundDescription is not null)
            {
                var witnessId = existing.GetTriplesWithPredicateObject(Vocabulary.InquiryWitnessName, new LiteralNode(witnessNode.Value))
                    .SingleOrDefault(t => existing.ContainsTriple(new Triple(t.Subject, Vocabulary.InquiryWitnessAppearanceDescription, new LiteralNode(foundDescription.Value))))?.Subject as IUriNode
                    ?? CacheClient.NewId;
                GraphAssert.Integer(graph, witnessId, witnessIndex, Vocabulary.InquiryAppearanceSequence);
                GraphAssert.Text(graph, witnessId, witnessNode.Value, Vocabulary.InquiryWitnessName); //TODO: check if can be split
                GraphAssert.Text(graph, witnessId, foundDescription.Value, Vocabulary.InquiryWitnessAppearanceDescription);
                graph.Assert(id, Vocabulary.InquiryAssetHasInquiryAppearance, witnessId);

                return witnessId;
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

    private IUriNode? FetchCourtCaseId(IGraph graph, IGraph rdf, INode id, IGraph existing, int caseIndex)
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

    private static string GetPartialPath(string path) => path.Substring(path.LastIndexOf('/') + 1);

    private sealed record RelationVariation(string Id, string Location, string Name);

    private static bool HasPathPartialMatch(string fullPath, string partialPath)
    {
        var fullPathSegemnts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return partialPath.Split('/', StringSplitOptions.RemoveEmptyEntries).All(p =>
            fullPathSegemnts.Contains(p) || fullPathSegemnts.Contains(p.Replace(' ', '_')));
    }
}
