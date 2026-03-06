using Api;
using Rdf;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class PersonMapper
{
    internal static RecordOutput.Person? GetFromAsset(IGraph graph)
    {
        var civil = graph.GetSingleUriNode(Vocabulary.AssetHasPerson);
        var veteran = graph.GetSingleUriNode(Vocabulary.AssetHasVeteran);
        var person = civil ?? veteran;
        if (person is null)
        {
            return null;
        }
        return Populate(graph, veteran, person);
    }

    internal static IEnumerable<RecordOutput.Person>? GetFromSubset(IGraph graph)
    {
        var people = graph.GetUriNodes(Vocabulary.SubsetHasPerson);
        var list = new List<RecordOutput.Person>();
        foreach (var person in people)
        {
            list.Add(Populate(graph, null, person));
        }

        if (list.Count > 0)
        {
            return list;
        }
        return null;
    }

    internal static RecordOutput.Person Populate(IGraph graph, IUriNode? veteran, IUriNode person)
    {
        var address = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasContactPoint, Vocabulary.GeographicalPlaceName)?.Value;
        var parish = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasBirthAddress, Vocabulary.Parish)?.Value;
        var town = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasBirthAddress, Vocabulary.Town)?.Value;
        var county = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasBirthAddress, Vocabulary.County)?.Value;
        var country = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasBirthAddress, Vocabulary.Country)?.Value;
        var battalionMembership = graph.GetSingleUriNode(person, Vocabulary.PersonHasBattalionMembership);
        var battalionName = battalionMembership is null ?
            null : graph.GetSingleTransitiveLiteral(battalionMembership, Vocabulary.BattalionMembershipHasBattalion, Vocabulary.BattalionName)?.Value;
        var birthAddress = graph.GetSingleTransitiveLiteral(person, Vocabulary.PersonHasBirthAddress, Vocabulary.GeographicalPlaceName)?.Value;
        var navyMembership = graph.GetSingleUriNode(person, Vocabulary.PersonHasNavyMembership);
        var navyDivisionName = navyMembership is null ?
            null : graph.GetSingleTransitiveLiteral(navyMembership, Vocabulary.NavyMembershipHasNavyDivision, Vocabulary.NavyDivisionName)?.Value;
        var dateOfBirth = YmdMapper.GetTextDate(graph, person, Vocabulary.PersonHasDateOfBirth);
        var givenName = graph.GetSingleText(person, Vocabulary.PersonGivenName);
        var familyName = graph.GetSingleText(person, Vocabulary.PersonFamilyName);
        var fullName = graph.GetSingleText(person, Vocabulary.PersonFullName);
        var alternativeGivenName = graph.GetSingleText(person, Vocabulary.PersonAlternativeGivenName);
        var alternativeFamilyName = graph.GetSingleText(person, Vocabulary.PersonAlternativeFamilyName);
        var personAge = graph.GetSingleLiteral(person, Vocabulary.PersonAge)?.Value.Substring(1).Replace("Y","y ").Replace('M','m').Trim();
        var nationalRegistrationNumber = graph.GetSingleText(person, Vocabulary.NationalRegistrationNumber);
        var nextOfKinRelationship = graph.GetSingleUriNode(person, Vocabulary.PersonHasNextOfKinRelationship);
        var nextOfKinName = nextOfKinRelationship is null ?
            null : graph.GetSingleTransitiveLiteral(nextOfKinRelationship, Vocabulary.NextOfKinRelationshipHasNextOfKin, Vocabulary.PersonFullName)?.Value;
        var kins = nextOfKinRelationship is null ?
            null : graph.GetUriNodes(nextOfKinRelationship, Vocabulary.NextOfKinRelationshipHasKinship)
            .Select(n => n.Uri.LastSegment());
        var seamanServiceNumber = graph.GetSingleText(person, Vocabulary.SeamanServiceNumber);

        return new()
        {
            Address = address,
            Parish = parish,
            Town = town,
            County = county,
            Country = country,
            BattalionName = battalionName,
            BirthAddress = birthAddress,
            NavyDivisionName = navyDivisionName,
            DateOfBirth = dateOfBirth,
            Age = personAge,
            GivenName = givenName,
            FamilyName = familyName,
            AlternativeGivenName = alternativeGivenName,
            AlternativeFamilyName = alternativeFamilyName,
            FullName = fullName,
            IsVeteran = veteran is not null ? true : null,
            NationalRegistrationNumber = nationalRegistrationNumber,
            NextOfKinName = nextOfKinName,
            NextOfKinTypes = kins,
            SeamanServiceNumber = seamanServiceNumber
        };
    }
}