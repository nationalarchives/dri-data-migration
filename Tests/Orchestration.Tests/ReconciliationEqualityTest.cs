using Api;
using FluentAssertions;

namespace Orchestration.Tests;

[TestClass]
public sealed class ReconciliationEqualityComparerTest
{
    private readonly static string sameVariationName = "VariationName 1";
    private readonly static string differentVariationName = "VariationName 2";
    private readonly static string[] sameLegislation = ["Legislation 1", "Legislation 2"];
    private readonly static string[] differentLegislation = ["Legislation 1", "Legislation 2", "Legislation 3"];
    private readonly static DateTimeOffset sameReviewDate = DateTimeOffset.Parse("2000-01-01");
    private readonly static DateTimeOffset differentReviewDate = DateTimeOffset.UtcNow;

    [TestMethod("Difference logic")]
    [DynamicData(nameof(CheckEqualityData))]
    public void EqualityChecking(Dictionary<ReconciliationFieldNames, object?> preservica,
        Dictionary<ReconciliationFieldNames, object?> staging,
        List<ReconciliationFieldNames> expected,
        string because)
    {
        var difference = ReconciliationEqualityComparer.Check(preservica, staging);

        difference.Should().BeEquivalentTo(expected, because);
    }

    public static IEnumerable<object[]> CheckEqualityData => [
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate }
            },
            new List<ReconciliationFieldNames>(),
            "are equal"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate },
                { ReconciliationFieldNames.ImportLocation, Guid.NewGuid().ToString() }
            },
            new List<ReconciliationFieldNames>(),
            "contains all fields and values are equal"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, differentVariationName },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.VariationName },
            "variation name is different"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.RetentionReviewDate, differentReviewDate }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.RetentionReviewDate },
            "review date is different"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation }
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, differentVariationName },
                { ReconciliationFieldNames.LegislationSectionReference, differentLegislation }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.VariationName, ReconciliationFieldNames.LegislationSectionReference },
            "variation name and legislation are different"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.VariationName },
            "variation name is missing"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName },
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, null }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.VariationName },
            "variation name is null"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation },
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, new string[] { null } }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.LegislationSectionReference },
            "legislation item is null"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, null },
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, null }
            },
            new List<ReconciliationFieldNames>(){ },
            "variation name is null"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, null },
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.VariationName, sameVariationName }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.VariationName },
            "variation name is not null"
        ],
        [
            new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.LegislationSectionReference, new string[]{ null } },
            },
                new Dictionary<ReconciliationFieldNames, object?>
            {
                { ReconciliationFieldNames.LegislationSectionReference, sameLegislation }
            },
            new List<ReconciliationFieldNames>(){ ReconciliationFieldNames.LegislationSectionReference },
            "legislation name is not null"
        ]
    ];

}
