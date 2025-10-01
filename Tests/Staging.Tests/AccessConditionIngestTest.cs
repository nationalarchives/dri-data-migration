using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class AccessConditionIngestTest
{
    [TestMethod("Asserts new graph")]
    public async Task Adds()
    {
        var dri = new DriAccessCondition(new Uri("http://example.com/access-condition#ac1"), "Access condition name");

        var client = new Mock<ISparqlClient>();
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());
        var logger = new FakeLogger<AccessConditionIngest>();
        var ingest = new AccessConditionIngest(client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 2 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod("Asserts updated graph")]
    public async Task Updates()
    {
        var dri = new DriAccessCondition(new Uri("http://example.com/access-condition#ac1"), "Access condition name");
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.AccessConditionCode, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.AccessConditionName, new LiteralNode("Updated name"));

        var client = new Mock<ISparqlClient>();
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var logger = new FakeLogger<AccessConditionIngest>();
        var ingest = new AccessConditionIngest(client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 1 && r.RemovedTriples.Count() == 1),
            CancellationToken.None), Times.Once);
    }

    [TestMethod("Does nothing if completly matches")]
    public async Task Skips()
    {
        var dri = new DriAccessCondition(new Uri("http://example.com/access-condition#ac1"), "Access condition name");
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.AccessConditionCode, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.AccessConditionName, new LiteralNode(dri.Name));

        var client = new Mock<ISparqlClient>();
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var logger = new FakeLogger<AccessConditionIngest>();
        var ingest = new AccessConditionIngest(client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
