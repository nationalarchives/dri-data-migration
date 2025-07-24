using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Rdf.Tests;

public class BaseIngestTest
{
    internal Mock<ISparqlClient> sparqlClient;
    internal Mock<IMemoryCache> cache;

    internal static ILogger<AccessConditionIngest> loggerAc = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AccessConditionIngest>();
    internal static ILogger<LegislationIngest> loggerLeg = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<LegislationIngest>();
    internal static ILogger<GroundForRetentionIngest> loggerGfr = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<GroundForRetentionIngest>();
    internal static ILogger<SubsetIngest> loggerSub = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SubsetIngest>();
    internal static ILogger<AssetIngest> loggerAss = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AssetIngest>();
    internal static ILogger<VariationIngest> loggerVar = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<VariationIngest>();
    internal static ILogger<SensitivityReviewIngest> loggerSr = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SensitivityReviewIngest>();

    internal const string parentSubsetRef = "Parent subset 1";
    internal const string subsetRef = "Subset 1";
    internal const string subsetRef2 = "Subset 2";
    internal const string assetRef = "Asset 1";
    internal const string assetRef2 = "Asset 2";
    internal static object? parentSubsetNode = new UriNode(new("http://example.com/parent-subset-1"));
    internal static object? subsetNode = new UriNode(new("http://example.com/subset-1"));
    private static readonly object? subsetNode2 = new UriNode(new("http://example.com/subset-2"));
    internal static object? assetNode = new UriNode(new("http://example.com/asset-1"));
    private static readonly object? assetNode2 = new UriNode(new("http://example.com/asset-2"));
    internal static object? retentionNode = new UriNode(new("http://example.com/retention-1"));
    internal static readonly Uri variationRef = new("http://example.com/variation1");
    internal static object? variationNode = new UriNode(new("http://example.com/variation-1"));
    internal static readonly Uri accessConditionRef = new("http://example.com/ac1#ac1");
    internal static readonly Uri accessConditionRef2 = new("http://example.com/ac1#D");
    internal static readonly IUriNode accessConditionNode = new UriNode(new("http://example.com/access-condition"));
    internal static readonly IUriNode accessConditionNode2 = new UriNode(new("http://example.com/access-condition-2"));
    internal static readonly Uri legislationRef = new("http://example.com/legislation1");
    internal static readonly Uri legislationRef2 = new("http://example.com/legislation2");
    internal static readonly IUriNode legislationNode = new UriNode(new("http://example.com/legislation"));
    private static readonly IUriNode legislationNode2 = new UriNode(new("http://example.com/legislation2"));
    internal static readonly Uri groundForRetentionRef = new("http://example.com/gfr#gfr1");
    internal static readonly Uri groundForRetentionRef2 = new("http://example.com/gfr#gfr2");
    internal static readonly IUriNode groundForRetentionNode = new UriNode(new("http://example.com/ground-for-retention"));
    private static readonly IUriNode groundForRetentionNode2 = new UriNode(new("http://example.com/ground-for-retention-2"));
    internal static readonly Uri previousSrRef = new("http://example.com/previoussr");
    internal static readonly Uri previousSrRef2 = new("http://example.com/previoussr2");
    internal static object? previousSrNode = new UriNode(new("http://example.com/previous-sr-1"));
    private static readonly object? previousSrNode2 = new UriNode(new("http://example.com/previous-sr-2"));

    internal void Initialize()
    {
        sparqlClient = new Mock<ISparqlClient>();
        cache = new Mock<IMemoryCache>();
    }

    internal static void SetupFetchOrNewSubset(Mock<IMemoryCache> cache, string reference, object? node) =>
        SetupFetchOrNew(cache, "subset", reference, node);

    internal static void SetupFetchOrNewSensitivityReview(Mock<IMemoryCache> cache, string reference, object? node) =>
        SetupFetchOrNew(cache, "sensititvity-review", reference, node);

    internal static void SetupFetchSubset(Mock<IMemoryCache> cache, Mock<ISparqlClient> client, string reference, object? node) =>
        SetupFetch(cache, client, "subsetReference", "subset", reference, node);

    internal static void SetupFetchAsset(Mock<IMemoryCache> cache, Mock<ISparqlClient> client, string reference, object? node) =>
        SetupFetch(cache, client, "assetReference", "asset", reference, node);

    internal static void SetupFetchVariation(Mock<IMemoryCache> cache, Mock<ISparqlClient> client, string reference, object? node) =>
        SetupFetch(cache, client, "variationDriId", "variation", reference, node);

    internal static void SetupFetchRetention(Mock<IMemoryCache> cache, Mock<ISparqlClient> client, string reference, object? node) =>
        SetupFetch(cache, client, "assetHasRetention", "retention", reference, node);

    internal static void SetupFetchAccessCondition(Mock<ISparqlClient> client, string key, IUriNode value) =>
        SetupFetchDictionary(client, "accessConditionCode", key, value);

    internal static void SetupFetchLegislation(Mock<ISparqlClient> client, string key, IUriNode value) =>
        SetupFetchDictionary(client, "legislationHasUkLegislation", key, value);

    internal static void SetupFetchGroundForRetention(Mock<ISparqlClient> client, string key, IUriNode value) =>
        SetupFetchDictionary(client, "groundForRetentionCode", key, value);

    internal static Action<Mock<ISparqlClient>, Mock<IMemoryCache>> NoopSetup() => (client, cache) => { };

    private static void SetupFetchOrNew(Mock<IMemoryCache> cache, string prefix, string reference, object? node) =>
        cache.Setup(c => c.TryGetValue(It.Is<object>(m => m.Equals($"{prefix}-{reference}")), out node)).Returns(true);

    private static void SetupFetch(Mock<IMemoryCache> cache, Mock<ISparqlClient> client, string match, string prefix, string reference, object? node)
    {
        client.Setup(c => c.GetSubjectAsync(It.Is<string>(m => m.Contains(match)), It.Is<object>(m => m.ToString() == reference)))
            .ReturnsAsync(node as IUriNode);
        cache.Setup(c => c.CreateEntry(It.Is<object>(m => m.Equals($"{prefix}-{reference}"))))
            .Returns(Mock.Of<ICacheEntry>());
    }

    private static void SetupFetchDictionary(Mock<ISparqlClient> client, string match, string key, IUriNode value) =>
        client.Setup(s => s.GetDictionaryAsync(It.Is<string>(m => m.Contains(match))))
            .Returns(Task.FromResult(new Dictionary<string, IUriNode> { { key, value } }));

    internal static readonly DriAccessCondition accessCondition = new(new Uri("http://example.com/ac#ac1"), "Access Condition 1");
    internal static readonly DriGroundForRetention groundForRetention = new("Code 1", "Ground for retention 1");
    internal static readonly DriLegislation legislation = new(new Uri("http://example.com/legislation"), "Section 1");
    internal static readonly DriSubset subset = new("Subset 1", "Directory 1", parentSubsetRef);
    internal static readonly DriAsset asset = new("Asset 1", "Directory 1", subsetRef);
    internal static readonly DriVariation variation = new(new("http://example.com/variation"), "Variation name 1", assetRef);
    internal static readonly DriSensitivityReview sr = new(new("http://example.com/sr"), "Variation name 1", variationRef,
        new("http://example.com/target-type#variation"), accessConditionRef, [legislationRef],
        DateTimeOffset.UtcNow, previousSrRef, "Sensitive name", "Sensitive description", DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow, 1, "Restriction description", 2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, groundForRetentionRef);

    internal static IGraph Build(DriAccessCondition dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        graph.Assert(id, Vocabulary.AccessConditionCode, new LiteralNode(dri.Link.Fragment.Substring(1)));
        graph.Assert(id, Vocabulary.AccessConditionName, new LiteralNode(dri.Name));

        return graph;
    }

    internal static IGraph Build(DriGroundForRetention dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        graph.Assert(id, Vocabulary.GroundForRetentionCode, new LiteralNode(dri.Code));
        graph.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode(dri.Description));

        return graph;
    }

    internal static IGraph Build(DriLegislation dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        graph.Assert(id, Vocabulary.LegislationHasUkLegislation, new UriNode(dri.Link));
        graph.Assert(id, Vocabulary.LegislationSectionReference, new LiteralNode(dri.Section));

        return graph;
    }

    internal static IGraph Build(DriSubset dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        graph.Assert(id, Vocabulary.SubsetReference, new LiteralNode(dri.Reference));
        graph.Assert(id, Vocabulary.SubsetHasRetention, (IUriNode?)retentionNode);
        graph.Assert((IUriNode?)retentionNode, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));
        graph.Assert(id, Vocabulary.SubsetHasBroaderSubset, (IUriNode?)parentSubsetNode);
        graph.Assert((IUriNode?)parentSubsetNode, Vocabulary.SubsetReference, new LiteralNode(dri.ParentReference));

        return graph;
    }

    internal static IGraph Build(DriAsset dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        graph.Assert(id, Vocabulary.AssetReference, new LiteralNode(dri.Reference));
        if (dri.SubsetReference == subsetRef)
        {
            graph.Assert(id, Vocabulary.AssetHasSubset, (IUriNode?)subsetNode);
        }
        if (dri.SubsetReference == subsetRef2)
        {
            graph.Assert(id, Vocabulary.AssetHasSubset, (IUriNode?)subsetNode2);
        }
        graph.Assert(id, Vocabulary.AssetHasRetention, (IUriNode?)retentionNode);
        graph.Assert((IUriNode?)retentionNode, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));

        return graph;
    }

    internal static IGraph Build(DriVariation dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        if (dri.AssetReference == assetRef)
        {
            graph.Assert(id, Vocabulary.VariationHasAsset, (IUriNode?)assetNode);
        }
        if (dri.AssetReference == assetRef2)
        {
            graph.Assert(id, Vocabulary.VariationHasAsset, (IUriNode?)assetNode2);
        }
        graph.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        graph.Assert(id, Vocabulary.VariationName, new LiteralNode(dri.VariationName));

        return graph;
    }

    internal static IGraph Build(DriSensitivityReview dri)
    {
        var graph = new Graph();
        var id = new UriNode(new Uri("http://example.com/s"));
        var restriction = new UriNode(new Uri("http://example.com/r"));
        var retentionRestriction = new UriNode(new Uri("http://example.com/rr"));

        graph.Assert(id, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.Id));
        graph.Assert(id, Vocabulary.SensitivityReviewDate, new DateNode(dri.Date!.Value));
        graph.Assert(id, Vocabulary.SensitivityReviewSensitiveName, new LiteralNode(dri.SensitiveName));
        graph.Assert(id, Vocabulary.SensitivityReviewSensitiveDescription, new LiteralNode(dri.SensitiveDescription));
        if (dri.AccessCondition.ToString() == accessConditionRef.ToString())
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasAccessCondition, accessConditionNode);
        }
        if (dri.AccessCondition.ToString() == accessConditionRef2.ToString())
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasAccessCondition, accessConditionNode2);
        }
        if (dri.PreviousId?.ToString() == previousSrRef.ToString())
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, (IUriNode?)previousSrNode);
            graph.Assert((IUriNode?)previousSrNode, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.PreviousId.ToString()));
        }
        if (dri.PreviousId?.ToString() == previousSrRef2.ToString())
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, (IUriNode?)previousSrNode2);
            graph.Assert((IUriNode?)previousSrNode2, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.PreviousId.ToString()));
        }
        graph.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateNode(dri.ReviewDate!.Value));
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateNode(dri.RestrictionStartDate!.Value));
        if (new string[] { "D", "U" }.Contains(dri.AccessCondition.Fragment.Substring(1)))
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionEndYear, new LongNode(dri.RestrictionDuration!.Value));
        }
        else
        {
            graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LiteralNode($"P{dri.RestrictionDuration}Y", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
        }
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDescription, new LiteralNode(dri.RestrictionDescription));
        foreach (var item in dri.Legislations)
        {
            if (item.ToString() == legislationRef.ToString())
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislationNode);
            }
            if (item.ToString() == legislationRef2.ToString())
            {
                graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislationNode2);
            }
        }
        if (dri.TargetType.Fragment == "#DeliverableUnit")
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasAsset, (IUriNode?)assetNode);
        }
        else
        {
            graph.Assert(id, Vocabulary.SensitivityReviewHasVariation, (IUriNode?)variationNode);
        }
        graph.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
        graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber!.Value));
        graph.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate, new DateNode(dri.InstrumentSignedDate!.Value));
        graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateNode(dri.RestrictionReviewDate!.Value));
        if (dri.GroundForRetention!.ToString() == groundForRetentionRef.ToString())
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, groundForRetentionNode);
        }
        if (dri.GroundForRetention.ToString() == groundForRetentionRef2.ToString())
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, groundForRetentionNode2);
        }
        if (dri.TargetReference == assetRef)
        {
            graph.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasRetention, (IUriNode?)retentionNode);
            graph.Assert((IUriNode?)assetNode, Vocabulary.AssetHasRetention, (IUriNode?)retentionNode);
        }

        return graph;
    }
}
