using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class SubsetIngestTest
{
    private readonly DriSubset dri = new("Subset1", "/subset1", "Subset parent");
    private readonly FakeLogger<SubsetIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode broaderSubset = CacheClient.NewId;

    public SubsetIngestTest()
    {
        cache = new();
        cache.Setup(c => c.CacheFetchOrNew(CacheEntityKind.Subset, dri.ParentReference!,
            Vocabulary.SubsetReference, CancellationToken.None))
            .ReturnsAsync(broaderSubset);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Id matches reference")]
    public void IdCalculation()
    {
        dri.Id.Should().Be(dri.Reference);
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new SubsetIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 4 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.SubsetReference, new LiteralNode(dri.Reference));
        var oldBroaderSubset = CacheClient.NewId;
        existing.Assert(id, Vocabulary.SubsetHasBroaderSubset, oldBroaderSubset);
        var retention = CacheClient.NewId;
        existing.Assert(id, Vocabulary.SubsetHasRetention, retention);
        existing.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new SubsetIngest(cache.Object, client.Object, logger);

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
        existing.Assert(id, Vocabulary.SubsetReference, new LiteralNode(dri.Reference));
        existing.Assert(id, Vocabulary.SubsetHasBroaderSubset, broaderSubset);
        var retention = CacheClient.NewId;
        existing.Assert(id, Vocabulary.SubsetHasRetention, retention);
        existing.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new SubsetIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
