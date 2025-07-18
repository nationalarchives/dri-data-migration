using Api;
using FluentAssertions;
using System.Net.Http.Headers;

namespace Rdf.Tests;

[TestClass]
public class DriExportSensitivityReviewTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads sensitivity reviews")]
    [DynamicData(nameof(ReadsSensitivityReviewsData))]
    public async Task ReadsSensitivityReviews(HttpResponseMessage message, IEnumerable<DriSensitivityReview> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var srs = await exporter.GetSensitivityReviewsByCode("ignore", 0, 0);

        srs.Should().BeEquivalentTo(expected, because);
    }

    private record TestSr(string Type, string Reference, DriSensitivityReview Sr);
    
    private static DateTimeOffset GenerateDate(int day, int hour, int span) => new DateTimeOffset(2000, 1, day, hour, 0, 0, TimeSpan.FromHours(span));

    private static readonly TestSr minimalSr = new("http://nationalarchives.gov.uk/terms/dri#File", "ignore",
        new("http://example.com/sr1", null, new Uri("http://example.com/f1"), "ac1a", []));
    private static readonly TestSr multipleLegislations = new("http://nationalarchives.gov.uk/terms/dri#File", "ignore",
        new("http://example.com/sr2", null, new Uri("http://example.com/f2"), "ac2b", [new Uri("http://example.com/l1"), new Uri("http://example.com/l2")]));
    private static readonly TestSr assetOrSubset = new("http://nationalarchives.gov.uk/terms/dri#DeliverableUnit", "Asset or Subset reference 1",
        new("http://example.com/sr3", "Asset or Subset reference 1", new Uri("http://example.com/f3"), "ac3c", []));
    private static readonly TestSr allFieldsSr = new("http://nationalarchives.gov.uk/terms/dri#DeliverableUnit", "Asset or Subset reference 2",
        new("http://example.com/sr4", "Asset or Subset reference 2", new Uri("http://example.com/f4"), "ac4d", [new Uri("http://example.com/l3")],
        GenerateDate(1,1,0), "http://example.com/past-sr4", "sensitive name", "sensitive description", GenerateDate(2, 2, 1),
        GenerateDate(3, 3, 2), 1, "description", 2, GenerateDate(4, 4, 3), GenerateDate(5, 5, 4), "g1"));

    public static IEnumerable<object[]> ReadsSensitivityReviewsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([minimalSr]), MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriSensitivityReview>{ minimalSr.Sr },
            "has minimal matching sensitivity review"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([multipleLegislations]), MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriSensitivityReview>{ multipleLegislations.Sr },
            "has multiple legislations"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([assetOrSubset]), MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriSensitivityReview>{ assetOrSubset.Sr },
            "is subset or reference"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([allFieldsSr]), MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriSensitivityReview>{ allFieldsSr.Sr },
            "parses all fields"
        ]
    ];

    private static string BuildResponse(TestSr[] srs) =>
        string.Concat("@prefix ex: <http://example.com/schema/> .\r\n@prefix xsd:<http://www.w3.org/2001/XMLSchema#>.\r\n",
            string.Join("\r\n",
                srs.Select(s => $"""
                            <{s.Sr.Id}> ex:x-type <{s.Type}>;
                                ex:x-id <{s.Sr.TargetId}>;
                                ex:x-reference  "{s.Reference}";
                                ex:sensitivityReviewDriId <{s.Sr.Id}>;
                                {ConditionalFormat("sensitivityReviewDate", s.Sr.Date)}
                                {ConditionalFormat("sensitivityReviewSensitiveName", s.Sr.SensitiveName)}
                                {ConditionalFormat("sensitivityReviewSensitiveDescription", s.Sr.SensitiveDescription)}
                                {ConditionalFormat("sensitivityReviewHasPastSensitivityReview", s.Sr.PreviousId is null ? null : new Uri(s.Sr.PreviousId))}
                                ex:sensitivityReviewHasSensitivityReviewRestriction [
                                    {ConditionalFormat("sensitivityReviewRestrictionReviewDate", s.Sr.ReviewDate)}
                                    {ConditionalFormat("sensitivityReviewRestrictionCalculationStartDate", s.Sr.RestrictionStartDate)}
                                    {ConditionalFormat("sensitivityReviewRestrictionDuration", s.Sr.RestrictionDuration)}
                                    {ConditionalFormat("sensitivityReviewRestrictionDescription", s.Sr.RestrictionDescription)}
                                    ex:sensitivityReviewRestrictionHasRetentionRestriction [
                                        {ConditionalFormat("retentionInstrumentNumber", s.Sr.InstrumentNumber)}
                                        {ConditionalFormat("retentionInstrumentSignedDate", s.Sr.InstrumentSignedDate)}
                                        {ConditionalFormat("retentionRestrictionReviewDate", s.Sr.RestrictionReviewDate)}
                                        ex:retentionRestrictionHasGroundForRetention [
                                            {ConditionalFormat("groundForRetentionCode", s.Sr.GroundForRetentionCode is null ? null : new Uri($"http://example.com/ground#{s.Sr.GroundForRetentionCode}"))}
                                        ]
                                    ];
                                    ex:sensitivityReviewRestrictionHasLegislation [
                                        {ConditionalFormat("legislationHasUkLegislation", s.Sr.Legislations)}
                                    ]
                                ];
                                ex:sensitivityReviewHasAccessCondition [
                                    ex:accessConditionCode <http://example.com/access#{s.Sr.AccessConditionCode}>
                                ].
                            """)));

    private static string ConditionalFormat(string predicate, string? value) => value is null ? string.Empty : $"ex:{predicate} \"{value}\";";
    private static string ConditionalFormat(string predicate, DateTimeOffset? value) => value is null ? string.Empty : $"ex:{predicate} \"{value.Value.ToString("yyyy-MM-ddTHH:mm:ssK")}\"^^xsd:dateTime;";
    private static string ConditionalFormat(string predicate, long? value) => value is null ? string.Empty : $"ex:{predicate} \"{value}\"^^xsd:integer;";
    private static string ConditionalFormat(string predicate, Uri value) => value is null ? string.Empty : $"ex:{predicate} <{value}>;";
    private static string ConditionalFormat(string predicate, IEnumerable<Uri> value) => value.Any() ? $"ex:{predicate} {string.Join(',', value.Select(v => $"<{v}>"))};" : string.Empty;
}
