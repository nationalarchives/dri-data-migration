using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;

namespace Reconciliation.Tests;

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
            [ReconciliationFieldName.Reference] = "reference",
            [ReconciliationFieldName.FileFolder] = Vocabulary.Subset.Uri,
            [ReconciliationFieldName.ImportLocation] = $"{code}/f1",
            [ReconciliationFieldName.VariationName] = "f1"
        };
        var fakeLogger = new FakeLogger<Comparer>();
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Information, false);
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Warning, true);
        var client = new Mock<IStagingReconciliationClient>();
        var options = Options.Create<ReconciliationSettings>(new()
        {
            Code = code,
            FilePrefix = "file://test location",
            FileLocation = "../../../Csv/metadata-folder.csv",
            MapKind = MapType.Metadata,
            FetchPageSize = 0
        });
        var preservicaLogger = new FakeLogger<PreservicaMetadata>();
        var source = new Mock<PreservicaMetadata>(new object[] { preservicaLogger, options });

        client.Setup(c => c.FetchAsync(code, 0, 0, CancellationToken.None)).ReturnsAsync([data]);

        var reconciliation = new Comparer(fakeLogger, options, client.Object, [source.Object]);
        var summary = await reconciliation.ReconcileAsync(CancellationToken.None);

        summary.Should().BeEquivalentTo(new ReconciliationSummary(0, 0, 0, 0, 0));
    }

    [TestMethod("reconciles metadata with file record")]
    public async Task ReconcilesMetadataFile()
    {
        var code = "code1";
        var data = new Dictionary<ReconciliationFieldName, object>
        {
            [ReconciliationFieldName.Reference] = "reference",
            [ReconciliationFieldName.FileFolder] = Vocabulary.Variation.Uri,
            [ReconciliationFieldName.ImportLocation] = $"{code}/f1/test1.txt",
            [ReconciliationFieldName.VariationName] = "test1.txt"
        };
        var fakeLogger = new FakeLogger<Comparer>();
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Warning, true);
        fakeLogger.ControlLevel(Microsoft.Extensions.Logging.LogLevel.Information, false);
        var client = new Mock<IStagingReconciliationClient>();
        var options = Options.Create<ReconciliationSettings>(new()
        {
            Code = code,
            FilePrefix = "file://test location",
            FileLocation = "../../../Csv/metadata-file.csv",
            MapKind = MapType.Metadata,
            FetchPageSize = 0
        });
        var preservicaLogger = new FakeLogger<PreservicaMetadata>();
        var source = new Mock<PreservicaMetadata>(new object[] { preservicaLogger, options });

        client.Setup(c => c.FetchAsync(code, 0, 0, CancellationToken.None)).ReturnsAsync([data]);

        var reconciliation = new Comparer(fakeLogger, options, client.Object, [source.Object]);
        var summary = await reconciliation.ReconcileAsync(CancellationToken.None);

        summary.Should().BeEquivalentTo(new ReconciliationSummary(0, 0, 0, 0, 0));
    }
}
