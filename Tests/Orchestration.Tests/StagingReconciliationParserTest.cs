using Api;
using FluentAssertions;
using Moq;

namespace Orchestration.Tests;

[TestClass]
public sealed class StagingReconciliationParserTest
{
    [TestMethod("Parses staging reconciliation output")]
    [DynamicData(nameof(ParsingData))]
    public async Task Parsing(IEnumerable<Dictionary<ReconciliationFieldNames, object>> output,
        IEnumerable<Dictionary<ReconciliationFieldNames, object>> expected,
        string because)
    {
        var client = new Mock<IStagingReconciliationClient>();
        client.Setup(c => c.FetchAsync(It.Is<string>(s => s.Equals(code)), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(output);

        var parser = new StagingReconciliationParser(client.Object, code, prefix);
        var result = await parser.ParseAsync();

        result.Should().BeEquivalentTo(expected, because);
    }

    private static string code = "CODE 1";
    private static string prefix = $"file://{code}/content";
    private static string folderName = "Folder1";
    private static string fileName = "File1.txt";
    private static string noChange = "Value does not change";

    public static IEnumerable<object[]> PaxxrsingData => [
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    {  ReconciliationFieldNames.FileFolder, Vocabulary.Subset.Uri }
                }
            }
        ]
        ];

    public static IEnumerable<object[]> ParsingData => [
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.FileFolder, Vocabulary.Subset.Uri } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.FileFolder, "folder" } }
            },
            "Subset type translates to folder"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.FileFolder, Vocabulary.Variation.Uri } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.FileFolder, "file" } }
            },
            "Variation type translates to file"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, Vocabulary.Variation.Uri },
                    { ReconciliationFieldNames.ImportLocation, $"{code}/{folderName}/{fileName}" }
                }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, "file" },
                    { ReconciliationFieldNames.ImportLocation, $"{prefix}/{folderName}/{fileName}" }
                }
            },
            "file location value is translated"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, Vocabulary.Subset.Uri },
                    { ReconciliationFieldNames.ImportLocation, $"{code}/{folderName}" }
                }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, "folder" },
                    { ReconciliationFieldNames.ImportLocation, $"{prefix}/{folderName}/" }
                }
            },
            "folder location value is translated and ends with forward slash"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, Vocabulary.Subset.Uri },
                    { ReconciliationFieldNames.VariationName, $"{code}/{folderName}" }
                }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, "folder" },
                    { ReconciliationFieldNames.VariationName, $"{folderName}" }
                }
            },
            "name of the folder is translated"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.FileFolder, Vocabulary.Variation.Uri },
                    { ReconciliationFieldNames.VariationName, fileName }
                }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    {  ReconciliationFieldNames.FileFolder, "file" },
                    {  ReconciliationFieldNames.VariationName, fileName }
                }
            },
            "name of the file remains as is"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.AccessConditionName, "access condition" } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.AccessConditionName, "access_condition" } }
            },
            "replaces whitespaces with underscore in access condition value"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.SensitivityReviewDuration, "P15Y" } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.SensitivityReviewDuration, 15 } }
            },
            "duration value is converted to numer of years"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.SensitivityReviewDuration, "P15Y" },
                    { ReconciliationFieldNames.SensitivityReviewEndYear, 2020 }
                }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.SensitivityReviewDuration, 2020 } }
            },
            "end year value takes precedence over duration"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { {  ReconciliationFieldNames.LegislationSectionReference, null } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { {  ReconciliationFieldNames.LegislationSectionReference, new string[]{ "open" } } }
            },
            "legislation value null becomes open"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.LegislationSectionReference, string.Empty } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.LegislationSectionReference, new string[]{ "open" } } }
            },
            "empty legislation value becomes open"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.LegislationSectionReference, "a,b,c" } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.LegislationSectionReference, new string[]{ "a","b","c" } } }
            },
            "legislation value is transformed into array"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.RetentionType, "retained by department under section 3.4" } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.RetentionType, "retained_under_3.4" } }
            },
            "retention type value (...under 3.4) is transformed into specific value"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.RetentionType, "abc retained def" } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.RetentionType, "abc_retained_def" } }
            },
            "replaces whitespaces with underscore in retention type value when contains text 'retained'"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.RetentionType, "abc def" } }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new() { { ReconciliationFieldNames.RetentionType, "abc def" } }
            },
            "retention type value remains as is when does not contain text 'retained'"
        ],
        [
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                { 
                    { ReconciliationFieldNames.GroundForRetentionCode, noChange },
                    { ReconciliationFieldNames.Id, noChange },
                    { ReconciliationFieldNames.IsPublicDescription, noChange },
                    { ReconciliationFieldNames.IsPublicName, noChange },
                    { ReconciliationFieldNames.RetentionInstrumentNumber, noChange },
                    { ReconciliationFieldNames.RetentionInstrumentSignedDate, noChange },
                    { ReconciliationFieldNames.RetentionReviewDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewRestrictionCalculationStartDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewRestrictionReviewDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewSensitiveDescription, noChange },
                    { ReconciliationFieldNames.SensitivityReviewSensitiveName, noChange }
                }
            },
            new List<Dictionary<ReconciliationFieldNames, object>>
            {
                new()
                {
                    { ReconciliationFieldNames.GroundForRetentionCode, noChange },
                    { ReconciliationFieldNames.Id, noChange },
                    { ReconciliationFieldNames.IsPublicDescription, noChange },
                    { ReconciliationFieldNames.IsPublicName, noChange },
                    { ReconciliationFieldNames.RetentionInstrumentNumber, noChange },
                    { ReconciliationFieldNames.RetentionInstrumentSignedDate, noChange },
                    { ReconciliationFieldNames.RetentionReviewDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewRestrictionCalculationStartDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewRestrictionReviewDate, noChange },
                    { ReconciliationFieldNames.SensitivityReviewSensitiveDescription, noChange },
                    { ReconciliationFieldNames.SensitivityReviewSensitiveName, noChange }
                }
            },
            "these are pass-through values"
        ]
    ];
}
