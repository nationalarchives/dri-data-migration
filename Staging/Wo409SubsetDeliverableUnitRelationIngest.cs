using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

internal class Wo409SubsetDeliverableUnitRelationIngest(ILogger<Wo409SubsetDeliverableUnitIngest> logger)
{
    private readonly IUriNode rdfsObject = new UriNode(new Uri(RdfSpecsHelper.RdfObject));

    internal void AddRelation(IGraph graph, IGraph existing, IGraph rdf,
        INode subjectId, IUriNode person)
    {
        var relation = rdf.GetSingleBlankNode(subjectId, IngestVocabulary.Relation);
        if (relation is null)
        {
            return;
        }
        var relationPerson = rdf.GetSingleBlankNode(relation, IngestVocabulary.Person);
        if (relationPerson is null)
        {
            return;
        }
        var relationNameSubject = rdf.GetSingleBlankNode(relationPerson, IngestVocabulary.Name) ?? relationPerson;
        var relationName = rdf.GetSingleLiteral(relationNameSubject, IngestVocabulary.NameString);
        if (relationName is null || string.IsNullOrWhiteSpace(relationName.Value))
        {
            return;
        }
        var relationship = existing.GetSingleUriNode(person, Vocabulary.PersonHasNextOfKinRelationship) ?? CacheClient.NewId;
        graph.Assert(person, Vocabulary.PersonHasNextOfKinRelationship, relationship);

        var personNextOfKin = existing.GetSingleUriNode(relationship, Vocabulary.NextOfKinRelationshipHasNextOfKin) ?? CacheClient.NewId;
        graph.Assert(relationship, Vocabulary.NextOfKinRelationshipHasNextOfKin, personNextOfKin);
        GraphAssert.Text(graph, personNextOfKin, relationName.Value, Vocabulary.PersonFullName);

        var relationType = rdf.GetSingleUriNodeSubject(rdfsObject, relation);
        if (relationType is null)
        {
            return;
        }
        var typeValues = GetUriFragment(relationType.Uri)?.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (typeValues is null)
        {
            logger.UnrecognizedKinship(relationType.Uri);
        }
        else
        {
            GraphAssert.Text(graph, relationship, string.Join(' ', typeValues).Trim(), Vocabulary.KinshipVerbatim);
            foreach (var typeValue in typeValues)
            {
                var kinships = GetKinships(typeValue.Replace('-', ' ').Trim());
                if (kinships is null)
                {
                    logger.UnrecognizedKinship(relationType.Uri);
                }
                else
                {
                    foreach (var kinship in kinships)
                    {
                        graph.Assert(relationship, Vocabulary.NextOfKinRelationshipHasKinship, kinship);
                    }
                }
            }
        }
    }

    private static IUriNode[]? GetKinships(string kinshipText) =>
        kinshipText.Replace('-', ' ').Trim() switch
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

    internal static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.TrimStart('#') : null;
}
