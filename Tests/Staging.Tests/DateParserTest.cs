using Api;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Staging.Tests;

[TestClass]
public sealed class DateParserTest
{
    private const string xml = """
        <?xml version="1.0"?>
        <RdfNode>
            <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:tna="http://nationalarchives.gov.uk/metadata/tna#" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:trans="http://nationalarchives.gov.uk/dri/transcription">
                <tna:Test rdf:about="http://example.com/Test">
                    <tna:archivistNote rdf:parseType="Resource">
                        <tna:archivistNoteInfo>Archivist note info</tna:archivistNoteInfo>
                        <tna:archivistNoteDate>DATEPLACEHOLDER</tna:archivistNoteDate>
                    </tna:archivistNote>
                    <trans:dateOfOriginalSeal>DATERANGEPLACEHOLDER</trans:dateOfOriginalSeal>
                </tna:Test>
            </rdf:RDF>
        </RdfNode>
        """;
    private readonly DriVariationFile variationDri = new("Variation1", "/subset1", "Variation name", "Manifestation1", xml, 1, null);
    private readonly DriAssetDeliverableUnit assetDri = new("Asset1", "Asset reference", xml, "BornDigital", "[]");
    private readonly FakeLogger<VariationFileIngest> variationLogger = new();
    private readonly FakeLogger<AssetDeliverableUnitIngest> assetLogger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache = new();
    private readonly Mock<IAssetDeliverableUnitRelation> assetDeliverableUnitRelation = new();

    public DateParserTest()
    {
        var variation = new Graph();
        var asset = new Graph();
        var id = CacheClient.NewId;
        variation.Assert(id, Vocabulary.VariationDriId, new LiteralNode(variationDri.Id));
        asset.Assert(id, Vocabulary.AssetDriId, new LiteralNode(assetDri.Id));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => d["id"].ToString() == variationDri.Id), CancellationToken.None))
            .ReturnsAsync(variation);
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => d["id"].ToString() == assetDri.Id), CancellationToken.None))
            .ReturnsAsync(asset);
        variationLogger.ControlLevel(LogLevel.Trace, false);
        assetLogger.ControlLevel(LogLevel.Trace, false);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        variationLogger.Collector.Clear();
    }

    [TestMethod(DisplayName = "Rejects invalid date")]
    public async Task InvalidDate()
    {
        var ingest = new VariationFileIngest(cache.Object, client.Object, variationLogger);

        await ingest.SetAsync([variationDri], CancellationToken.None);

        using (new AssertionScope())
        {
            client.Verify(c => c.ApplyDiffAsync(It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 6 && !r.RemovedTriples.Any()),
                CancellationToken.None), Times.Once);
            variationLogger.Collector.LatestRecord.Should().Match<FakeLogRecord>(l => l.Id.Id == 26);
        }
    }

    [TestMethod(DisplayName = "Parses date")]
    [DataRow("[2001-12-31]", 2001, 12, 31, 10)]
    [DataRow("2001 Sep", 2001, 9, null, 9)]
    [DataRow("2001 Sept", 2001, 9, null, 9)]
    [DataRow("31/12/2001", 2001, 12, 31, 10)]
    [DataRow("2001 Feb 28", 2001, 2, 28, 10)]
    [DataRow("2001 Nov 1", 2001, 11, 1, 10)]
    [DataRow("2001-12-31", 2001, 12, 31, 10)]
    [DataRow("2001-12-31 10:59:45.123", 2001, 12, 31, 10)]
    [DataRow("2001", 2001, null, null, 8)]
    public async Task ParsesDate(string dateText, int year, int? month, int? day, int assertedTriples)
    {
        var ingest = new VariationFileIngest(cache.Object, client.Object, variationLogger);
        var replacedDri = variationDri with { Xml = xml.Replace("DATEPLACEHOLDER", dateText) };

        await ingest.SetAsync([replacedDri], CancellationToken.None);

        using (new AssertionScope())
        {
            client.Verify(c => c.ApplyDiffAsync(
                It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == assertedTriples && !r.RemovedTriples.Any() &&
                    r.AddedTriples.Single(t => t.Predicate == Vocabulary.Year && t.Object.AsValuedNode().AsString() == year.ToString()) != null &&
                    (month == null || r.AddedTriples.Single(t => t.Predicate == Vocabulary.Month && t.Object.AsValuedNode().AsString() == $"--{month!:D2}") != null) &&
                    (day == null || r.AddedTriples.Single(t => t.Predicate == Vocabulary.Day && t.Object.AsValuedNode().AsString() == $"---{day!:D2}") != null)),
                CancellationToken.None), Times.Once);
            variationLogger.Collector.Count.Should().Be(0);
        }
    }

    [TestMethod(DisplayName = "Rejects invalid date range")]
    public async Task InvalidDateRange()
    {
        var ingest = new AssetDeliverableUnitIngest(cache.Object, client.Object, assetLogger, assetDeliverableUnitRelation.Object);

        await ingest.SetAsync([assetDri], CancellationToken.None);

        using (new AssertionScope())
        {
            client.Verify(c => c.ApplyDiffAsync(It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 2 && !r.RemovedTriples.Any()),
                CancellationToken.None), Times.Once);
            assetLogger.Collector.LatestRecord.Should().Match<FakeLogRecord>(l => l.Id.Id == 26);
        }
    }

    [TestMethod(DisplayName = "Parses date range")]
    [DataRow("[2001-12-31]", 2001, 12, 31, null, null, null, 6)]
    [DataRow("2001 Sep", 2001, 9, null, null, null, null, 5)]
    [DataRow("2001 Sept", 2001, 9, null, null, null, null, 5)]
    [DataRow("31/12/2001", 2001, 12, 31, null, null, null, 6)]
    [DataRow("2001 Feb 28", 2001, 2, 28, null, null, null, 6)]
    [DataRow("2001 Nov 1", 2001, 11, 1, null, null, null, 6)]
    [DataRow("2001-12-31", 2001, 12, 31, null, null, null, 6)]
    [DataRow("2001-12-31 10:59:45.123", 2001, 12, 31, null, null, null, 6)]
    [DataRow("2001", 2001, null, null, null, null, null, 4)]
    [DataRow("2001-2002", 2001, null, null, 2002, null, null, 6)]
    public async Task ParsesDateRange(string dateText, int startYear, int? startMonth, int? startDay,
        int? endYear, int? endMonth, int? endDay, int assertedTriples)
    {
        var ingest = new AssetDeliverableUnitIngest(cache.Object, client.Object, assetLogger, assetDeliverableUnitRelation.Object);
        var replacedDri = assetDri with { Xml = xml.Replace("DATERANGEPLACEHOLDER", dateText) };

        await ingest.SetAsync([replacedDri], CancellationToken.None);

        using (new AssertionScope())
        {
            client.Verify(c => c.ApplyDiffAsync(
                It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == assertedTriples && !r.RemovedTriples.Any() &&
                    r.AddedTriples.Single(t =>
                        t.Subject == r.AddedTriples.Single(t => t.Predicate == Vocabulary.SealAssetHasStartDate).Object &&
                        t.Predicate == Vocabulary.Year && t.Object.AsValuedNode().AsString() == startYear.ToString()) != null &&
                    (startMonth == null || r.AddedTriples.Single(t =>
                        t.Subject == r.AddedTriples.Single(t => t.Predicate == Vocabulary.SealAssetHasStartDate).Object &&
                        t.Predicate == Vocabulary.Month && t.Object.AsValuedNode().AsString() == $"--{startMonth!:D2}") != null) &&
                    (startDay == null || r.AddedTriples.Single(t =>
                        t.Subject == r.AddedTriples.Single(t => t.Predicate == Vocabulary.SealAssetHasStartDate).Object &&
                        t.Predicate == Vocabulary.Day && t.Object.AsValuedNode().AsString() == $"---{startDay!:D2}") != null) &&
                    (endYear == null || r.AddedTriples.Single(t =>
                        t.Subject == r.AddedTriples.Single(t => t.Predicate == Vocabulary.SealAssetHasEndDate).Object &&
                        t.Predicate == Vocabulary.Year && t.Object.AsValuedNode().AsString() == endYear.ToString()) != null) &&
                    (endMonth == null || r.AddedTriples.Single(t =>
                        t.Subject == r.AddedTriples.Single(t => t.Predicate == Vocabulary.SealAssetHasEndDate).Object &&
                        t.Predicate == Vocabulary.Month && t.Object.AsValuedNode().AsString() == $"--{endMonth!:D2}") != null) &&
                    (endDay == null || r.AddedTriples.Single(t =>
                        t.Subject == r.AddedTriples.Single(t => t.Predicate == Vocabulary.SealAssetHasEndDate).Object &&
                        t.Predicate == Vocabulary.Day && t.Object.AsValuedNode().AsString() == $"---{endDay!:D2}") != null)),
                CancellationToken.None), Times.Once);
            variationLogger.Collector.Count.Should().Be(0);
        }
    }
}
