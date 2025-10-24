using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class AccessConditionIngestTest
{
    private readonly DriAccessCondition dri = new(new("http://example.com/access-condition#ac1"), "Access condition name");
    private readonly FakeLogger<AccessConditionIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Id matches link fragment")]
    public void IdCalculation()
    {
        dri.Id.Should().Be("ac1");
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());
        var ingest = new AccessConditionIngest(client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 2 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.AccessConditionCode, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.AccessConditionName, new LiteralNode("Updated name"));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new AccessConditionIngest(client.Object, logger);

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
        existing.Assert(id, Vocabulary.AccessConditionCode, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.AccessConditionName, new LiteralNode(dri.Name));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new AccessConditionIngest(client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
