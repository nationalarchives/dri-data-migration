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
    public void CsvParsing(int rowNumber, Dictionary<ReconciliationFieldNames, object> expected)
    {
        var parser = new PreservicaExportParser("../../../Csv/input.csv", csvParsingMap);
        var row = parser.Parse().ElementAt(rowNumber);

        row.Should().BeEquivalentTo(expected, "values are equal");
    }

    private readonly Dictionary<string, ReconciliationRow> csvParsingMap = new()
        {
            { "text", new (ReconciliationFieldNames.VariationName, PreservicaExportParser.ToText) },
            { "location", new(ReconciliationFieldNames.ImportLocation, PreservicaExportParser.ToLocation) },
            { "textlist", new(ReconciliationFieldNames.LegislationSectionReference, PreservicaExportParser.ToTextList) },
            { "date", new(ReconciliationFieldNames.RetentionReviewDate, PreservicaExportParser.ToDate) },
            { "int", new(ReconciliationFieldNames.RetentionInstrumentNumber, PreservicaExportParser.ToInt) },
            { "bool", new(ReconciliationFieldNames.IsPublicDescription, PreservicaExportParser.ToBool) }
        };

    public static IEnumerable<object[]> CsvParsingData => [
        [
            0,
            new Dictionary<ReconciliationFieldNames, object>
            {
                { ReconciliationFieldNames.VariationName, "Text1" },
                { ReconciliationFieldNames.ImportLocation, "file://test location 1" },
                { ReconciliationFieldNames.LegislationSectionReference, new string[] { "Item1", "Item2" } },
                { ReconciliationFieldNames.RetentionReviewDate, DateTimeOffset.Parse("2000-01-01") },
                { ReconciliationFieldNames.RetentionInstrumentNumber, 10 },
                { ReconciliationFieldNames.IsPublicDescription, false }
            }
        ],
        [
            1,
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, "Text2" },
                { ReconciliationFieldNames.ImportLocation, "file://test-location-2" },
                { ReconciliationFieldNames.LegislationSectionReference, new string[] { "Item3" } },
                { ReconciliationFieldNames.RetentionReviewDate, DateTimeOffset.Parse("2000-01-01") },
                { ReconciliationFieldNames.IsPublicDescription, true }
            }
        ],
        [
            2,
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, "Text3" },
                { ReconciliationFieldNames.RetentionReviewDate, DateTimeOffset.Parse("2000-01-01+06:00") },
                { ReconciliationFieldNames.RetentionInstrumentNumber, 0 }
            }
        ]
    ];
}
