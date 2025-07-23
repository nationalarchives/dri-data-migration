using Api;
using FluentAssertions;
using Microsoft.VisualBasic.FileIO;

namespace Orchestration.Tests;

[TestClass]
public sealed class PreservicaExportParserTest
{
    [TestMethod("Missing file throws exception")]
    public void MissingFile()
    {
        var parser = new PreservicaExportParser(Guid.NewGuid().ToString(), null);

        parser.Invoking(p => p.Parse().FirstOrDefault()).Should().Throw<FileNotFoundException>();
    }

    [TestMethod("Malformed file throws exception")]
    public void MalformedFile()
    {
        var parser = new PreservicaExportParser("../../../Csv/malformed.csv", null);

        parser.Invoking(p => p.Parse().FirstOrDefault()).Should().Throw<MalformedLineException>();
    }

    
    [TestMethod("Parses CSV")]
    [DynamicData(nameof(CsvParsingData))]
    public void CsvParsing(int rowNumber, Dictionary<ReconciliationFieldName, object> expected)
    {
        var parser = new PreservicaExportParser("../../../Csv/input.csv", csvParsingMap);
        var row = parser.Parse().ElementAt(rowNumber);

        row.Should().BeEquivalentTo(expected, "values are equal");
    }

    private readonly Dictionary<string, ReconciliationRow> csvParsingMap = new()
        {
            { "text", new (ReconciliationFieldName.VariationName, PreservicaExportParser.ToText) },
            { "location", new(ReconciliationFieldName.ImportLocation, PreservicaExportParser.ToLocation) },
            { "textlist", new(ReconciliationFieldName.LegislationSectionReference, PreservicaExportParser.ToTextList) },
            { "date", new(ReconciliationFieldName.RetentionReviewDate, PreservicaExportParser.ToDate) },
            { "int", new(ReconciliationFieldName.RetentionInstrumentNumber, PreservicaExportParser.ToInt) },
            { "bool", new(ReconciliationFieldName.IsPublicDescription, PreservicaExportParser.ToBool) }
        };

    public static IEnumerable<object[]> CsvParsingData => [
        [
            0,
            new Dictionary<ReconciliationFieldName, object>
            {
                { ReconciliationFieldName.VariationName, "Text1" },
                { ReconciliationFieldName.ImportLocation, "file://test location 1" },
                { ReconciliationFieldName.LegislationSectionReference, new string[] { "Item1", "Item2" } },
                { ReconciliationFieldName.RetentionReviewDate, DateTimeOffset.Parse("2000-01-01") },
                { ReconciliationFieldName.RetentionInstrumentNumber, 10 },
                { ReconciliationFieldName.IsPublicDescription, false }
            }
        ],
        [
            1,
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, "Text2" },
                { ReconciliationFieldName.ImportLocation, "file://test-location-2" },
                { ReconciliationFieldName.LegislationSectionReference, new string[] { "Item3" } },
                { ReconciliationFieldName.RetentionReviewDate, DateTimeOffset.Parse("2000-01-01") },
                { ReconciliationFieldName.IsPublicDescription, true }
            }
        ],
        [
            2,
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, "Text3" },
                { ReconciliationFieldName.RetentionReviewDate, DateTimeOffset.Parse("2000-01-01+06:00") },
                { ReconciliationFieldName.RetentionInstrumentNumber, 0 }
            }
        ]
    ];
}
