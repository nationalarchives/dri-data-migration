using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Xml;
using VDS.RDF;

namespace Staging;

public class Wo409SubsetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient,
    ILogger<Wo409SubsetDeliverableUnitIngest> logger) :
    StagingIngest<DriWo409SubsetDeliverableUnit>(sparqlClient, logger, "Wo409SubsetDeliverableUnitGraph")
{
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);
    private readonly DateParser dateParser = new(logger);
    private readonly Wo409SubsetDeliverableUnitRelationIngest relationIngest = new(logger);
    private readonly IUriNode wo409 = new UriNode(new Uri("http://example.com/subject"));
    private readonly Uri givenName = new("http://example.com/given");
    private readonly Uri familyName = new("http://example.com/familyName");

    internal override async Task<Graph?> BuildAsync(IGraph existing,
        DriWo409SubsetDeliverableUnit dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.AssetDriId, driId);
        if (id is null)
        {
            logger.AssetNotFound(dri.Id);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AssetDriId, driId);
        GraphAssert.Text(graph, id, dri.ParentId, Vocabulary.Wo409SubsetDriId);
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.Wo409SubsetDriXml);
            await ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
        }

        return graph;
    }

    private async Task ExtractXmlData(IGraph graph, IGraph existing, IUriNode id,
        string xml, CancellationToken cancellationToken)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var rdf = rdfXmlLoader.GetRdf(doc);
        if (rdf is null)
        {
            logger.AssetXmlMissingRdf(id.Uri);
            return;
        }

        var subjectTriple = rdf.GetTriplesWithSubjectPredicate(wo409, IngestVocabulary.Subject).SingleOrDefault();
        if (subjectTriple is null)
        {
            return;
        }

        var person = AddPerson(graph, existing, rdf, id);
        if (person is null)
        {
            return;
        }

        GraphAssert.Text(graph, person, rdf, IngestVocabulary.NationalRegistrationNumber,
            Vocabulary.NationalRegistrationNumber);

        var contactPoint = await GetAddressAsync(rdf, subjectTriple.Object, cancellationToken);
        if (contactPoint is not null)
        {
            graph.Assert(person, Vocabulary.PersonHasContactPoint, contactPoint);
        }

        await AddBirthAsync(graph, existing, rdf, subjectTriple.Object, person, cancellationToken);
        await AddPlaceAsync(graph, existing, rdf, person, cancellationToken);
        relationIngest.AddRelation(graph, existing, rdf, subjectTriple.Object, person);
    }

    private IUriNode? AddPerson(IGraph graph, IGraph existing, IGraph rdf, INode id)
    {
        var names = rdf.GetTriplesWithPredicate(IngestVocabulary.NamePart).Select(t => t.Object).Cast<ILiteralNode>();
        var firstName = names.SingleOrDefault(n => n.DataType == givenName)?.Value;
        var lastName = names.SingleOrDefault(n => n.DataType == familyName)?.Value;
        var fullName = string.IsNullOrWhiteSpace(firstName) ? null :
            string.IsNullOrWhiteSpace(lastName) ? firstName : $"{firstName} {lastName}";
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }
        var previousMilitaryService = rdf.GetTriplesWithPredicate(IngestVocabulary.PreviousMilitaryService)
            .SingleOrDefault()?.Object as ILiteralNode;
        bool isVeteran = previousMilitaryService?.Value == "true";
        var person = (isVeteran ?
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasVeteran).SingleOrDefault()?.Object as IUriNode :
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasPerson).SingleOrDefault()?.Object as IUriNode) ??
            CacheClient.NewId;
        graph.Assert(id, isVeteran ? Vocabulary.AssetHasVeteran : Vocabulary.AssetHasPerson, person);
        GraphAssert.Text(graph, person, firstName, Vocabulary.PersonGivenName);
        GraphAssert.Text(graph, person, lastName, Vocabulary.PersonFamilyName);
        GraphAssert.Text(graph, person, fullName, Vocabulary.PersonFullName);

        return person;
    }

    private async Task AddBirthAsync(IGraph graph, IGraph existing, IGraph rdf, INode subjectId,
        IUriNode person, CancellationToken cancellationToken)
    {
        var birth = rdf.GetTriplesWithSubjectPredicate(subjectId, IngestVocabulary.Birth)
            .SingleOrDefault()?.Object as IBlankNode;
        if (birth is not null)
        {
            var birthDate = rdf.GetSingleLiteral(birth, IngestVocabulary.Date);
            if (birthDate is not null && !string.IsNullOrWhiteSpace(birthDate.Value) &&
                dateParser.TryParseDate(birthDate.Value, out var birthDt))
            {
                var dob = existing.GetSingleUriNode(person, Vocabulary.PersonHasDateOfBirth) ?? CacheClient.NewId;
                graph.Assert(person, Vocabulary.PersonHasDateOfBirth, dob);
                GraphAssert.YearMonthDay(graph, dob, birthDt!.Year, birthDt!.Month, birthDt!.Day, birthDate.Value);
            }
            var birthAddress = await GetAddressAsync(rdf, birth, cancellationToken);
            if (birthAddress is not null)
            {
                graph.Assert(person, Vocabulary.PersonHasBirthAddress, birthAddress);
            }
        }
    }

    private async Task AddPlaceAsync(IGraph graph, IGraph existing,
        IGraph rdf, IUriNode person, CancellationToken cancellationToken)
    {
        var place = rdf.GetTriplesWithPredicate(IngestVocabulary.County).SingleOrDefault()?.Object as ILiteralNode;
        if (place is not null && !string.IsNullOrWhiteSpace(place.Value))
        {
            var battalionNumber = rdf.GetTriplesWithPredicate(IngestVocabulary.References).SingleOrDefault()?.Object as ILiteralNode;
            if (battalionNumber is not null && !string.IsNullOrWhiteSpace(battalionNumber.Value))
            {
                var membership = existing.GetTriplesWithSubjectPredicate(person, Vocabulary.PersonHasBattalionMembership).SingleOrDefault()?.Object ??
                    CacheClient.NewId;
                var unitAndPlace = $"{battalionNumber.Value} {place.Value}";
                var armyUnit = await cacheClient.CacheFetchOrNew(CacheEntityKind.Battalion, unitAndPlace,
                    Vocabulary.BattalionName, cancellationToken);
                graph.Assert(person, Vocabulary.PersonHasBattalionMembership, membership);
                graph.Assert(membership, Vocabulary.BattalionMembershipHasBattalion, armyUnit);
            }
        }
    }

    private async Task<IUriNode?> GetAddressAsync(IGraph rdf, INode subjectId, CancellationToken cancellationToken)
    {
        var address = rdf.GetTriplesWithSubjectPredicate(subjectId, IngestVocabulary.Address)
            .SingleOrDefault()?.Object as IBlankNode;
        if (address is not null)
        {
            var addressText = rdf.GetTriplesWithSubjectPredicate(address, IngestVocabulary.AddressString)
                .SingleOrDefault()?.Object as ILiteralNode;
            if (addressText is not null && !string.IsNullOrWhiteSpace(addressText.Value))
            {
                return await cacheClient.CacheFetchOrNew(CacheEntityKind.GeographicalPlace,
                    addressText.Value, Vocabulary.GeographicalPlaceName, cancellationToken);
            }
        }
        return null;
    }
}
