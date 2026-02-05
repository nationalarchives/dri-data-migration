using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class PersonMapper
{
    internal static Person? GetIndividual(IGraph graph)
    {
        var civil = graph.GetSingleUriNode(Vocabulary.AssetHasPerson);
        var veteran = graph.GetSingleUriNode(Vocabulary.AssetHasVeteran);
        var person = civil ?? veteran;
        if (person is null)
        {
            return null;
        }
        var address = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasContactPoint, Vocabulary.GeographicalPlaceName)?.Value;
        var battalionMembership = graph.GetSingleUriNode(person, Vocabulary.PersonHasBattalionMembership);
        var battalionName = battalionMembership is null ?
            null : graph.GetSingleTransitiveLiteral(battalionMembership, Vocabulary.BattalionMembershipHasBattalion, Vocabulary.BattalionName)?.Value;
        var birthAddress = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasBirthAddress, Vocabulary.GeographicalPlaceName)?.Value;
        var dateOfBirth = YmdMapper.GetTextDate(graph, person, Vocabulary.PersonHasDateOfBirth);
        var familyName = graph.GetSingleText(person, Vocabulary.PersonFamilyName);
        var fullName = graph.GetSingleText(person, Vocabulary.PersonFullName);
        var givenName = graph.GetSingleText(person, Vocabulary.PersonGivenName);
        var nationalRegistrationNumber = graph.GetSingleText(person, Vocabulary.NationalRegistrationNumber);
        var nextOfKinRelationship = graph.GetSingleUriNode(person, Vocabulary.PersonHasNextOfKinRelationship);
        var nextOfKinName = nextOfKinRelationship is null ?
            null : graph.GetSingleTransitiveLiteral(nextOfKinRelationship, Vocabulary.NextOfKinRelationshipHasNextOfKin, Vocabulary.PersonFullName)?.Value;
        var kins = nextOfKinRelationship is null ?
            null : graph.GetUriNodes(nextOfKinRelationship, Vocabulary.NextOfKinRelationshipHasKinship)
            .Select(n => n.Uri.Segments.Last());
        var seamanServiceNumber = graph.GetSingleText(person, Vocabulary.SeamanServiceNumber);

        return new()
        {
            Address = address,
            BattalionName = battalionName,
            BirthAddress = birthAddress,
            DateOfBirth = dateOfBirth,
            FamilyName = familyName,
            FullName = fullName,
            GivenName = givenName,
            IsVeteran = veteran is not null ? true : null,
            NationalRegistrationNumber = nationalRegistrationNumber,
            NextOfKinName = nextOfKinName,
            NextOfKinTypes = kins,
            SeamanServiceNumber = seamanServiceNumber
        };
    }
}