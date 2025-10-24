using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging.Tests;

[TestClass]
public sealed class ChangeIngestTest
{
    private readonly DriChange dri = new("ChangeId1", "DeliverableUnit", "Asset1", DateTimeOffset.UtcNow,
        "User name", "FristName LastName", "Before and after diff");
    private readonly FakeLogger<ChangeIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode asset = CacheClient.NewId;
    private readonly IUriNode user = CacheClient.NewId;

    public ChangeIngestTest()
    {
        cache = new();
        cache.Setup(c => c.CacheFetch(CacheEntityKind.Asset, dri.Reference, CancellationToken.None))
            .ReturnsAsync(asset);
        cache.Setup(c => c.CacheFetchOrNew(CacheEntityKind.Operator, dri.UserName,
            Vocabulary.OperatorIdentifier, CancellationToken.None))
            .ReturnsAsync(user);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new ChangeIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 6 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.ChangeDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.ChangeDescription, new LiteralNode("Updated diff"));
        existing.Assert(id, Vocabulary.ChangeDateTime, new DateTimeNode(dri.Timestamp));
        existing.Assert(id, Vocabulary.ChangeHasAsset, asset);
        existing.Assert(id, Vocabulary.ChangeHasOperator, user);
        existing.Assert(user, Vocabulary.OperatorName, new LiteralNode(dri.FullName));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new ChangeIngest(cache.Object, client.Object, logger);

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
        existing.Assert(id, Vocabulary.ChangeDriId, new LiteralNode(dri.Id));
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dri.Diff));
        existing.Assert(id, Vocabulary.ChangeDescription, new LiteralNode(base64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        existing.Assert(id, Vocabulary.ChangeDateTime, new DateTimeNode(dri.Timestamp));
        existing.Assert(id, Vocabulary.ChangeHasAsset, asset);
        existing.Assert(id, Vocabulary.ChangeHasOperator, user);
        existing.Assert(user, Vocabulary.OperatorName, new LiteralNode(dri.FullName));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new ChangeIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
