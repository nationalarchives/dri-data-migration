using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;

namespace Orchestration.Tests;

[TestClass]
public sealed class ReconciliationTest
{
    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("reconciles metadata with folder record")]
    public async Task ReconcilesMetadataFolder()
    {
        var code = "code1";
        var data = new Dictionary<ReconciliationFieldName, object>
        {
            [ReconciliationFieldName.FileFolder] = Vocabulary.Subset.Uri,
            [ReconciliationFieldName.ImportLocation] = $"{code}/f1",
            [ReconciliationFieldName.VariationName] = "f1"
        };
        var fakeLogger = new FakeLogger<Reconciliation>();
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Information, false);
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Warning, true);
        var client = new Mock<IStagingReconciliationClient>();
        var options = Options.Create<StagingSettings>(new());

        client.Setup(c => c.FetchAsync(code, 0, 0)).ReturnsAsync([data]);

        var reconciliation = new Reconciliation(fakeLogger, client.Object, options);
        await reconciliation.ReconcileAsync(code, "file://test location", "../../../Csv/metadata-folder.csv", PreservicaExportMap.MapType.Metadata);

        fakeLogger.Collector.Count.Should().Be(0);
    }

    [TestMethod("reconciles metadata with file record")]
    public async Task ReconcilesMetadataFile()
    {
        var code = "code1";
        var data = new Dictionary<ReconciliationFieldName, object>
        {
            [ReconciliationFieldName.FileFolder] = Vocabulary.Variation.Uri,
            [ReconciliationFieldName.ImportLocation] = $"{code}/f1/test1.txt",
            [ReconciliationFieldName.VariationName] = "test1.txt"
        };
        var fakeLogger = new FakeLogger<Reconciliation>();
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Warning, true);
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Information, false);
        var client = new Mock<IStagingReconciliationClient>();
        var options = Options.Create<StagingSettings>(new());

        client.Setup(c => c.FetchAsync(code, 0, 0)).ReturnsAsync([data]);

        var reconciliation = new Reconciliation(fakeLogger, client.Object, options);
        await reconciliation.ReconcileAsync(code, "file://test location", "../../../Csv/metadata-file.csv", PreservicaExportMap.MapType.Metadata);

        fakeLogger.Collector.Count.Should().Be(0);
    }
}
