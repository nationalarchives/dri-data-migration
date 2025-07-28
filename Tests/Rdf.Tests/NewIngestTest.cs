using Api;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Reflection;
using VDS.RDF;

namespace Rdf.Tests;

[TestClass]
public class NewIngestTest : BaseIngestTest
{
    [TestInitialize]
    public void TestInitialize() => Initialize();

    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("Performs update when there is a new record")]
    [DynamicData(nameof(ChangeIngestData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task ChangeIngest(Func<IMemoryCache, ISparqlClient, Task> ingest,
         Action<Mock<ISparqlClient>, Mock<IMemoryCache>> additionalSetup, int addedCount, string _)
    {
        sparqlClient.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());
        additionalSetup(sparqlClient, cache);

        await ingest(cache.Object, sparqlClient.Object);

        sparqlClient.Verify(c => c.ApplyDiffAsync(It.Is<GraphDiffReport>(r =>
            r.AddedTriples.Count() == addedCount && !r.RemovedTriples.Any()), CancellationToken.None));
    }

    public static IEnumerable<object[]> ChangeIngestData => [
        [
            async (IMemoryCache cache, ISparqlClient client) => await new AccessConditionIngest(cache, client, loggerAc)
                .SetAsync([accessCondition], CancellationToken.None),
            NoopSetup(),
            2,
            "access condition"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new GroundForRetentionIngest(cache, client, loggerGfr)
                .SetAsync([groundForRetention], CancellationToken.None),
            NoopSetup(),
            2,
            "ground for retention"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new LegislationIngest(cache, client, loggerLeg)
                .SetAsync([legislation], CancellationToken.None),
            NoopSetup(),
            2,
            "legislation"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new SubsetIngest(cache, client, loggerSub)
                .SetAsync([subset], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchOrNewSubset(cache, parentSubsetRef, parentSubsetNode),
            5,
            "subset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new SubsetIngest(cache, client, loggerSub)
                .SetAsync([subset], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) =>
            {
                var notFound = (object?)null;
                cache.Setup(c => c.TryGetValue(It.Is<object>(m => m.Equals($"subset-parentSubsetRef")), out notFound)).Returns(false);
                SetupFetchSubset(cache, client, parentSubsetRef, (object?)(IUriNode)null);
            },
            5,
            "subset with not present parent subset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new AssetIngest(cache, client, loggerAss)
                .SetAsync([asset], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchSubset(cache, client, subsetRef, subsetNode),
            4,
            "asset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new VariationIngest(cache, client, loggerVar)
                .SetAsync([variation], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchAsset(cache, client, assetRef, assetNode),
            3,
            "variation"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new SensitivityReviewIngest(cache, client, loggerSr)
                .SetAsync([sr], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) =>
            {
                SetupFetchAccessCondition(client, accessConditionRef.Fragment.Substring(1), accessConditionNode);
                SetupFetchOrNewSensitivityReview(cache, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchVariation(cache, client, variationRef.ToString(), variationNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            19,
            "sensitivity review"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new SensitivityReviewIngest(cache, client, loggerSr)
                .SetAsync([sr with { TargetType = new("http://example.com/target-type#DeliverableUnit"), TargetReference = assetRef }], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) =>
            {
                SetupFetchAccessCondition(client, accessConditionRef.Fragment.Substring(1), accessConditionNode);
                SetupFetchOrNewSensitivityReview(cache, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchAsset(cache, client, assetRef, assetNode);
                SetupFetchRetention(cache, client, (assetNode as IUriNode)!.Uri.ToString(), retentionNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            21,
            "sensitivity review with asset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new SensitivityReviewIngest(cache, client, loggerSr)
                .SetAsync([sr with { TargetType = new("http://example.com/target-type#DeliverableUnit"), TargetReference = subsetRef }], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) =>
            {
                SetupFetchAccessCondition(client, accessConditionRef.Fragment.Substring(1), accessConditionNode);
                SetupFetchOrNewSensitivityReview(cache, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchSubset(cache, client, subsetRef, subsetNode);
                SetupFetchRetention(cache, client, (subsetNode as IUriNode)!.Uri.ToString(), retentionNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            21,
            "sensitivity review with subset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client) => await new SensitivityReviewIngest(cache, client, loggerSr)
                .SetAsync([sr with { AccessCondition = accessConditionRef2, RestrictionDuration = 2020 }], CancellationToken.None),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) =>
            {
                SetupFetchAccessCondition(client, accessConditionRef2.Fragment.Substring(1), accessConditionNode2);
                SetupFetchOrNewSensitivityReview(cache, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchVariation(cache, client, variationRef.ToString(), variationNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            19,
            "sensitivity review with end year"
        ]
    ];
}
