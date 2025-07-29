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
        Func<IMemoryCache, ISparqlClient, Task> ingest, string _)
    {
        sparqlClient.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => d["id"].ToString() == id), CancellationToken.None))
            .ReturnsAsync(existing);
        additionalSetup(sparqlClient, cache);

        await ingest(cache.Object, sparqlClient.Object);

        sparqlClient.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never());
    }

    public static IEnumerable<object[]> NoChangeNoIngestData => [
        [
            accessCondition.Id,
            Build(accessCondition),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client) => await new AccessConditionIngest(cache, client, loggerAc).SetAsync([accessCondition], CancellationToken.None),
            "access condition"
        ],
        [
            groundForRetention.Id,
            Build(groundForRetention),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client) => await new GroundForRetentionIngest(cache, client, loggerGfr).SetAsync([groundForRetention], CancellationToken.None),
            "ground for retention"
        ],
        [
            legislation.Id,
            Build(legislation),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client) => await new LegislationIngest(cache, client, loggerLeg).SetAsync([legislation], CancellationToken.None),
            "legislation"
        ],
        [
            subset.Id,
            Build(subset),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchOrNewSubset(cache, client, parentSubsetRef, parentSubsetNode),
            async (IMemoryCache cache, ISparqlClient client) => await new SubsetIngest(cache, client, loggerSub).SetAsync([subset], CancellationToken.None),
            "subset"
        ],
        [
            asset.Id,
            Build(asset),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchSubset(cache, client, subsetRef, subsetNode),
            async (IMemoryCache cache, ISparqlClient client) => await new AssetIngest(cache, client, loggerAss).SetAsync([asset], CancellationToken.None),
            "asset"
        ],
        [
            variation.Id,
            Build(variation),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchAsset(cache, client, assetRef, assetNode),
            async (IMemoryCache cache, ISparqlClient client) => await new VariationIngest(cache, client, loggerVar).SetAsync([variation], CancellationToken.None),
            "variation"
        ],
        [
            sr.Id,
            Build(sr),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => 
            {
                SetupFetchAccessCondition(client, accessConditionRef.Fragment.Substring(1), accessConditionNode);
                SetupFetchOrNewSensitivityReview(cache, client, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchVariation(cache, client, variationRef.ToString(), variationNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            async (IMemoryCache cache, ISparqlClient client) => await new SensitivityReviewIngest(cache, client, loggerSr).SetAsync([sr], CancellationToken.None),
            "sensitivity review"
        ]
    ];
}
