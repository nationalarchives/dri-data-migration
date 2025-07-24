using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using VDS.RDF;

namespace Rdf.Tests;

[TestClass]
public class UpdateIngestTest : BaseIngestTest
{
    [TestInitialize]
    public void TestInitialize() => Initialize();

    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("Performs update when there is any change")]
    [DynamicData(nameof(ChangeIngestData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task ChangeIngest(string id, IGraph existing, Action<Mock<ISparqlClient>, Mock<IMemoryCache>> additionalSetup,
        Func<IMemoryCache, ISparqlClient, Task> ingest, int addedCount, int removedCount, string _)
    {
        sparqlClient.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => d["id"].ToString() == id)))
            .ReturnsAsync(existing);
        additionalSetup(sparqlClient, cache);

        await ingest(cache.Object, sparqlClient.Object);

        sparqlClient.Verify(c => c.ApplyDiffAsync(It.Is<GraphDiffReport>(r =>
            r.AddedTriples.Count() == addedCount && r.RemovedTriples.Count() == removedCount)));
    }

    public static IEnumerable<object[]> ChangeIngestData => [
        [
            accessCondition.Id,
            Build(accessCondition with { Name = "Old access condition name" }),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client) => await new AccessConditionIngest(cache, client, loggerAc).SetAsync([accessCondition]),
            1, 1,
            "access condition"
        ],
        [
            groundForRetention.Id,
            Build(groundForRetention with { Description = "Old ground for retention description" }),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client) => await new GroundForRetentionIngest(cache, client, loggerGfr).SetAsync([groundForRetention]),
            1, 1,
            "ground for retention"
        ],
        [
            legislation.Id,
            Build(legislation with { Section = "Old legislation section" }),
            NoopSetup(),
            async (IMemoryCache cache, ISparqlClient client) => await new LegislationIngest(cache, client, loggerLeg).SetAsync([legislation]),
            1, 1,
            "legislation"
        ],
        [
            subset.Id,
            Build(subset with { ParentReference = "Old parent subset reference" }),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchOrNewSubset(cache, parentSubsetRef, parentSubsetNode),
            async (IMemoryCache cache, ISparqlClient client) => await new SubsetIngest(cache, client, loggerSub).SetAsync([subset]),
            1, 1,
            "subset"
        ],
        [
            asset.Id,
            Build(asset with { Directory = "Old asset directory", SubsetReference = subsetRef2 }),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchSubset(cache, client, subsetRef, subsetNode),
            async (IMemoryCache cache, ISparqlClient client) => await new AssetIngest(cache, client, loggerAss).SetAsync([asset]),
            2, 2,
            "asset"
        ],
        [
            variation.Id,
            Build(variation with { AssetReference = assetRef2, VariationName = "Old variation name" }),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) => SetupFetchAsset(cache, client, assetRef, assetNode),
            async (IMemoryCache cache, ISparqlClient client) => await new VariationIngest(cache, client, loggerVar).SetAsync([variation]),
            2, 2,
            "variation"
        ],
        [
            sr.Id,
            Build(sr with {
                AccessCondition = accessConditionRef2, Date = DateTimeOffset.UtcNow.AddDays(-1),
                GroundForRetention = groundForRetentionRef2, InstrumentNumber = 10,
                InstrumentSignedDate = DateTimeOffset.UtcNow.AddDays(-1),
                Legislations = [legislationRef, legislationRef2], PreviousId = previousSrRef2,
                RestrictionDescription = "Old restriction description", RestrictionDuration = 2020,
                RestrictionReviewDate = DateTimeOffset.UtcNow.AddDays(-1),
                RestrictionStartDate = DateTimeOffset.UtcNow.AddDays(-1), ReviewDate = DateTimeOffset.UtcNow.AddDays(-1),
                SensitiveDescription = "Old sensitive description", SensitiveName = "Old sensitive name",
                TargetId = new("http://example.com/oldasset"), TargetReference = assetRef,
                TargetType = new("http://example.com/target-type#DeliverableUnit")
            }),
            (Mock<ISparqlClient> client, Mock<IMemoryCache> cache) =>
            {
                SetupFetchAccessCondition(client, accessConditionRef.Fragment.Substring(1), accessConditionNode);
                SetupFetchOrNewSensitivityReview(cache, previousSrRef.ToString(), previousSrNode);
                SetupFetchLegislation(client, legislationRef.ToString(), legislationNode);
                SetupFetchVariation(cache, client, variationRef.ToString(), variationNode);
                SetupFetchGroundForRetention(client, groundForRetentionRef.Fragment.Substring(1), groundForRetentionNode);
            },
            async (IMemoryCache cache, ISparqlClient client) => await new SensitivityReviewIngest(cache, client, loggerSr).SetAsync([sr]),
            15, 18,
            "sensitivity review"
        ]
    ];
}
