using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging.Tests;

[TestClass]
public sealed class VariationFileIngestTest
{
    private const string xml = """
        <?xml version="1.0"?>
        <RdfNode>
            <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:tna="http://nationalarchives.gov.uk/metadata/tna#" xmlns:dcterms="http://purl.org/dc/terms/">
                <tna:Test rdf:about="http://example.com/Test">
                    <tna:note>Some note</tna:note>
                    <rdfs:comment>Some comment</rdfs:comment>
                    <tna:physicalCondition>Some physical condition</tna:physicalCondition>
                    <tna:googleId>Google Id</tna:googleId>
                    <tna:googleParentId>Google parent Id</tna:googleParentId>
                    <tna:scanId>Scan Id</tna:scanId>
                    <tna:scanOperator>Scan operator</tna:scanOperator>
                    <tna:scanLocation>Scan location</tna:scanLocation>
                    <tna:imageSplit>yes</tna:imageSplit>
                    <tna:imageCrop>manual</tna:imageCrop>
                    <tna:imageDeskew>auto</tna:imageDeskew>
                    <tna:archivistNote rdf:parseType="Resource">
                        <tna:archivistNoteInfo>Archivist note info</tna:archivistNoteInfo>
                        <tna:archivistNoteDate>30 Sept 2001</tna:archivistNoteDate>
                    </tna:archivistNote>
                    <dcterms:description>Temporary description to be removed</dcterms:description>
                </tna:Test>
            </rdf:RDF>
        </RdfNode>
        """;
    private readonly DriVariationFile dri = new("Variation1", "/subset1", "Variation name", "Manifestation1", xml);
    private readonly FakeLogger<VariationFileIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode scanLocation = CacheClient.NewId;
    private readonly IUriNode datedNote = CacheClient.NewId;
    private readonly IUriNode date = CacheClient.NewId;

    public VariationFileIngestTest()
    {
        cache = new();
        cache.Setup(c => c.CacheFetchOrNew(CacheEntityKind.GeographicalPlace, "Scan location",
            Vocabulary.GeographicalPlaceName, CancellationToken.None))
            .ReturnsAsync(scanLocation);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        client.Reset();
    }

    [TestMethod(DisplayName = "Asserts new graph")]
    public async Task Adds()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);

        var ingest = new VariationFileIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 21 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Asserts updated graph")]
    public async Task Updates()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.VariationDriManifestationId, new LiteralNode(dri.ManifestationId));
        existing.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dri.Xml));
        existing.Assert(id, Vocabulary.VariationDriXml, new LiteralNode(base64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        existing.Assert(id, Vocabulary.VariationNote, new LiteralNode("Some note"));
        existing.Assert(id, Vocabulary.VariationNote, new LiteralNode("Some comment"));
        existing.Assert(id, Vocabulary.VariationPhysicalConditionDescription, new LiteralNode("Some physical condition"));
        existing.Assert(id, Vocabulary.VariationReferenceGoogleId, new LiteralNode("Google Id"));
        existing.Assert(id, Vocabulary.VariationReferenceParentGoogleId, new LiteralNode("Google parent Id"));
        existing.Assert(id, Vocabulary.ScannedVariationHasScannerGeographicalPlace, scanLocation);
        existing.Assert(id, Vocabulary.ScannerIdentifier, new LiteralNode("Scan Id"));
        existing.Assert(id, Vocabulary.ScannerOperatorIdentifier, new LiteralNode("Scan operator"));
        existing.Assert(id, Vocabulary.ScannedVariationHasImageSplit, Vocabulary.ImageSplit);
        existing.Assert(id, Vocabulary.ScannedVariationHasImageCrop, Vocabulary.ManualImageCrop);
        existing.Assert(id, Vocabulary.ScannedVariationHasImageDeskew, Vocabulary.AutoImageDeskew);
        existing.Assert(id, Vocabulary.VariationHasDatedNote, datedNote);
        existing.Assert(datedNote, Vocabulary.ArchivistNote, new LiteralNode("Archivist note info"));
        existing.Assert(datedNote, Vocabulary.DatedNoteHasDate, date);
        existing.Assert(date, Vocabulary.Year, new LiteralNode("1999", new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
        existing.Assert(date, Vocabulary.Month, new LiteralNode("--01", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gMonth")));
        existing.Assert(date, Vocabulary.Day, new LiteralNode("---02", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gDay")));
        existing.Assert(id, IngestVocabulary.Description, new LiteralNode("Temporary description to be removed"));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new VariationFileIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 3 && r.RemovedTriples.Count() == 3),
            CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Does nothing if completly matches existing data")]
    public async Task IsIdempotent()
    {
        var existing = new Graph();
        var id = CacheClient.NewId;
        existing.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        existing.Assert(id, Vocabulary.VariationDriManifestationId, new LiteralNode(dri.ManifestationId));
        existing.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dri.Xml));
        existing.Assert(id, Vocabulary.VariationDriXml, new LiteralNode(base64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        existing.Assert(id, Vocabulary.VariationNote, new LiteralNode("Some note"));
        existing.Assert(id, Vocabulary.VariationNote, new LiteralNode("Some comment"));
        existing.Assert(id, Vocabulary.VariationPhysicalConditionDescription, new LiteralNode("Some physical condition"));
        existing.Assert(id, Vocabulary.VariationReferenceGoogleId, new LiteralNode("Google Id"));
        existing.Assert(id, Vocabulary.VariationReferenceParentGoogleId, new LiteralNode("Google parent Id"));
        existing.Assert(id, Vocabulary.ScannedVariationHasScannerGeographicalPlace, scanLocation); 
        existing.Assert(id, Vocabulary.ScannerIdentifier, new LiteralNode("Scan Id"));
        existing.Assert(id, Vocabulary.ScannerOperatorIdentifier, new LiteralNode("Scan operator"));
        existing.Assert(id, Vocabulary.ScannedVariationHasImageSplit, Vocabulary.ImageSplit);
        existing.Assert(id, Vocabulary.ScannedVariationHasImageCrop, Vocabulary.ManualImageCrop);
        existing.Assert(id, Vocabulary.ScannedVariationHasImageDeskew, Vocabulary.AutoImageDeskew);
        existing.Assert(id, Vocabulary.VariationHasDatedNote, datedNote);
        existing.Assert(datedNote, Vocabulary.ArchivistNote, new LiteralNode("Archivist note info"));
        existing.Assert(datedNote, Vocabulary.DatedNoteHasDate, date);
        existing.Assert(date, Vocabulary.Year, new LiteralNode("2001", new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
        existing.Assert(date, Vocabulary.Month, new LiteralNode("--09", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gMonth")));
        existing.Assert(date, Vocabulary.Day, new LiteralNode("---30", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gDay")));
        existing.Assert(id, IngestVocabulary.Description, new LiteralNode("Temporary description to be removed"));
        
        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);
        var ingest = new VariationFileIngest(cache.Object, client.Object, logger);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(0);
        client.Verify(c => c.ApplyDiffAsync(It.IsAny<GraphDiffReport>(), CancellationToken.None), Times.Never);
    }
}
