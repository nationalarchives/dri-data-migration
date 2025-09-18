using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Dri.Tests;

[TestClass]
public sealed class RdfExporterTest
{
    private Mock<IDriSparqlClient> sparqlClient;
    private RdfExporter exporter;

    [TestInitialize]
    public void TestInitialize()
    {
        sparqlClient = new Mock<IDriSparqlClient>();
        var logger = new FakeLogger<RdfExporter>();
        var options = Microsoft.Extensions.Options.Options.Create<DriSettings>(new());
        exporter = new RdfExporter(logger, options, sparqlClient.Object);
    }


    [TestMethod("Reads access conditions")]
    public async Task FetchesAccessConditions()
    {
        var link = new Uri("http://example.com/ac");
        var name = "Access condition";
        var expected = new DriAccessCondition(link, name);
        var sparqlResult = new SparqlResultSet([
            new SparqlResult([
                new("s", new UriNode(link)),
                new("label", new LiteralNode(name))
                ])
            ]);
        sparqlClient.Setup(s => s.GetResultSetAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(sparqlResult);

        var dris = await exporter.GetAccessConditionsAsync(CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads legislations")]
    public async Task FetchesLegislations()
    {
        var link = new Uri("http://example.com/l");
        var section = "1(2)";
        var expected = new DriLegislation(link, section);
        var sparqlResult = new SparqlResultSet([
            new SparqlResult([
                new("legislation", new UriNode(link)),
                new("label", new LiteralNode(section))
                ])
            ]);
        sparqlClient.Setup(s => s.GetResultSetAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(sparqlResult);

        var dris = await exporter.GetLegislationsAsync(CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads grounds for retention")]
    public async Task FetchesGroundsForRetentionAsync()
    {
        var code = "X";
        var description = "Ground for retention X";
        var expected = new DriGroundForRetention(code, description);
        var sparqlResult = new SparqlResultSet([
            new SparqlResult([
                new("label", new LiteralNode(code)),
                new("comment", new LiteralNode(description))
                ])
            ]);
        sparqlClient.Setup(s => s.GetResultSetAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(sparqlResult);

        var dris = await exporter.GetGroundsForRetentionAsync(CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads subsets")]
    public async Task FetchesSubsets()
    {
        var reference = "Subset";
        var directory = "Location/";
        var parentReference = "Parent";
        var expected = new DriSubset(reference, directory, parentReference);
        var graph = new Graph();
        var subject = graph.CreateUriNode(new Uri("http://example.com/subset"));
        graph.Assert(subject, Vocabulary.SubsetReference, graph.CreateLiteralNode(reference));
        var retention = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.SubsetHasRetention, retention);
        graph.Assert(retention, Vocabulary.ImportLocation, graph.CreateLiteralNode(directory));
        var broader = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.SubsetHasBroaderSubset, broader);
        graph.Assert(broader, Vocabulary.SubsetReference, graph.CreateLiteralNode(parentReference));

        sparqlClient.Setup(s => s.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(graph);

        var dris = await exporter.GetSubsetsByCodeAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads assets")]
    public async Task FetchesAssets()
    {
        var link = new Uri("http://example.com/asset");
        var reference = "Asset";
        var directory = "Location/";
        var subsetReference = "Subset";
        var expected = new DriAsset(link, reference, directory, subsetReference);
        var graph = new Graph();
        var subject = graph.CreateUriNode(link);
        graph.Assert(subject, Vocabulary.AssetDriId, subject);
        graph.Assert(subject, Vocabulary.AssetReference, graph.CreateLiteralNode(reference));
        var subset = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.AssetHasSubset, subset);
        graph.Assert(subset, Vocabulary.SubsetReference, graph.CreateLiteralNode(subsetReference));
        var retention = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.AssetHasRetention, retention);
        graph.Assert(retention, Vocabulary.ImportLocation, graph.CreateLiteralNode(directory));

        sparqlClient.Setup(s => s.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(graph);

        var dris = await exporter.GetAssetsByCodeAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads variations")]
    public async Task FetchesVariations()
    {
        var link = new Uri("http://example.com/variation");
        var name = "Variation";
        var assetReference = "Asset";
        var expected = new DriVariation(link, name, assetReference);
        var graph = new Graph();
        var subject = graph.CreateUriNode(link);
        graph.Assert(subject, Vocabulary.VariationDriId, subject);
        var asset = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.VariationHasAsset, asset);
        graph.Assert(asset, Vocabulary.AssetReference, graph.CreateLiteralNode(assetReference));
        graph.Assert(subject, Vocabulary.VariationName, graph.CreateLiteralNode(name));

        sparqlClient.Setup(s => s.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(graph);

        var dris = await exporter.GetVariationsByCodeAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod("Reads sensitivity reviews")]
    public async Task FetchesSensitivityReviews()
    {
        var link = new Uri("http://example.com/sr");
        var targetReference = "Variation";
        var targetLink = new Uri("http://example.com/variation");
        var targetType = new Uri("http://example.com/File");
        var acLink = new Uri("http://example.com/ac");
        var legislationLink = new Uri("http://example.com/l");
        var reviewDate = DateTimeOffset.UtcNow.AddDays(-1);
        var previousSrLink = new Uri("http://example.com/previous-sr");
        var sensitiveName = "Sensitive name";
        var sensitiveDescription = "Sensitive description";
        var date = DateTimeOffset.UtcNow;
        var restrictionStartDate = DateTimeOffset.UtcNow.AddDays(-2);
        var restrictionDuration = 1;
        var restrictionDescription = "Restriction description";
        var instrumentNumber = 2;
        var instrumentSignedDate = DateTimeOffset.UtcNow.AddDays(-3);
        var restrictionReviewDate = DateTimeOffset.UtcNow.AddDays(-4);
        var groundForRetention = new Uri("http://example.com/gfr");
        var expected = new DriSensitivityReview(link, targetReference, targetLink,
            targetType, acLink, [legislationLink], reviewDate, previousSrLink,
            sensitiveName, sensitiveDescription, date, restrictionStartDate,
            restrictionDuration, restrictionDescription, instrumentNumber,
            instrumentSignedDate, restrictionReviewDate, groundForRetention);
        var graph = new Graph();
        var subject = graph.CreateUriNode(link);
        graph.Assert(subject, Vocabulary.SensitivityReviewDriId, subject);
        graph.Assert(subject, new UriNode(new Uri(Vocabulary.Namespace, "x-reference")), graph.CreateLiteralNode(targetReference));
        graph.Assert(subject, new UriNode(new Uri(Vocabulary.Namespace, "x-id")), graph.CreateUriNode(targetLink));
        graph.Assert(subject, new UriNode(new Uri(Vocabulary.Namespace, "x-type")), graph.CreateUriNode(targetType));
        graph.Assert(subject, Vocabulary.SensitivityReviewDate, new DateTimeNode(date));
        graph.Assert(subject, Vocabulary.SensitivityReviewSensitiveName, graph.CreateLiteralNode(sensitiveName));
        graph.Assert(subject, Vocabulary.SensitivityReviewSensitiveDescription, graph.CreateLiteralNode(sensitiveDescription));
        graph.Assert(subject, Vocabulary.SensitivityReviewHasPastSensitivityReview, graph.CreateUriNode(previousSrLink));
        var restriction = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateTimeNode(reviewDate));
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateTimeNode(restrictionStartDate));
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LongNode(restrictionDuration));
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDescription, graph.CreateLiteralNode(restrictionDescription));
        var retentionRestriction = graph.CreateBlankNode();
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
        var legislation = graph.CreateBlankNode();
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislation);
        graph.Assert(legislation, Vocabulary.LegislationHasUkLegislation, graph.CreateLiteralNode(legislationLink.ToString()));
        graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(instrumentNumber));
        graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate, new DateTimeNode(instrumentSignedDate));
        graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateTimeNode(restrictionReviewDate));
        var gfr = graph.CreateBlankNode();
        graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, gfr);
        graph.Assert(gfr, Vocabulary.GroundForRetentionCode, graph.CreateUriNode(groundForRetention));
        var ac = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.SensitivityReviewHasAccessCondition, ac);
        graph.Assert(ac, Vocabulary.AccessConditionCode, graph.CreateUriNode(acLink));
        
        sparqlClient.Setup(s => s.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(graph);

        var dris = await exporter.GetSensitivityReviewsByCodeAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }
}
