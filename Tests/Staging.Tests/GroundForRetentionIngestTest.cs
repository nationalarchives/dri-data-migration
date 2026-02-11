using Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Diagnostics.Metrics;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class GroundForRetentionIngestTest
{
    private readonly DriGroundForRetention dri = new("Ground for retention label", "Ground for retention comment");
    private readonly FakeLogger<GroundForRetentionIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly IMeterFactory meterFactory;

    public GroundForRetentionIngestTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        meterFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IMeterFactory>();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Id matches code")]
    public void IdCalculation()
    {
        dri.Id.Should().Be(dri.Code);
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new GroundForRetentionIngest(client.Object, logger, meterFactory);

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
        existing.Assert(id, Vocabulary.GroundForRetentionCode, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode("Updated comment"));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new GroundForRetentionIngest(client.Object, logger, meterFactory);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 1 && r.RemovedTriples.Count() == 1),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Does nothing if completely matches existing data")]
    public async Task IsIdempotent()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.GroundForRetentionCode, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode(dri.Description));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new GroundForRetentionIngest(client.Object, logger, meterFactory);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
