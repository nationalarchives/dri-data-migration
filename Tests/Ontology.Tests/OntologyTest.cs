using FluentAssertions;
using FluentAssertions.Execution;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;

namespace Ontology.Tests;

[TestClass]
public sealed class OntologyTest
{
    private readonly string fileLocation = $"{Directory.GetCurrentDirectory()}/../../../../../Ontology.ttl";

    [TestMethod(DisplayName = "Valid Turtle syntax")]
    public void ValidTurtle() =>
        FluentActions.Invoking(() =>
            FileLoader.Load(new Graph(), fileLocation, new TurtleParser()))
        .Should().NotThrow("Ontology has a valid Turtle syntax");

    [TestMethod(DisplayName = "Types are defined by schema")]
    [DataRow(OntologyHelper.OwlClass, "classes")]
    [DataRow(OntologyHelper.OwlObjectProperty, "object properties")]
    [DataRow(OntologyHelper.OwlDatatypeProperty, "data type properties")]
    public void IsDefinedBy(string nodeUri, string typeName)
    {
        var rdfType = new UriNode(new Uri(RdfSpecsHelper.RdfType));
        var owlNode = new UriNode(new Uri(nodeUri));
        var isDefinedBy = new UriNode(new Uri(OntologyHelper.PropertyIsDefinedBy));
        var schema = new UriNode(new Uri("http://id.example.com/schema"));
        var graph = new Graph();
        graph.LoadFromFile(fileLocation, new TurtleParser());

        var classes = graph.GetTriplesWithPredicateObject(rdfType, owlNode)
            .Select(t => t.Subject as IUriNode);
        var classesDefinedBy = classes.Where(c => graph.ContainsTriple(new Triple(c, isDefinedBy, schema)));

        classes.Should().NotBeEmpty($"Ontology contains {typeName}")
            .And.HaveCount(classesDefinedBy.Count(), $"all {typeName} should be defined by {schema}");
    }

    [TestMethod(DisplayName = "Object properties domains and ranges are Ontology classes")]
    public void ObjectPropertiesDomainRange()
    {
        var rdfType = new UriNode(new Uri(RdfSpecsHelper.RdfType));
        var owlClass = new UriNode(new Uri(OntologyHelper.OwlClass));
        var owlObjectProperty = new UriNode(new Uri(OntologyHelper.OwlObjectProperty));
        var domain = new UriNode(new Uri(OntologyHelper.PropertyDomain));
        var range = new UriNode(new Uri(OntologyHelper.PropertyRange));
        var graph = new Graph();
        graph.LoadFromFile(fileLocation, new TurtleParser());

        var classes = graph.GetTriplesWithPredicateObject(rdfType, owlClass)
            .Select(t => t.Subject as IUriNode);
        var objectProperties = graph.GetTriplesWithPredicateObject(rdfType, owlObjectProperty)
            .Select(t => t.Subject as IUriNode);
        var domains = objectProperties.SelectMany(o => graph.GetTriplesWithSubjectPredicate(o, domain))
            .Select(t => t.Object as IUriNode)
            .Distinct();
        var ranges = objectProperties.SelectMany(o => graph.GetTriplesWithSubjectPredicate(o, range))
            .Select(t => t.Object as IUriNode)
            .Distinct();

        using (new AssertionScope())
        {
            domains.Should().NotBeEmpty("object properties have domains")
                .And.BeSubsetOf(classes, "all object properties have domain object of Ontology classes");
            ranges.Should().NotBeEmpty("object properties have ranges")
                .And.BeSubsetOf(classes, "all object properties have range object of Ontology classes");
        }
    }

    [TestMethod(DisplayName = "Data type properties domains are Ontology classes")]
    public void DataTypePropertiesDomainRange()
    {
        var rdfType = new UriNode(new Uri(RdfSpecsHelper.RdfType));
        var owlClass = new UriNode(new Uri(OntologyHelper.OwlClass));
        var owlDatatypeProperty = new UriNode(new Uri(OntologyHelper.OwlDatatypeProperty));
        var domain = new UriNode(new Uri(OntologyHelper.PropertyDomain));
        var graph = new Graph();
        graph.LoadFromFile(fileLocation, new TurtleParser());

        var classes = graph.GetTriplesWithPredicateObject(rdfType, owlClass)
            .Select(t => t.Subject as IUriNode);
        var dataTypeProperties = graph.GetTriplesWithPredicateObject(rdfType, owlDatatypeProperty)
            .Select(t => t.Subject as IUriNode);
        var domains = dataTypeProperties.SelectMany(o => graph.GetTriplesWithSubjectPredicate(o, domain))
            .Select(t => t.Object as IUriNode)
            .Distinct();

        domains.Should().NotBeEmpty("data type properties have domains")
            .And.BeSubsetOf(classes, "all data type properties have domain object of Ontology classes");
    }
}
