using Api;
using Microsoft.Extensions.Logging;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class Wo409SubsetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient,
    ILogger<Wo409SubsetDeliverableUnitIngest> logger) :
    StagingIngest<DriWo409SubsetDeliverableUnit>(sparqlClient, logger, "Wo409SubsetDeliverableUnitGraph")
{
    private readonly RdfXmlLoader rdfXmlLoader = new(logger);
    private readonly IUriNode wo409 = new UriNode(new Uri("http://example.com/subject"));
    private readonly Uri givenName = new("http://example.com/given");
    private readonly Uri familyName = new("http://example.com/familyName");
    private readonly IUriNode rdfsObject = new UriNode(new Uri(RdfSpecsHelper.RdfObject));

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
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.Wo409SubsetDriXml);
            await ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
        }

        return graph;
    }

    private async Task ExtractXmlData(IGraph graph, IGraph existing, INode id,
        string xml, CancellationToken cancellationToken)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var rdf = rdfXmlLoader.GetRdf(doc);
        if (rdf is null)
        {
            logger.AssetXmlMissingRdf(id.AsValuedNode().AsString());
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
        AddRelation(graph, existing, rdf, subjectTriple.Object, person);
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
            var birthDate = rdf.GetTriplesWithSubjectPredicate(birth, IngestVocabulary.Date)
                .SingleOrDefault()?.Object as ILiteralNode;
            if (birthDate is not null && !string.IsNullOrWhiteSpace(birthDate.Value) &&
                DateParser.TryParseDate(birthDate.Value, out var birthDt))
            {
                var dob = existing.GetTriplesWithSubjectPredicate(person, Vocabulary.PersonHasDateOfBirth).SingleOrDefault()?.Object as IUriNode
                        ?? CacheClient.NewId;
                graph.Assert(person, Vocabulary.PersonHasDateOfBirth, dob);
                GraphAssert.YearMonthDay(graph, dob, birthDt!.Year, birthDt!.Month, birthDt!.Day);
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

    private void AddRelation(IGraph graph, IGraph existing, IGraph rdf,
        INode subjectId, IUriNode person)
    {
        var relation = rdf.GetTriplesWithSubjectPredicate(subjectId, IngestVocabulary.Relation)
            .SingleOrDefault()?.Object as IBlankNode;
        if (relation is not null)
        {
            var relationPerson = rdf.GetTriplesWithSubjectPredicate(relation, IngestVocabulary.Person)
                .SingleOrDefault()?.Object as IBlankNode;
            if (relationPerson is not null)
            {
                var relationNameSubject = rdf.GetTriplesWithSubjectPredicate(relationPerson, IngestVocabulary.Name)
                    .SingleOrDefault()?.Object as IBlankNode ?? relationPerson;
                var relationName = rdf.GetTriplesWithSubjectPredicate(relationNameSubject, IngestVocabulary.NameString)
                    .SingleOrDefault()?.Object as ILiteralNode;
                if (relationName is not null && !string.IsNullOrWhiteSpace(relationName.Value))
                {
                    var relationship = existing.GetTriplesWithSubjectPredicate(person, Vocabulary.PersonHasNextOfKinRelationship)
                        .SingleOrDefault()?.Object ?? CacheClient.NewId;
                    graph.Assert(person, Vocabulary.PersonHasNextOfKinRelationship, relationship);

                    var personNextOfKin = existing.GetTriplesWithSubjectPredicate(relationship, Vocabulary.NextOfKinRelationshipHasNextOfKin)
                        .SingleOrDefault()?.Object ?? CacheClient.NewId;
                    graph.Assert(relationship, Vocabulary.NextOfKinRelationshipHasNextOfKin, personNextOfKin);
                    GraphAssert.Text(graph, personNextOfKin, relationName.Value, Vocabulary.PersonFullName);

                    var relationType = rdf.GetTriplesWithPredicateObject(rdfsObject, relation)
                        .SingleOrDefault()?.Subject as IUriNode;
                    if (relationType is not null)
                    {
                        var typeValues = GetUriFragment(relationType.Uri)?.Split('_', StringSplitOptions.RemoveEmptyEntries);
                        if (typeValues is null)
                        {
                            logger.UnrecognizedKinship(relationType.Uri);
                        }
                        else
                        {
                            List<IUriNode> kinships = [];
                            foreach (var typeValue in typeValues)
                            {
                                IUriNode[]? kinship = typeValue.Replace('-', ' ').Trim() switch
                                {
                                    "Wife" or "wife" or "w" => [Vocabulary.Wife],
                                    "Husband" => [Vocabulary.Husband],
                                    "Mother" => [Vocabulary.Mother],
                                    "Father" or "father" or "Parents  Father" => [Vocabulary.Father],
                                    "Sister" or "sister" or "Eldest Sister" => [Vocabulary.Sister],
                                    "Brother" or "Brothers" => [Vocabulary.Brother],
                                    "Brother OR Sister" => [Vocabulary.Brother, Vocabulary.Sister],
                                    "Uncle" or "Uncle  Guardian" => [Vocabulary.Uncle],
                                    "Son" or "Sons" or "Eldest Son" or "Eldest son" => [Vocabulary.Son],
                                    "Adopted Mother" => [Vocabulary.AdoptedMother],
                                    "Adopted Daughter" => [Vocabulary.AdoptedDaughter],
                                    "Adopted Son" => [Vocabulary.AdoptedSon],
                                    "Daughter" => [Vocabulary.Daughter],
                                    "Three Children" or "Eldest Child" => [Vocabulary.Daughter, Vocabulary.Son],
                                    "Parents" or "Parent" => [Vocabulary.Mother, Vocabulary.Father],
                                    "Grandparents" or "Grandparent" => [Vocabulary.Grandmother, Vocabulary.Grandfather],
                                    "Grandmother" => [Vocabulary.Grandmother],
                                    "Grandfather" => [Vocabulary.Grandfather],
                                    "Grandson" => [Vocabulary.Grandson],
                                    "Aunt" or "Mother s Sister" => [Vocabulary.Aunt],
                                    "Stepmother" => [Vocabulary.Stepmother],
                                    "Stepfather" => [Vocabulary.Stepfather],
                                    "Step Uncle" => [Vocabulary.Stepuncle],
                                    "Stepsister" or "Step sister" => [Vocabulary.Stepsister],
                                    "Stepbrother" or "Step Brother" => [Vocabulary.Stepbrother],
                                    "Half brother" or "Half Brother" => [Vocabulary.HalfBrother],
                                    "Foster Mother" => [Vocabulary.FosterMother],
                                    "Foster Father" => [Vocabulary.FosterFather],
                                    "Foster Parent" or "Foster Parents" => [Vocabulary.FosterMother, Vocabulary.FosterFather],
                                    "Stepson" => [Vocabulary.Stepson],
                                    "Niece" => [Vocabulary.Niece],
                                    "Sister in Law" or "Sister in law" => [Vocabulary.SisterInLaw],
                                    "Brother in Law" => [Vocabulary.BrotherInLaw],
                                    "Mother in Law" => [Vocabulary.MotherInLaw],
                                    "Father in Law" => [Vocabulary.FatherInLaw],
                                    "Son in Law" => [Vocabulary.SonInLaw],
                                    "Not Related" => [Vocabulary.NotRelated],
                                    "Friend" => [Vocabulary.Friend],
                                    "Nephew" => [Vocabulary.Nephew],
                                    "Cousin" => [Vocabulary.Cousin],
                                    "Fiance" => [Vocabulary.Fiance],
                                    "Fiancee" => [Vocabulary.Fiancee],
                                    "Landlady" => [Vocabulary.Landlady],
                                    "Housekeeper" => [Vocabulary.Housekeeper],
                                    "Guardian" => [Vocabulary.Guardian],
                                    "Undefined" => [Vocabulary.UndefinedKinship],
                                    _ => null
                                };
                                if (kinship is null)
                                {
                                    logger.UnrecognizedKinship(relationType.Uri);
                                }
                                else
                                {
                                    kinships.AddRange(kinship);
                                }

                            }
                            foreach (var kinship in kinships)
                            {
                                graph.Assert(relationship, Vocabulary.NextOfKinRelationshipHasKinship, kinship);
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task<IUriNode?> GetAddressAsync(IGraph rdf, INode subjectid, CancellationToken cancellationToken)
    {
        var address = rdf.GetTriplesWithSubjectPredicate(subjectid, IngestVocabulary.Address)
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
