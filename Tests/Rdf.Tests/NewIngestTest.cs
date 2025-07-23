using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    public async Task ChangeIngest(Func<IMemoryCache, ISparqlClient, ILogger, Task> ingest,
         Action<Mock<ISparqlClient>, Mock<IMemoryCache>> additionalSetup, int addedCount, string _)
    {
        sparqlClient.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new Graph());
        additionalSetup(sparqlClient, cache);

        await ingest(cache.Object, sparqlClient.Object, logger);

        sparqlClient.Verify(c => c.ApplyDiffAsync(It.Is<GraphDiffReport>(r =>
            r.AddedTriples.Count() == addedCount && !r.RemovedTriples.Any())));
    }

    public static IEnumerable<object[]> ChangeIngestData => [
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new AccessConditionIngest(cache, client, logger)
                .Set([accessCondition]),
            NoopSetup(),
            2,
            "access condition"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new GroundForRetentionIngest(cache, client, logger)
                .Set([groundForRetention]),
            NoopSetup(),
            2,
            "ground for retention"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new LegislationIngest(cache, client, logger)
                .Set([legislation]),
            NoopSetup(),
            2,
            "legislation"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SubsetIngest(cache, client, logger)
                .Set([subset]),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchOrNewSubset(cache, parentSubsetRef, parentSubsetNode),
            5,
            "subset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SubsetIngest(cache, client, logger)
                .Set([subset]),
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
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new AssetIngest(cache, client, logger)
                .Set([asset]),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchSubset(cache, client, subsetRef, subsetNode),
            4,
            "asset"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new VariationIngest(cache, client, logger)
                .Set([variation]),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchAsset(cache, client, assetRef, assetNode),
            3,
            "variation"
        ],
        [
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SensitivityReviewIngest(cache, client, logger)
                .Set([sr]),
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
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SensitivityReviewIngest(cache, client, logger)
                .Set([sr with { TargetType = new("http://example.com/target-type#DeliverableUnit"), TargetReference = assetRef }]),
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
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SensitivityReviewIngest(cache, client, logger)
                .Set([sr with { TargetType = new("http://example.com/target-type#DeliverableUnit"), TargetReference = subsetRef }]),
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
            async (IMemoryCache cache, ISparqlClient client, ILogger logger) => await new SensitivityReviewIngest(cache, client, logger)
                .Set([sr with { AccessCondition = accessConditionRef2, RestrictionDuration = 2020 }]),
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
