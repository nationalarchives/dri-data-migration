using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Diagnostics.Metrics;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

public class Adm158SubsetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient,
    ILogger<Adm158SubsetDeliverableUnitIngest> logger, IMeterFactory meterFactory) :
    StagingIngest<DriAdm158SubsetDeliverableUnit>(sparqlClient, logger, meterFactory, "Adm158SubsetDeliverableUnitGraph")
{
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);

    internal override async Task<Graph?> BuildAsync(IGraph existing,
        DriAdm158SubsetDeliverableUnit dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.SubsetReference, driId);
        if (id is null)
        {
            logger.SubsetNotFound(dri.Id);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.SubsetReference, driId);
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.Adm158SubsetDriXml);
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

        var person = AddPerson(graph, existing, rdf, id);
        AddAge(graph, rdf, person);
        await AddPlaceOfBirthAsync(graph, rdf, person, cancellationToken);
        await AddNavyAsync(graph, existing, rdf, person, cancellationToken);
    }

    private static IUriNode AddPerson(IGraph graph, IGraph existing, IGraph rdf, IUriNode id)
    {
        var person = existing.GetSingleUriNode(id, Vocabulary.SubsetHasPerson) ?? CacheClient.NewId;
        graph.Assert(id, Vocabulary.SubsetHasPerson, person);

        GraphAssert.Text(graph, person, rdf, new Dictionary<IUriNode, IUriNode>()
        {
            [IngestVocabulary.Surname] = Vocabulary.PersonFamilyName,
            [IngestVocabulary.SurnameOther] = Vocabulary.PersonAlternativeFamilyName,
            [IngestVocabulary.Forenames] = Vocabulary.PersonGivenName,
            [IngestVocabulary.ForenamesOther] = Vocabulary.PersonAlternativeGivenName
        }, "*");

        return person;
    }

    private static void AddAge(IGraph graph, IGraph rdf, IUriNode person)
    {
        var ageYears = rdf.GetSingleLiteral(IngestVocabulary.AgeYears);
        if (ageYears is not null && int.TryParse(ageYears.Value, out var years))
        {
            var age = $"P{years}Y";
            var ageMonths = rdf.GetSingleLiteral(IngestVocabulary.AgeMonths);
            if (ageMonths is not null && int.TryParse(ageMonths.Value, out var months))
            {
                age = $"P{years}Y{months}M";
            }
            graph.Assert(person, Vocabulary.PersonAge, new LiteralNode(age, new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
        }
    }

    private async Task AddPlaceOfBirthAsync(IGraph graph, IGraph rdf, IUriNode person,
        CancellationToken cancellationToken)
    {
        var placeOfBirthParish = rdf.GetSingleText(IngestVocabulary.PlaceOfBirthParish);
        var placeOfBirthTown = rdf.GetSingleText(IngestVocabulary.PlaceOfBirthTown);
        var placeOfBirthCounty = rdf.GetSingleText(IngestVocabulary.PlaceOfBirthCounty);
        var placeOfBirthCountry = rdf.GetSingleText(IngestVocabulary.PlaceOfBirthCountry);

        var place = $"{SpaceOrComma(placeOfBirthParish)}{SpaceOrComma(placeOfBirthTown)}{SpaceOrComma(placeOfBirthCounty)}{SpaceOrComma(placeOfBirthCountry)}".Trim().Trim(',');

        var birthAddress = await cacheClient.CacheFetchOrNew(CacheEntityKind.GeographicalPlace,
            place, Vocabulary.GeographicalPlaceName, cancellationToken);
        graph.Assert(person, Vocabulary.PersonHasBirthAddress, birthAddress);
        GraphAssert.Text(graph, birthAddress!, placeOfBirthParish, Vocabulary.Parish, "*");
        GraphAssert.Text(graph, birthAddress!, placeOfBirthTown, Vocabulary.Town, "*");
        GraphAssert.Text(graph, birthAddress!, placeOfBirthCounty, Vocabulary.County, "*");
        GraphAssert.Text(graph, birthAddress!, placeOfBirthCountry, Vocabulary.Country, "*");
    }

    private static string SpaceOrComma(string? text) => string.IsNullOrWhiteSpace(text) || text == "*" ? string.Empty : $"{text}, ";

    private async Task AddNavyAsync(IGraph graph, IGraph existing,
        IGraph rdf, IUriNode person, CancellationToken cancellationToken)
    {
        var divisionDescription = rdf.GetSingleText(IngestVocabulary.DivisionDescription);
        if (!string.IsNullOrWhiteSpace(divisionDescription))
        {
            var membership = existing.GetSingleUriNode(person, Vocabulary.PersonHasNavyMembership) ?? CacheClient.NewId;
            graph.Assert(person, Vocabulary.PersonHasNavyMembership, membership);
            var division = await cacheClient.CacheFetchOrNew(CacheEntityKind.NavyDivision, divisionDescription,
                Vocabulary.NavyDivisionName, cancellationToken);
            graph.Assert(membership, Vocabulary.NavyMembershipHasNavyDivision, division);
        }
    }
}
