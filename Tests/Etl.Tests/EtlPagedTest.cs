using Api;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;

namespace Etl.Tests;

[TestClass]
public sealed class EtlPagedTest
{
    private readonly IOptions<DriSettings> options = Options.Create<DriSettings>(new()
    {
        FetchPageSize = 1
    });

    [TestMethod(DisplayName = "ETLs subset")]
    public async Task Subset()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.SetupSequence(e => e.GetSubsetsAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync([It.IsAny<DriSubset>()])
            .ReturnsAsync([It.IsAny<DriSubset>()])
            .ReturnsAsync([]);
        var logger = new FakeLogger<EtlSubset>();
        var ingest = new Mock<IStagingIngest<DriSubset>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriSubset>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlSubset(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriSubset>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs asset")]
    public async Task Asset()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.SetupSequence(e => e.GetAssetsAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync([It.IsAny<DriAsset>()])
            .ReturnsAsync([It.IsAny<DriAsset>()])
            .ReturnsAsync([]);
        var logger = new FakeLogger<EtlAsset>();
        var ingest = new Mock<IStagingIngest<DriAsset>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriAsset>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlAsset(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriAsset>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs variation")]
    public async Task Variation()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.SetupSequence(e => e.GetVariationsAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync([It.IsAny<DriVariation>()])
            .ReturnsAsync([It.IsAny<DriVariation>()])
            .ReturnsAsync([]);
        var logger = new FakeLogger<EtlVariation>();
        var ingest = new Mock<IStagingIngest<DriVariation>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriVariation>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlVariation(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriVariation>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs asset deliverable unit")]
    public async Task AssetDeliverableUnit()
    {
        var exporter = new Mock<IDriSqlExporter>();
        exporter.SetupSequence(e => e.GetAssetDeliverableUnits(It.IsAny<int>(), CancellationToken.None))
            .Returns([It.IsAny<DriAssetDeliverableUnit>()])
            .Returns([It.IsAny<DriAssetDeliverableUnit>()])
            .Returns([]);
        var logger = new FakeLogger<EtlAssetDeliverableUnit>();
        var ingest = new Mock<IStagingIngest<DriAssetDeliverableUnit>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriAssetDeliverableUnit>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlAssetDeliverableUnit(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriAssetDeliverableUnit>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs WO 409 subset deliverable unit")]
    public async Task Wo409SubsetDeliverableUnit()
    {
        var exporter = new Mock<IDriSqlExporter>();
        exporter.SetupSequence(e => e.GetWo409SubsetDeliverableUnits(It.IsAny<int>(), CancellationToken.None))
            .Returns([It.IsAny<DriWo409SubsetDeliverableUnit>()])
            .Returns([It.IsAny<DriWo409SubsetDeliverableUnit>()])
            .Returns([]);
        var logger = new FakeLogger<EtlWo409SubsetDeliverableUnit>();
        var ingest = new Mock<IStagingIngest<DriWo409SubsetDeliverableUnit>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriWo409SubsetDeliverableUnit>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlWo409SubsetDeliverableUnit(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriWo409SubsetDeliverableUnit>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs variation file")]
    public async Task VariationFile()
    {
        var exporter = new Mock<IDriSqlExporter>();
        exporter.SetupSequence(e => e.GetVariationFiles(It.IsAny<int>(), CancellationToken.None))
            .Returns([It.IsAny<DriVariationFile>()])
            .Returns([It.IsAny<DriVariationFile>()])
            .Returns([]);
        var logger = new FakeLogger<EtlVariationFile>();
        var ingest = new Mock<IStagingIngest<DriVariationFile>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriVariationFile>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlVariationFile(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriVariationFile>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs sensitivity review")]
    public async Task SensitivityReview()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.SetupSequence(e => e.GetSensitivityReviewsAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync([It.IsAny<DriSensitivityReview>()])
            .ReturnsAsync([It.IsAny<DriSensitivityReview>()])
            .ReturnsAsync([]);
        var logger = new FakeLogger<EtlSensitivityReview>();
        var ingest = new Mock<IStagingIngest<DriSensitivityReview>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriSensitivityReview>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlSensitivityReview(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriSensitivityReview>>(), CancellationToken.None), Times.Exactly(3));
    }

    [TestMethod(DisplayName = "ETLs change")]
    public async Task Change()
    {
        var exporter = new Mock<IDriSqlExporter>();
        exporter.SetupSequence(e => e.GetChanges(It.IsAny<int>(), CancellationToken.None))
            .Returns([It.IsAny<DriChange>()])
            .Returns([It.IsAny<DriChange>()])
            .Returns([]);
        var logger = new FakeLogger<EtlChange>();
        var ingest = new Mock<IStagingIngest<DriChange>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriChange>>(), CancellationToken.None))
            .ReturnsAsync(1);

        var etl = new EtlChange(logger, options, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriChange>>(), CancellationToken.None), Times.Exactly(3));
    }
}
