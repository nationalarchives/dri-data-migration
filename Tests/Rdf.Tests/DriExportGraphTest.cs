using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Rdf.Tests;

[TestClass]
public class DriExportGraphTest
{
    private Mock<IDriSparqlClient> sparqlClient;
    internal ILogger<DriExporter> logger;

    [TestInitialize]
    public void TestInitialize()
    {
        sparqlClient = new Mock<IDriSparqlClient>();
        logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DriExporter>();
    }

    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("Reads data")]
    [DynamicData(nameof(ReadsGraphData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task ReadsGraph<T>(IGraph data, Func<DriExporter, Task<IEnumerable<T>>> getData,
        T expected, string _) where T : IDriRecord
    {
        sparqlClient.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(data);

        var exporter = new DriExporter(logger, sparqlClient.Object);
        var result = await getData(exporter);

        result.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    private static readonly DriSubset subset = new("Subset 1", "Directory subset 1", "Parent subset 1");
    private static readonly DriSubset noParentSubset = new("Subset 1", "Directory subset 1");
    private static readonly DriAsset asset = new("Asset 1", "Directory asset 1", "Subset 1");
    private static readonly DriVariation variation = new(new("http://example.com/variation"), "Variation name 1", "Asset 1");
    private static readonly DriSensitivityReview minimalSr = new(new("http://example.com/sr"), "Asset 1", new("http://example.com/asset1"), new("http://example.com/target-type"), new("http://example.com/ac"), []);
    private static readonly DriSensitivityReview multipleLegislationsSr = new(new("http://example.com/sr"), "Asset 1", new("http://example.com/file1"), new("http://example.com/target-type"), new("http://example.com/ac"), [new("http://example.com/l1"), new("http://example.com/l2")]);
    private static readonly DriSensitivityReview allFieldsSr = new(new("http://example.com/sr"), "Asset 1", new("http://example.com/file1"), new("http://example.com/target-type"), new("http://example.com/ac"), [new("http://example.com/l1")],
        DateTimeOffset.UtcNow, new("http://example.com/past-sr"), "Sensitive name", "Sensitive description", DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow, 1, "Restriction description", 2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new("http://example.com/ground-for-retention1"));

    public static IEnumerable<object[]> ReadsGraphData => [
        [
            Build(subset),
            async (DriExporter exporter) => await exporter.GetSubsetsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            subset,
            "subset"
        ],
        [
            Build(noParentSubset),
            async (DriExporter exporter) => await exporter.GetSubsetsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            noParentSubset,
            "subset without parent"
        ],
        [
            Build(asset),
            async (DriExporter exporter) => await exporter.GetAssetsByCodeAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            asset,
            "asset"
        ],
        [
            Build(variation),
            async (DriExporter exporter) => await exporter.GetVariationsByCodeAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            variation,
            "variation"
        ],
        [
            Build(variation),
            async (DriExporter exporter) => await exporter.GetVariationsByCodeAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            variation,
            "variation"
        ],
        [
            Build(minimalSr),
            async (DriExporter exporter) => await exporter.GetSensitivityReviewsByCodeAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            minimalSr,
            "minimal sensitivity review"
        ],
        [
            Build(multipleLegislationsSr),
            async (DriExporter exporter) => await exporter.GetSensitivityReviewsByCodeAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            multipleLegislationsSr,
            "sensitivity review with multiple legislations"
        ],
        [
            Build(allFieldsSr),
            async (DriExporter exporter) => await exporter.GetSensitivityReviewsByCodeAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None),
            allFieldsSr,
            "sensitivity review with all fields"
        ]
    ];

    private static IGraph Build(DriSubset dri)
    {
        var graph = new Graph();
        var subject = new UriNode(new Uri("http://example.com/s"));
        var retention = new BlankNode("r");
        graph.Assert(subject, Vocabulary.SubsetReference, new LiteralNode(dri.Reference));
        graph.Assert(subject, Vocabulary.SubsetHasRetention, retention);
        graph.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));

        if (dri.ParentReference is not null)
        {
            var broader = new BlankNode("b");
            graph.Assert(subject, Vocabulary.SubsetHasBroaderSubset, broader);
            graph.Assert(broader, Vocabulary.SubsetReference, new LiteralNode(dri.ParentReference));
        }

        return graph;
    }

    private static IGraph Build(DriAsset dri)
    {
        var graph = new Graph();
        var subject = new BlankNode("a");
        var subsetNode = new BlankNode("s");
        var retention = new BlankNode("r");

        graph.Assert(subject, Vocabulary.AssetReference, new LiteralNode(dri.Reference));
        graph.Assert(subject, Vocabulary.AssetHasSubset, subsetNode);
        graph.Assert(subsetNode, Vocabulary.SubsetReference, new LiteralNode(dri.SubsetReference));
        graph.Assert(subject, Vocabulary.AssetHasRetention, retention);
        graph.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));


        return graph;
    }

    private static IGraph Build(DriVariation dri)
    {
        var graph = new Graph();
        var subject = new UriNode(new Uri("http://example.com/s"));
        var assetNode = new BlankNode("a");

        graph.Assert(subject, Vocabulary.VariationDriId, new UriNode(dri.Link));
        graph.Assert(subject, Vocabulary.VariationName, new LiteralNode(dri.VariationName));
        graph.Assert(subject, Vocabulary.VariationHasAsset, assetNode);
        graph.Assert(assetNode, Vocabulary.AssetReference, new LiteralNode(dri.AssetReference));
        graph.Assert(subject, Vocabulary.ImportLocation, new LiteralNode(dri.VariationName));

        return graph;
    }

    private static IGraph Build(DriSensitivityReview dri)
    {
        var graph = new Graph();
        var tempReferencePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-reference"));
        var tempIdPredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-id"));
        var tempTypePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-type"));
        var subject = new UriNode(new Uri("http://example.com/s"));
        var restriction = new BlankNode("r");
        var retentionRestriction = new BlankNode("rr");
        var ground = new BlankNode("g");
        var ac = new BlankNode("ac");
        var legislation = new BlankNode("l");

        graph.Assert(subject, Vocabulary.SensitivityReviewDriId, new UriNode(dri.Link));
        graph.Assert(subject, tempReferencePredicate, new LiteralNode(dri.TargetReference));
        graph.Assert(subject, tempTypePredicate, new UriNode(dri.TargetType));
        graph.Assert(subject, tempIdPredicate, new UriNode(dri.TargetId));
        if (dri.Date is not null)
        {
            graph.Assert(subject, Vocabulary.SensitivityReviewDate, new DateNode(dri.Date.Value));
        }
        if (dri.SensitiveName is not null)
        {
            graph.Assert(subject, Vocabulary.SensitivityReviewSensitiveName, new LiteralNode(dri.SensitiveName));
        }
        if (dri.SensitiveDescription is not null)
        {
            graph.Assert(subject, Vocabulary.SensitivityReviewSensitiveDescription, new LiteralNode(dri.SensitiveDescription));
        }
        if (dri.PreviousId is not null)
        {
            graph.Assert(subject, Vocabulary.SensitivityReviewHasPastSensitivityReview, new UriNode(dri.PreviousId));
        }

        graph.Assert(subject, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);

        if (dri.ReviewDate is not null)
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateNode(dri.ReviewDate.Value));
        }
        if (dri.RestrictionStartDate is not null)
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateNode(dri.RestrictionStartDate.Value));
        }
        if (dri.RestrictionDuration is not null)
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LongNode(dri.RestrictionDuration.Value));
        }
        if (dri.RestrictionDescription is not null)
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDescription, new LiteralNode(dri.RestrictionDescription));
        }
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
        if (dri.InstrumentNumber is not null)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber.Value));
        }
        if (dri.InstrumentSignedDate is not null)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate, new DateNode(dri.InstrumentSignedDate.Value));
        }
        if (dri.RestrictionReviewDate is not null)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateNode(dri.RestrictionReviewDate.Value));
        }
        graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, ground);
        if (dri.GroundForRetention is not null)
        {
            graph.Assert(ground, Vocabulary.GroundForRetentionCode, new UriNode(dri.GroundForRetention));
        }
        graph.Assert(subject, Vocabulary.SensitivityReviewHasAccessCondition, ac);
        if (dri.AccessCondition is not null)
        {
            graph.Assert(ac, Vocabulary.AccessConditionCode, new UriNode(dri.AccessCondition));
        }
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislation);
        foreach (var item in dri.Legislations)
        {
            graph.Assert(legislation, Vocabulary.LegislationHasUkLegislation, new UriNode(item));
        }

        return graph;
    }
}
