using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;

namespace Etl.Tests;

[TestClass]
public sealed class DataProcessingTest
{
    private readonly FakeLogger<DataProcessing> logger = new();
#pragma warning disable CS8618
    private IOptions<DriSettings> options;
#pragma warning restore CS8618

    [TestInitialize]
    public void TestInitialize()
    {
        options = Options.Create<DriSettings>(new());
    }

    [TestMethod(DisplayName = "Executes ETLs in order")]
    public async Task RunsInOrder()
    {
        var executionOrder = new List<EtlStageType>();
        var firstEtl = new Mock<IEtl>();
        firstEtl.Setup(e => e.StageType).Returns(EtlStageType.AccessCondition);
        firstEtl.Setup(e => e.RunAsync(It.IsAny<int>(), CancellationToken.None))
            .Callback(() => executionOrder.Add(EtlStageType.AccessCondition));
        var lastEtl = new Mock<IEtl>();
        lastEtl.Setup(e => e.StageType).Returns(EtlStageType.Change);
        lastEtl.Setup(e => e.RunAsync(It.IsAny<int>(), CancellationToken.None))
            .Callback(() => executionOrder.Add(EtlStageType.Change));

        var dataProcessing = new DataProcessing(logger, options, [lastEtl.Object, firstEtl.Object]);
        await dataProcessing.EtlAsync(CancellationToken.None);

        executionOrder.Should().BeEquivalentTo([EtlStageType.AccessCondition, EtlStageType.Change]);
    }

    [TestMethod(DisplayName = "Skips ETL")]
    public async Task Skips()
    {
        var firstEtl = new Mock<IEtl>();
        firstEtl.Setup(e => e.StageType).Returns(EtlStageType.AccessCondition);
        var secondEtl = new Mock<IEtl>();
        secondEtl.Setup(e => e.StageType).Returns(EtlStageType.Legislation);
        secondEtl.Setup(e => e.RunAsync(It.IsAny<int>(), CancellationToken.None));
        options.Value.RestartFromStage = EtlStageType.Legislation;

        var dataProcessing = new DataProcessing(logger, options, [firstEtl.Object, secondEtl.Object]);
        await dataProcessing.EtlAsync(CancellationToken.None);

        firstEtl.Verify(e => e.RunAsync(It.IsAny<int>(), CancellationToken.None), Times.Never);
        secondEtl.Verify(e => e.RunAsync(It.IsAny<int>(), CancellationToken.None), Times.Once);
    }

    [TestMethod(DisplayName = "Restarts with given offset")]
    public async Task Restarts()
    {
        options.Value.RestartFromStage = EtlStageType.AccessCondition;
        options.Value.RestartFromOffset = 2;
        var firstEtl = new Mock<IEtl>();
        firstEtl.Setup(e => e.StageType).Returns(EtlStageType.AccessCondition);
        firstEtl.Setup(e => e.RunAsync(2, CancellationToken.None));
        var secondEtl = new Mock<IEtl>();
        secondEtl.Setup(e => e.StageType).Returns(EtlStageType.Legislation);
        secondEtl.Setup(e => e.RunAsync(0, CancellationToken.None));

        var dataProcessing = new DataProcessing(logger, options, [firstEtl.Object, secondEtl.Object]);
        await dataProcessing.EtlAsync(CancellationToken.None);

        firstEtl.Verify(e => e.RunAsync(2, CancellationToken.None), Times.Once);
        secondEtl.Verify(e => e.RunAsync(0, CancellationToken.None), Times.Once);
    }
}
