using Api;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Etl.Tests;

[TestClass]
public sealed class EtlNotPagedTest
{
    [TestMethod(DisplayName = "ETLs access condition")]
    public async Task AccessCondition()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.Setup(e => e.GetAccessConditionsAsync(CancellationToken.None)).ReturnsAsync([It.IsAny<DriAccessCondition>()]);
        var logger = new FakeLogger<EtlAccessCondition>();
        var ingest = new Mock<IStagingIngest<DriAccessCondition>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriAccessCondition>>(), CancellationToken.None))
            .ReturnsAsync(1);
        var etl = new EtlAccessCondition(logger, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriAccessCondition>>(), CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "ETLs legislation")]
    public async Task Legislation()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.Setup(e => e.GetLegislationsAsync(CancellationToken.None)).ReturnsAsync([It.IsAny<DriLegislation>()]);
        var logger = new FakeLogger<EtlLegislation>();
        var ingest = new Mock<IStagingIngest<DriLegislation>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriLegislation>>(), CancellationToken.None))
            .ReturnsAsync(1);
        var etl = new EtlLegislation(logger, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriLegislation>>(), CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "ETLs grounds for retention")]
    public async Task GroundsForRetention()
    {
        var exporter = new Mock<IDriRdfExporter>();
        exporter.Setup(e => e.GetGroundsForRetentionAsync(CancellationToken.None)).ReturnsAsync([It.IsAny<DriGroundForRetention>()]);
        var logger = new FakeLogger<EtlGroundForRetention>();
        var ingest = new Mock<IStagingIngest<DriGroundForRetention>>();
        ingest.Setup(i => i.SetAsync(It.IsAny<IEnumerable<DriGroundForRetention>>(), CancellationToken.None))
            .ReturnsAsync(1);
        var etl = new EtlGroundForRetention(logger, exporter.Object, ingest.Object);

        await etl.RunAsync(0, CancellationToken.None);

        ingest.Verify(i => i.SetAsync(It.IsAny<IEnumerable<DriGroundForRetention>>(), CancellationToken.None), Times.Once);
    }
}
