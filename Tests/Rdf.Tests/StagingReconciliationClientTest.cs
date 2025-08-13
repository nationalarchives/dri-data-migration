using Api;
using FluentAssertions;
using Moq;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Rdf.Tests;

[TestClass]
public class StagingReconciliationClientTest
{
    private Mock<IReconciliationSparqlClient> sparqlClient;

    [TestInitialize]
    public void TestInitialize()
    {
        sparqlClient = new Mock<IReconciliationSparqlClient>();
    }

    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("Reads")]
    [DynamicData(nameof(ReadsData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task Reads(SparqlResultSet data,
        Dictionary<ReconciliationFieldName, object> expected, string _)
    {
        sparqlClient.Setup(c => c.GetResultSetAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(data);

        var client = new StagingReconciliationClient(sparqlClient.Object);
        var result = await client.FetchAsync("ignore", 0, 0, CancellationToken.None);

        result.Should().ContainSingle().And.BeEquivalentTo([expected]);
    }

    private static readonly Dictionary<ReconciliationFieldName, object> allFields = new()
    {
            { ReconciliationFieldName.Id, new Uri("http://example.com/s") },
            { ReconciliationFieldName.FileFolder, new Uri("http://example.com/fileOrFolder") },
            { ReconciliationFieldName.ImportLocation, "Directory" },
            { ReconciliationFieldName.VariationName, "Variation name" },
            { ReconciliationFieldName.AccessConditionName, "Access condition name" },
            { ReconciliationFieldName.RetentionType, "Access condition name" },
            { ReconciliationFieldName.SensitivityReviewDate, DateTimeOffset.UtcNow.AddDays(-1) },
            { ReconciliationFieldName.SensitivityReviewSensitiveName, "Sensitive name" },
            { ReconciliationFieldName.IsPublicName, false },
            { ReconciliationFieldName.IsPublicDescription, false },
            { ReconciliationFieldName.SensitivityReviewRestrictionReviewDate, DateTimeOffset.UtcNow.AddDays(-2) },
            { ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, DateTimeOffset.UtcNow.AddDays(-3) },
            { ReconciliationFieldName.SensitivityReviewEndYear, 2020 },
            { ReconciliationFieldName.LegislationSectionReference, "Legislation section" },
            { ReconciliationFieldName.RetentionReviewDate, DateTimeOffset.UtcNow.AddDays(-4) },
            { ReconciliationFieldName.RetentionInstrumentNumber, 2 },
            { ReconciliationFieldName.RetentionInstrumentSignatureDate, DateTimeOffset.UtcNow.AddDays(-5) },
            { ReconciliationFieldName.GroundForRetentionCode, "Ground for retention code" }
        };

    private static readonly Dictionary<ReconciliationFieldName, object> minFields = new()
    {
        { ReconciliationFieldName.Id, new Uri("http://example.com/s") },
        { ReconciliationFieldName.FileFolder, new Uri("http://example.com/fileOrFolder") },
        { ReconciliationFieldName.ImportLocation, "Directory" },
        { ReconciliationFieldName.IsPublicName, false },
        { ReconciliationFieldName.IsPublicDescription, false }
    };

    public static IEnumerable<object[]> ReadsData => [
        [ Build(allFields), allFields, "when all values are returned" ],
        [ Build(minFields), minFields, "when only required values are returned" ],
    ];

    private static SparqlResultSet Build(Dictionary<ReconciliationFieldName, object> dir)
    {
        var sparqlResult = new SparqlResult([
            new("s", new UriNode(dir[ReconciliationFieldName.Id] as Uri)),
            new("t", new UriNode(dir[ReconciliationFieldName.FileFolder] as Uri)),
            new(Vocabulary.ImportLocation.Uri.Segments.Last(), new LiteralNode(dir[ReconciliationFieldName.ImportLocation] as string)),
            new("reference", dir.TryGetValue(ReconciliationFieldName.Reference, out var v) && v is not null ? new LiteralNode(v as string) : null),
            new(Vocabulary.VariationName.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.VariationName, out var v0) && v0 is not null ? new LiteralNode(v0 as string) : null),
            new(Vocabulary.VariationDriId.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.DriId, out var v00) && v00 is not null ? new LiteralNode(v00 as string) : null),
            new(Vocabulary.AccessConditionCode.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.AccessConditionCode, out var v000) && v is not null ? new LiteralNode(v000 as string) : null),
            new(Vocabulary.AccessConditionName.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.AccessConditionName, out var v2) && v2 is not null ? new LiteralNode(v2 as string) : null),
            new("retentionType", dir.TryGetValue(ReconciliationFieldName.RetentionType, out var v3) && v3 is not null ? new LiteralNode(v3 as string) : null),
            new(Vocabulary.SensitivityReviewDate.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.SensitivityReviewDate, out var v4) && v4 is not null ? new DateNode((DateTimeOffset)v4) : null),
            new(Vocabulary.SensitivityReviewSensitiveName.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.SensitivityReviewSensitiveName, out var v5) && v5 is not null ? new LiteralNode(v5 as string) : null),
            new("isPublicName", dir.TryGetValue(ReconciliationFieldName.IsPublicName, out var v6) && v6 is not null && v6 is bool b ? new BooleanNode(b) : new BooleanNode(true)),
            new("isPublicDescription", dir.TryGetValue(ReconciliationFieldName.IsPublicDescription, out var isPd) && isPd is not null && isPd is bool isPdB ? new BooleanNode(isPdB) : new BooleanNode(true)),
            new(Vocabulary.SensitivityReviewRestrictionReviewDate.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.SensitivityReviewRestrictionReviewDate, out var v9) && v9 is not null ? new DateNode((DateTimeOffset)v9) : null),
            new(Vocabulary.SensitivityReviewRestrictionCalculationStartDate.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, out var v10) && v10 is not null ? new DateNode((DateTimeOffset)v10) : null),
            new(Vocabulary.SensitivityReviewRestrictionDuration.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.SensitivityReviewDuration, out var v11) && v11 is not null && v11 is TimeSpan ts ? new LiteralNode((ts.TotalDays / 365).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDuration)) : null),
            new(Vocabulary.SensitivityReviewRestrictionEndYear.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.SensitivityReviewEndYear, out var v12) && v12 is not null ? new LongNode((int)v12) : null),
            new(Vocabulary.LegislationSectionReference.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.LegislationSectionReference, out var v13) && v13 is not null ? new LiteralNode(v13 as string) : null),
            new(Vocabulary.RetentionRestrictionReviewDate.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.RetentionReviewDate, out var v14) && v14 is not null ? new DateNode((DateTimeOffset)v14) : null),
            new(Vocabulary.RetentionInstrumentNumber.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.RetentionInstrumentNumber, out var v15) && v15 is not null ? new LongNode((int)v15) : null),
            new(Vocabulary.RetentionInstrumentSignatureDate.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.RetentionInstrumentSignatureDate, out var v16) && v16 is not null ? new DateNode((DateTimeOffset)v16) : null),
            new(Vocabulary.GroundForRetentionCode.Uri.Segments.Last(), dir.TryGetValue(ReconciliationFieldName.GroundForRetentionCode, out var v17) && v17 is not null ? new LiteralNode(v17 as string) : null)
        ]);

        return new SparqlResultSet([sparqlResult]);
    }
}
