using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class AssetIngestTest
{
    private readonly DriAsset dri = new(new("http://example.com/asset1"), "Asset1", "/subset1", "Subset1");
    private readonly FakeLogger<AssetIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode subset = CacheClient.NewId;
    private readonly IUriNode retention = CacheClient.NewId;

    public AssetIngestTest()
    {
        cache = new();
        cache.Setup(c => c.CacheFetch(CacheEntityKind.Subset, dri.SubsetReference, CancellationToken.None))
            .ReturnsAsync(subset);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod("Id matches link path segment")]
    public void IdCalculation()
    {
        dri.Id.Should().Be("asset1");
    }

    [TestMethod("Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new AssetIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 5 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod("Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.AssetDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.AssetReference, new LiteralNode(dri.Reference));
        existing.Assert(id, Vocabulary.AssetHasSubset, subset);
        existing.Assert(id, Vocabulary.AssetHasRetention, retention);
        existing.Assert(retention, Vocabulary.ImportLocation, new LiteralNode("Updated location"));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new AssetIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 1 && r.RemovedTriples.Count() == 1),
            CancellationToken.None), Times.Once);
    }

    [TestMethod("Does nothing if completly matches existing data")]
    public async Task IsIdempotent()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.AssetDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.AssetReference, new LiteralNode(dri.Reference));
        existing.Assert(id, Vocabulary.AssetHasSubset, subset);
        existing.Assert(id, Vocabulary.AssetHasRetention, retention);
        existing.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new AssetIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
