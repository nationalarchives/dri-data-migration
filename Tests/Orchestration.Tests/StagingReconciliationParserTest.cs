using Api;
using FluentAssertions;
using Moq;

namespace Orchestration.Tests;

[TestClass]
public sealed class StagingReconciliationParserTest
{
    [TestMethod("Parses staging reconciliation output")]
    [DynamicData(nameof(ParsingData))]
    public async Task Parsing(IEnumerable<Dictionary<ReconciliationFieldName, object>> output,
        IEnumerable<Dictionary<ReconciliationFieldName, object>> expected,
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
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    {  ReconciliationFieldName.FileFolder, Vocabulary.Subset.Uri }
                }
            }
        ]
        ];

    public static IEnumerable<object[]> ParsingData => [
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.FileFolder, Vocabulary.Subset.Uri } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.FileFolder, "folder" } }
            },
            "Subset type translates to folder"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.FileFolder, Vocabulary.Variation.Uri } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.FileFolder, "file" } }
            },
            "Variation type translates to file"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, Vocabulary.Variation.Uri },
                    { ReconciliationFieldName.ImportLocation, $"{code}/{folderName}/{fileName}" }
                }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, "file" },
                    { ReconciliationFieldName.ImportLocation, $"{prefix}/{folderName}/{fileName}" }
                }
            },
            "file location value is translated"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, Vocabulary.Subset.Uri },
                    { ReconciliationFieldName.ImportLocation, $"{code}/{folderName}" }
                }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, "folder" },
                    { ReconciliationFieldName.ImportLocation, $"{prefix}/{folderName}/" }
                }
            },
            "folder location value is translated and ends with forward slash"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, Vocabulary.Subset.Uri },
                    { ReconciliationFieldName.VariationName, $"{code}/{folderName}" }
                }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, "folder" },
                    { ReconciliationFieldName.VariationName, $"{folderName}" }
                }
            },
            "name of the folder is translated"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.FileFolder, Vocabulary.Variation.Uri },
                    { ReconciliationFieldName.VariationName, fileName }
                }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    {  ReconciliationFieldName.FileFolder, "file" },
                    {  ReconciliationFieldName.VariationName, fileName }
                }
            },
            "name of the file remains as is"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.AccessConditionName, "access condition" } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.AccessConditionName, "access_condition" } }
            },
            "replaces whitespaces with underscore in access condition value"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.SensitivityReviewDuration, "P15Y" } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.SensitivityReviewDuration, 15 } }
            },
            "duration value is converted to numer of years"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.SensitivityReviewDuration, "P15Y" },
                    { ReconciliationFieldName.SensitivityReviewEndYear, 2020 }
                }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.SensitivityReviewDuration, 2020 } }
            },
            "end year value takes precedence over duration"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { {  ReconciliationFieldName.LegislationSectionReference, null } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { {  ReconciliationFieldName.LegislationSectionReference, new string[]{ "open" } } }
            },
            "legislation value null becomes open"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.LegislationSectionReference, string.Empty } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.LegislationSectionReference, new string[]{ "open" } } }
            },
            "empty legislation value becomes open"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.LegislationSectionReference, "a,b,c" } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.LegislationSectionReference, new string[]{ "a","b","c" } } }
            },
            "legislation value is transformed into array"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.RetentionType, "retained by department under section 3.4" } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.RetentionType, "retained_under_3.4" } }
            },
            "retention type value (...under 3.4) is transformed into specific value"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.RetentionType, "abc retained def" } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.RetentionType, "abc_retained_def" } }
            },
            "replaces whitespaces with underscore in retention type value when contains text 'retained'"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.RetentionType, "abc def" } }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new() { { ReconciliationFieldName.RetentionType, "abc def" } }
            },
            "retention type value remains as is when does not contain text 'retained'"
        ],
        [
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                { 
                    { ReconciliationFieldName.GroundForRetentionCode, noChange },
                    { ReconciliationFieldName.Id, noChange },
                    { ReconciliationFieldName.IsPublicDescription, noChange },
                    { ReconciliationFieldName.IsPublicName, noChange },
                    { ReconciliationFieldName.RetentionInstrumentNumber, noChange },
                    { ReconciliationFieldName.RetentionInstrumentSignatureDate, noChange },
                    { ReconciliationFieldName.RetentionReviewDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewRestrictionReviewDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewSensitiveDescription, noChange },
                    { ReconciliationFieldName.SensitivityReviewSensitiveName, noChange }
                }
            },
            new List<Dictionary<ReconciliationFieldName, object>>
            {
                new()
                {
                    { ReconciliationFieldName.GroundForRetentionCode, noChange },
                    { ReconciliationFieldName.Id, noChange },
                    { ReconciliationFieldName.IsPublicDescription, noChange },
                    { ReconciliationFieldName.IsPublicName, noChange },
                    { ReconciliationFieldName.RetentionInstrumentNumber, noChange },
                    { ReconciliationFieldName.RetentionInstrumentSignatureDate, noChange },
                    { ReconciliationFieldName.RetentionReviewDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewRestrictionReviewDate, noChange },
                    { ReconciliationFieldName.SensitivityReviewSensitiveDescription, noChange },
                    { ReconciliationFieldName.SensitivityReviewSensitiveName, noChange }
                }
            },
            "these are pass-through values"
        ]
    ];
}
