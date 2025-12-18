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
            <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:tna="http://nationalarchives.gov.uk/metadata/tna#" xmlns:dcterms="http://purl.org/dc/terms/">
                <tna:Test rdf:about="http://example.com/Test">
                    <tna:archivistNote rdf:parseType="Resource">
                        <tna:archivistNoteInfo>Archivist note info</tna:archivistNoteInfo>
                        <tna:archivistNoteDate>PLACEHOLDER</tna:archivistNoteDate>
                    </tna:archivistNote>
                </tna:Test>
            </rdf:RDF>
        </RdfNode>
        """;
    private readonly DriVariationFile dri = new("Variation1", "/subset1", "Variation name", "Manifestation1", xml);
    private readonly FakeLogger<VariationFileIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache = new();

    public DateParserTest()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        logger.ControlLevel(LogLevel.Trace, false);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        logger.Collector.Clear();
    }

    [TestMethod(DisplayName = "Rejects invalid date")]
    public async Task InvalidDate()
    {
        var ingest = new VariationFileIngest(cache.Object, client.Object, logger);

        await ingest.SetAsync([dri], CancellationToken.None);

        using (new AssertionScope())
        {
            client.Verify(c => c.ApplyDiffAsync(It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 5 && !r.RemovedTriples.Any()),
                CancellationToken.None), Times.Once);
            logger.Collector.LatestRecord.Should().Match<FakeLogRecord>(l => l.Id.Id == 26);
        }
    }

    [TestMethod(DisplayName = "Parses date")]
    [DataRow("[2001-12-31]", 2001, 12, 31, 9)]
    [DataRow("2001 Sep", 2001, 9, null, 8)]
    [DataRow("2001 Sept", 2001, 9, null, 8)]
    [DataRow("31/12/2001", 2001, 12, 31, 9)]
    [DataRow("2001 Feb 28", 2001, 2, 28, 9)]
    [DataRow("2001 Nov 1", 2001, 11, 1, 9)]
    [DataRow("2001-12-31", 2001, 12, 31, 9)]
    [DataRow("2001-12-31 10:59:45.123", 2001, 12, 31, 9)]
    [DataRow("2001", 2001, null, null, 7)]
    public async Task ParsesDate(string dateText, int year, int? month, int? day, int assertedTriples)
    {
        var ingest = new VariationFileIngest(cache.Object, client.Object, logger);
        var replacedDri = dri with { Xml = xml.Replace("PLACEHOLDER", dateText) };

        await ingest.SetAsync([replacedDri], CancellationToken.None);

        using (new AssertionScope())
        {
            client.Verify(c => c.ApplyDiffAsync(
                It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == assertedTriples && !r.RemovedTriples.Any() &&
                    r.AddedTriples.Single(t => t.Predicate == Vocabulary.Year && t.Object.AsValuedNode().AsString() == year.ToString()) != null &&
                    (month == null || r.AddedTriples.Single(t => t.Predicate == Vocabulary.Month && t.Object.AsValuedNode().AsString() == $"--{month!:D2}") != null) &&
                    (day == null || r.AddedTriples.Single(t => t.Predicate == Vocabulary.Day && t.Object.AsValuedNode().AsString() == $"---{day!:D2}") != null)),
                CancellationToken.None), Times.Once);
            logger.Collector.Count.Should().Be(0);
        }
    }
}
