using Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Diagnostics.Metrics;
using VDS.RDF;

namespace Staging.Tests;

[TestClass]
public sealed class Adm158SubsetDeliverableUnitIngestTest
{
    private const string xml = """
        <DeliverableUnit xmlns="http://www.tessella.com/XIP/v4" status="same">
        	<Metadata schemaURI="http://nationalarchives.gov.uk/metadata/tna#">
        		<tna:metadata xmlns:tna="http://nationalarchives.gov.uk/metadata/tna#">
        			<rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
        				<tna:DigitalFolder rdf:about="http://datagov.nationalarchives.gov.uk/66/ADM/158/106/1/3">
        					<tna:cataloguing>
        						<tna:Cataloguing>
        							<dcterms:coverage xmlns:dcterms="http://purl.org/dc/terms/">
        								<tna:CoveringDates>
        									<tna:fullDate>[1800-1899]</tna:fullDate>
        									<tna:startDate>1800</tna:startDate>
        									<tna:endDate>1899</tna:endDate>
        								</tna:CoveringDates>
        							</dcterms:coverage>
        							<tna:legalStatus rdf:resource="http://datagov.nationalarchives.gov.uk/resource/Public_Record"/>
        							<tna:heldBy rdf:datatype="xs:string">The National Archives, Kew</tna:heldBy>
        						</tna:Cataloguing>
        					</tna:cataloguing>
        					<tna:transcription>
        						<tnatrans:Transcription xmlns:tnatrans="http://nationalarchives.gov.uk/dri/transcription">
        							<tnatrans:surname rdf:datatype="xs:string">Smith</tnatrans:surname>
        							<tnatrans:surnameOther rdf:datatype="xs:string">Smith2</tnatrans:surnameOther>
        							<tnatrans:forenames rdf:datatype="xs:string">Bob</tnatrans:forenames>
        							<tnatrans:forenamesOther rdf:datatype="xs:string">*</tnatrans:forenamesOther>
        							<tnatrans:ageYears rdf:datatype="xs:string">18</tnatrans:ageYears>
        							<tnatrans:ageMonths rdf:datatype="xs:string">1</tnatrans:ageMonths>
        							<tnatrans:placeOfBirthParish rdf:datatype="xs:string">Parish1</tnatrans:placeOfBirthParish>
        							<tnatrans:placeOfBirthTown rdf:datatype="xs:string">*</tnatrans:placeOfBirthTown>
        							<tnatrans:placeOfBirthCounty rdf:datatype="xs:string">County1</tnatrans:placeOfBirthCounty>
        							<tnatrans:placeOfBirthCountry rdf:datatype="xs:string">*</tnatrans:placeOfBirthCountry>
        							<tnatrans:divisionDescription rdf:datatype="xs:string">Division1</tnatrans:divisionDescription>
        						</tnatrans:Transcription>
        					</tna:transcription>
        				</tna:DigitalFolder>
        			</rdf:RDF>
        		</tna:metadata>
        	</Metadata>
        </DeliverableUnit>
        """;
    private readonly DriAdm158SubsetDeliverableUnit dri = new("ADM/158/1/2/3", xml);
    private readonly FakeLogger<Adm158SubsetDeliverableUnitIngest> logger = new();
    private readonly Mock<ISparqlClient> client = new();
    private readonly Mock<ICacheClient> cache;
    private readonly IUriNode address = CacheClient.NewId;
    private readonly IUriNode membership = CacheClient.NewId;
    private readonly IMeterFactory meterFactory;

    public Adm158SubsetDeliverableUnitIngestTest()
    {
        cache = new();
        cache.Setup(c => c.CacheFetchOrNew(CacheEntityKind.GeographicalPlace, It.IsAny<string>(),
            Vocabulary.GeographicalPlaceName, CancellationToken.None)).ReturnsAsync(address);
        cache.Setup(c => c.CacheFetchOrNew(CacheEntityKind.NavyDivision, It.IsAny<string>(),
            Vocabulary.NavyDivisionName, CancellationToken.None)).ReturnsAsync(membership);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        meterFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IMeterFactory>();
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
        existing.Assert(id, Vocabulary.SubsetReference, new LiteralNode(dri.Id));

        client.Setup(c => c.GetGraphAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), CancellationToken.None))
            .ReturnsAsync(existing);

        var ingest = new Adm158SubsetDeliverableUnitIngest(cache.Object, client.Object, logger, meterFactory);

        var recordIngestedCount = await ingest.SetAsync([dri], CancellationToken.None);

        recordIngestedCount.Should().Be(1);
        client.Verify(c => c.ApplyDiffAsync(
            It.Is<GraphDiffReport>(r => r.AddedTriples.Count() == 11 && !r.RemovedTriples.Any()),
            CancellationToken.None), Times.Once);
    }
}
