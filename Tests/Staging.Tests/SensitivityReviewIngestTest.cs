using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging.Tests;

[TestClass]
public sealed class SensitivityReviewIngestTest
{
    private readonly DriSensitivityReview dri = new(new("http://example.com/sr1"),
        "Variation1", new("http://example.com/variation1"), new("http://example.com/type#File"),
        new("http://example.com/access-condition#ac1"), [new("http://example.com/legislation1")],
        DateTimeOffset.UtcNow.AddDays(-1), new("http://example.com/sr-previous"),
        "Sensitive name", "Sensitive description", DateTimeOffset.UtcNow.AddDays(-2),
        DateTimeOffset.UtcNow.AddDays(-3), 1, "Restriction description", 2,
        DateTimeOffset.UtcNow.AddDays(-4), DateTimeOffset.UtcNow.AddDays(-5),
        new("http://example.com/ground-for-retention#gfr1"));
    private readonly FakeLogger<SensitivityReviewIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode accessCondition = CacheClient.NewId;
    private readonly IUriNode legislation = CacheClient.NewId;
    private readonly IUriNode gfr = CacheClient.NewId;
    private readonly IUriNode previousSr = CacheClient.NewId;
    private readonly IUriNode restriction = CacheClient.NewId;
    private readonly IUriNode variation = CacheClient.NewId;
    private readonly IUriNode retentionRestriction = CacheClient.NewId;

    public SensitivityReviewIngestTest()
    {
        cache = new();
        cache.Setup(c => c.AccessConditions(CancellationToken.None))
            .ReturnsAsync(new Dictionary<string, IUriNode> { ["ac1"] = accessCondition });
        cache.Setup(c => c.Legislations(CancellationToken.None))
            .ReturnsAsync(new Dictionary<string, IUriNode> { [dri.Legislations.Single().ToString()] = legislation });
        cache.Setup(c => c.GroundsForRetention(CancellationToken.None))
            .ReturnsAsync(new Dictionary<string, IUriNode> { ["gfr1"] = gfr });
        cache.Setup(c => c.CacheFetchOrNew(CacheEntityKind.SensititvityReview, dri.PreviousId,
            Vocabulary.SensitivityReviewDriId, CancellationToken.None))
            .ReturnsAsync(previousSr);
        cache.Setup(c => c.CacheFetch(CacheEntityKind.Variation, dri.TargetId, CancellationToken.None))
            .ReturnsAsync(variation);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod("Id matches link path segment")]
    public void IdCalculation()
    {
        dri.Id.Should().Be("sr1");
    }

    [TestMethod("Target ID matches target link path segment")]
    public void TargetIdCalculation()
    {
        dri.TargetId.Should().Be("variation1");
    }

    [TestMethod("Previous ID matches previous link path segment")]
    public void PreviousIdCalculation()
    {
        dri.PreviousId.Should().Be("sr-previous");
    }

    [TestMethod("Asserts new graph")]
    public async Task Adds()
    {
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(new Graph());

        var ingest = new SensitivityReviewIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 18 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod("Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.SensitivityReviewDate, new DateNode(dri.Date!.Value));
        existing.Assert(id, Vocabulary.SensitivityReviewSensitiveName, new LiteralNode("Updated sensitive name"));
        existing.Assert(id, Vocabulary.SensitivityReviewSensitiveDescription, new LiteralNode(dri.SensitiveDescription));
        existing.Assert(id, Vocabulary.SensitivityReviewHasAccessCondition, accessCondition);
        existing.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, previousSr);
        existing.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateNode(dri.ReviewDate!.Value));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateNode(dri.RestrictionStartDate!.Value));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LiteralNode($"P{dri.RestrictionDuration!.Value}Y", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDescription, new LiteralNode(dri.RestrictionDescription));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislation);
        existing.Assert(id, Vocabulary.SensitivityReviewHasVariation, variation);
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
        existing.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber!.Value));
        existing.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate, new DateNode(dri.InstrumentSignedDate!.Value));
        existing.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateNode(dri.RestrictionReviewDate!.Value));
        existing.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, gfr);

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new SensitivityReviewIngest(cache.Object, client.Object, logger);

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
        existing.Assert(id, Vocabulary.SensitivityReviewDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.SensitivityReviewDate, new DateNode(dri.Date!.Value));
        existing.Assert(id, Vocabulary.SensitivityReviewSensitiveName, new LiteralNode(dri.SensitiveName));
        existing.Assert(id, Vocabulary.SensitivityReviewSensitiveDescription, new LiteralNode(dri.SensitiveDescription));
        existing.Assert(id, Vocabulary.SensitivityReviewHasAccessCondition, accessCondition);
        existing.Assert(id, Vocabulary.SensitivityReviewHasPastSensitivityReview, previousSr);
        existing.Assert(id, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction, restriction);
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate, new DateNode(dri.ReviewDate!.Value));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate, new DateNode(dri.RestrictionStartDate!.Value));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDuration, new LiteralNode($"P{dri.RestrictionDuration!.Value}Y", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionDescription, new LiteralNode(dri.RestrictionDescription));
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation, legislation);
        existing.Assert(id, Vocabulary.SensitivityReviewHasVariation, variation);
        existing.Assert(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction, retentionRestriction);
        existing.Assert(retentionRestriction, Vocabulary.RetentionInstrumentNumber, new LongNode(dri.InstrumentNumber!.Value));
        existing.Assert(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate, new DateNode(dri.InstrumentSignedDate!.Value));
        existing.Assert(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate, new DateNode(dri.RestrictionReviewDate!.Value));
        existing.Assert(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention, gfr);

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new SensitivityReviewIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
