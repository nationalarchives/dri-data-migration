using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using VDS.RDF;

namespace Rdf.Tests;

[TestClass]
public class NoChangeIngestTest : BaseIngestTest
{
    [TestInitialize]
    public void TestInitialize() => Initialize();

    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("Doesn't ingest when there is no change")]
    [DynamicData(nameof(NoChangeNoIngestData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task NoChangeNoIngest(string id, IGraph existing, Action<Mock<ISparqlClient>, Mock<IMemoryCache>> additionalSetup,
        Func<IMemoryCache, ISparqlClient, ILogger, Task> ingest, string _)
    {
        sparqlClient.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => d["id"].ToString() == id)))
            .ReturnsAsync(existing);
        additionalSetup(sparqlClient, cache);

        await ingest(cache.Object, sparqlClient.Object, logger);

        sparqlClient.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>()), Times.Never());
    }

    public static IEnumerable<object[]> NoChangeNoIngestData => [
        [
            accessCondition.Id,
            Build(accessCondition),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new AccessConditionIngest(cache, client, logger).Set([accessCondition]),
            "access condition"
        ],
        [
            groundForRetention.Id,
            Build(groundForRetention),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new GroundForRetentionIngest(cache, client, logger).Set([groundForRetention]),
            "ground for retention"
        ],
        [
            legislation.Id,
            Build(legislation),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new LegislationIngest(cache, client, logger).Set([legislation]),
            "legislation"
        ],
        [
            subset.Id,
            Build(subset),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchOrNewSubset(cache, parentSubsetRef, parentSubsetNode),
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SubsetIngest(cache, client, logger).Set([subset]),
            "subset"
        ],
        [
            asset.Id,
            Build(asset),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchSubset(cache, client, subsetRef, subsetNode),
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new AssetIngest(cache, client, logger).Set([asset]),
            "asset"
        ],
        [
            variation.Id,
            Build(variation),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchAsset(cache, client, assetRef, assetNode),
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new VariationIngest(cache, client, logger).Set([variation]),
            "variation"
        ],
        [
            sr.Id,
            Build(sr),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => 
            {
                SetupFetchAccessCondition(client, accessConditionRef.Fragment.Substring(1), accessConditionNode);
                SetupFetchOrNewSensitivityReview(cache, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchVariation(cache, client, variationRef.ToString(), variationNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SensitivityReviewIngest(cache, client, logger).Set([sr]),
            "sensitivity review"
        ]
    ];
}
