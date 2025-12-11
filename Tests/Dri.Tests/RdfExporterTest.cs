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
#pragma warning disable CS8618
    private Mock<IDriSparqlClient> sparqlClient;
    private RdfExporter exporter;
#pragma warning restore CS8618

    [TestInitialize]
    public void TestInitialize()
    {
        sparqlClient = new Mock<IDriSparqlClient>();
        var logger = new FakeLogger<RdfExporter>();
        var options = Microsoft.Extensions.Options.Options.Create<DriSettings>(new());
        exporter = new RdfExporter(logger, options, sparqlClient.Object);
    }

    [TestMethod(DisplayName = "Reads access conditions")]
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

    [TestMethod(DisplayName = "Reads legislations")]
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

    [TestMethod(DisplayName = "Reads grounds for retention")]
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

    [TestMethod(DisplayName = "Reads subsets")]
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

        var dris = await exporter.GetSubsetsAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod(DisplayName = "Reads assets")]
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

        var dris = await exporter.GetAssetsAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod(DisplayName = "Reads variations")]
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

        var dris = await exporter.GetVariationsAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod(DisplayName = "Reads sensitivity reviews (no files)")]
    public async Task FetchesSensitivityReviewsNoFile()
    {
        var link = new Uri("http://example.com/sr");
        var targetReference = "Variation";
        var targetLink = new Uri("http://example.com/variation");
        var targetType = new Uri("http://example.com/#DeliverableUnit");
        var previousSrLink = new Uri("http://example.com/previous-sr");
        var sensitiveName = "Sensitive name";
        var sensitiveDescription = "Sensitive description";
        var date = DateTimeOffset.UtcNow;
        var changeDriId = new Uri("http://example.com/change");
        var changeDescription = "Change description";
        var changeDateTime = DateTimeOffset.UtcNow.AddDays(-5);
        var changeOperatorLink = new Uri("http://example.com/operator");
        var changeOperatorName = "Operator name";
        var expected = new DriSensitivityReview(link, targetReference, targetLink,
            targetType, null, [], null, previousSrLink, sensitiveName,
            sensitiveDescription, date, null, null, null, null, null, null, null,
            changeDriId, changeDescription, changeDateTime, changeOperatorLink, changeOperatorName);
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
        graph.Assert(subject, Vocabulary.ChangeDriId, new UriNode(changeDriId));
        graph.Assert(subject, Vocabulary.ChangeDescription, new LiteralNode(changeDescription));
        graph.Assert(subject, Vocabulary.ChangeDateTime, new DateTimeNode(changeDateTime));
        var operatorId = graph.CreateUriNode(changeOperatorLink);
        graph.Assert(subject, Vocabulary.ChangeHasOperator, operatorId);
        graph.Assert(operatorId, Vocabulary.OperatorIdentifier, new UriNode(changeOperatorLink));
        graph.Assert(operatorId, Vocabulary.OperatorName, new LiteralNode(changeOperatorName));

        sparqlClient.Setup(s => s.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(graph);

        var dris = await exporter.GetSensitivityReviewsAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    [TestMethod(DisplayName = "Reads sensitivity reviews")]
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
        var changeDriId = new Uri("http://example.com/change");
        var changeDescription = "Change description";
        var changeDateTime = DateTimeOffset.UtcNow.AddDays(-5);
        var changeOperatorLink = new Uri("http://example.com/operator");
        var changeOperatorName = "Operator name";
        var expected = new DriSensitivityReview(link, targetReference, targetLink,
            targetType, acLink, [legislationLink], reviewDate, previousSrLink,
            sensitiveName, sensitiveDescription, date, restrictionStartDate,
            restrictionDuration, restrictionDescription, instrumentNumber,
            instrumentSignedDate, restrictionReviewDate, groundForRetention,
            changeDriId, changeDescription, changeDateTime, changeOperatorLink, changeOperatorName);
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
        graph.Assert(subject, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateTimeNode(reviewDate));
        graph.Assert(subject, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateTimeNode(restrictionStartDate));
        graph.Assert(subject, Vocabulary.SensitivityReviewRestrictionDuration, new LongNode(restrictionDuration));
        graph.Assert(subject, Vocabulary.SensitivityReviewRestrictionDescription, graph.CreateLiteralNode(restrictionDescription));
        var legislation = graph.CreateBlankNode();
        graph.Assert(subject, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislation);
        graph.Assert(legislation, Vocabulary.LegislationHasUkLegislation, graph.CreateUriNode(legislationLink));
        graph.Assert(subject, Vocabulary.RetentionInstrumentNumber, new LongNode(instrumentNumber));
        graph.Assert(subject, Vocabulary.RetentionInstrumentSignatureDate, new DateTimeNode(instrumentSignedDate));
        graph.Assert(subject, Vocabulary.RetentionRestrictionReviewDate, new DateTimeNode(restrictionReviewDate));
        graph.Assert(subject, Vocabulary.GroundForRetentionCode, graph.CreateUriNode(groundForRetention));
        graph.Assert(subject, Vocabulary.AccessConditionCode, graph.CreateUriNode(acLink));
        graph.Assert(subject, Vocabulary.ChangeDriId, new UriNode(changeDriId));
        graph.Assert(subject, Vocabulary.ChangeDescription, new LiteralNode(changeDescription));
        graph.Assert(subject, Vocabulary.ChangeDateTime, new DateTimeNode(changeDateTime));
        var operatorId = graph.CreateUriNode(changeOperatorLink);
        graph.Assert(subject, Vocabulary.ChangeHasOperator, operatorId);
        graph.Assert(operatorId, Vocabulary.OperatorIdentifier, new UriNode(changeOperatorLink));
        graph.Assert(operatorId, Vocabulary.OperatorName, new LiteralNode(changeOperatorName));

        sparqlClient.Setup(s => s.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(graph);

        var dris = await exporter.GetSensitivityReviewsAsync(0, CancellationToken.None);

        dris.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }
}
