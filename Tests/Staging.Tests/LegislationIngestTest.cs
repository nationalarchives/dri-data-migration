using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class LegislationIngestTest
{
    private readonly DriLegislation dri = new(new("http://example.com/legislation1"), "Section1");
    private readonly FakeLogger<LegislationIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Id matches link")]
    public void IdCalculation()
    {
        dri.Id.Should().Be(dri.Link.ToString());
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new LegislationIngest(client.Object, logger);

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
        existing.Assert(id, Vocabulary.LegislationHasUkLegislation, new UriNode(dri.Link));
        existing.Assert(id, Vocabulary.LegislationSectionReference, new LiteralNode("Updated section"));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new LegislationIngest(client.Object, logger);

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
        existing.Assert(id, Vocabulary.LegislationHasUkLegislation, new UriNode(dri.Link));
        existing.Assert(id, Vocabulary.LegislationSectionReference, new LiteralNode(dri.Section));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new LegislationIngest(client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
