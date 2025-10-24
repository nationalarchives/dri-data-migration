using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class VariationIngestTest
{
    private readonly DriVariation dri = new(new("http://example.com/variation1"), "Variation1", "Asset1");
    private readonly FakeLogger<VariationIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode asset = CacheClient.NewId;

    public VariationIngestTest()
    {
        cache = new();
        cache.Setup(c => c.CacheFetch(CacheEntityKind.Asset, dri.AssetReference, CancellationToken.None))
            .ReturnsAsync(asset);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Id matches link path segment")]
    public void IdCalculation()
    {
        dri.Id.Should().Be("variation1");
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new VariationIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 3 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.VariationHasAsset, asset);
        existing.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.VariationName, new LiteralNode("Updated name"));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new VariationIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 1 && r.RemovedTriples.Count() == 1),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Does nothing if completly matches existing data")]
    public async Task IsIdempotent()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.VariationHasAsset, asset);
        existing.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.VariationName, new LiteralNode(dri.VariationName));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new VariationIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
