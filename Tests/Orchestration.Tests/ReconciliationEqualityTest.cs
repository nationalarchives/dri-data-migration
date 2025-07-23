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
    public void EqualityChecking(Dictionary<ReconciliationFieldName, object?> preservica,
        Dictionary<ReconciliationFieldName, object?> staging,
        List<ReconciliationFieldName> expected,
        string because)
    {
        var difference = ReconciliationEqualityComparer.Check(preservica, staging);

        difference.Should().BeEquivalentTo(expected, because);
    }

    public static IEnumerable<object[]> CheckEqualityData => [
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate }
            },
            new List<ReconciliationFieldName>(),
            "are equal"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate },
                { ReconciliationFieldName.ImportLocation, Guid.NewGuid().ToString() }
            },
            new List<ReconciliationFieldName>(),
            "contains all fields and values are equal"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, differentVariationName },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.VariationName },
            "variation name is different"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.RetentionReviewDate, sameReviewDate }
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.RetentionReviewDate, differentReviewDate }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.RetentionReviewDate },
            "review date is different"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation }
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, differentVariationName },
                { ReconciliationFieldName.LegislationSectionReference, differentLegislation }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.VariationName, ReconciliationFieldName.LegislationSectionReference },
            "variation name and legislation are different"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.VariationName },
            "variation name is missing"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName },
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, null }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.VariationName },
            "variation name is null"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation },
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, new string[] { null } }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.LegislationSectionReference },
            "legislation item is null"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, null },
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, null }
            },
            new List<ReconciliationFieldName>(){ },
            "variation name is null"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, null },
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.VariationName, sameVariationName }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.VariationName },
            "variation name is not null"
        ],
        [
            new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.LegislationSectionReference, new string[]{ null } },
            },
                new Dictionary<ReconciliationFieldName, object?>
            {
                { ReconciliationFieldName.LegislationSectionReference, sameLegislation }
            },
            new List<ReconciliationFieldName>(){ ReconciliationFieldName.LegislationSectionReference },
            "legislation name is not null"
        ]
    ];

}
