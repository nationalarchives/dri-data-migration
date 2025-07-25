using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Query;

namespace Rdf.Tests;

[TestClass]
public class DriExportResultSetTest
{
    private Mock<ISparqlClient> sparqlClient;
    internal ILogger<DriExport> logger;

    [TestInitialize]
    public void TestInitialize()
    {
        sparqlClient = new Mock<ISparqlClient>();
        logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DriExport>();
    }

    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("Reads sets")]
    [DynamicData(nameof(ReadsResultSetsData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task ReadsResultSets<T>(SparqlResultSet data,
        Func<DriExport, Task<IEnumerable<T>>> getData,
        T expected, string _) where T : IDriRecord
    {
        sparqlClient.Setup(c => c.GetResultSetAsync(It.IsAny<string>()))
            .ReturnsAsync(data);

        var exporter = new DriExport(logger, sparqlClient.Object);
        var result = await getData(exporter);

        result.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    private static readonly DriSubset broadestSubset = new("Subset 1", "Subset 1");
    private static readonly DriAccessCondition accessCondition = new(new("http://example.com/ac"), "Access condition 1");
    private static readonly DriLegislation legislation = new(new("http://example.com/legislation"), "Legislation section");
    private static readonly DriLegislation missingSectionLegislation = new(new("http://example.com/missing-section-legislation"));
    private static readonly DriGroundForRetention groundForRetention = new("GFR 1", "Ground for retention 1");

    public static IEnumerable<object[]> ReadsResultSetsData => [
        [
            BuildBroadest(broadestSubset),
            async (DriExport exporter) => await exporter.GetBroadestSubsetsAsync(),
            broadestSubset,
            "broadest subset"
        ],
        [
            Build(accessCondition),
            async (DriExport exporter) => await exporter.GetAccessConditionsAsync(),
            accessCondition,
            "access condition"
        ],
        [
            Build(legislation),
            async (DriExport exporter) => await exporter.GetLegislationsAsync(),
            legislation,
            "legislation"
        ],
        [
            Build(missingSectionLegislation),
            async (DriExport exporter) => await exporter.GetLegislationsAsync(),
            missingSectionLegislation,
            "legislation without section"
        ],
        [
            Build(groundForRetention),
            async (DriExport exporter) => await exporter.GetGroundsForRetentionAsync(),
            groundForRetention,
            "ground for retention"
        ]
    ];

    private static SparqlResultSet BuildBroadest(DriSubset dri)
    {
        var sparqlResult = new SparqlResult([
            new("directory", new LiteralNode(dri.Id))
        ]);

        return new SparqlResultSet([sparqlResult]);
    }

    private static SparqlResultSet Build(DriAccessCondition dri)
    {
        var sparqlResult = new SparqlResult([
            new("s", new UriNode(dri.Link)),
            new("label", new LiteralNode(dri.Name))
        ]);

        return new SparqlResultSet([sparqlResult]);
    }

    private static SparqlResultSet Build(DriLegislation dri)
    {
        var bindings = new List<KeyValuePair<string, INode>>()
        {
            new("legislation", new UriNode(dri.Link))
        };
        if (dri.Section is not null)
        {
            bindings.Add(new("label", new LiteralNode(dri.Section)));
        }

        var sparqlResult = new SparqlResult(bindings);

        return new SparqlResultSet([sparqlResult]);
    }

    private static SparqlResultSet Build(DriGroundForRetention dri)
    {
        var sparqlResult = new SparqlResult([
            new("label", new LiteralNode(dri.Code)),
            new("comment", new LiteralNode(dri.Description))
        ]);

        return new SparqlResultSet([sparqlResult]);
    }
}
