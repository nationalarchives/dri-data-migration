using Api;
using FluentAssertions;
using System.Net.Http.Json;

namespace Rdf.Tests;

[TestClass]
public class DriExportGroundForRetentionTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads grounds for retention")]
    [DynamicData(nameof(ReadsGroundsForRetentionData))]
    public async Task ReadsGroundsForRetention(HttpResponseMessage message, IEnumerable<DriGroundForRetention> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var grounds = await exporter.GetGroundForRetentions();

        grounds.Should().BeEquivalentTo(expected, because);
    }

    private const string codeBinding = "label";
    private const string descriptionBinding = "comment";
    private const string code1 = "1a";
    private const string code2 = "2b";
    private const string description1 = "Ground for retention 1";
    private const string description2 = "Ground for retention 2";

    public static IEnumerable<object[]> ReadsGroundsForRetentionData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([codeBinding, descriptionBinding], [[(codeBinding, code1), (descriptionBinding, description1)]]))
            },
            new List<DriGroundForRetention>{ new(code1, description1) },
            "has single matching ground for retention"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([descriptionBinding, codeBinding],
                    [
                        [(codeBinding, code1), (descriptionBinding, description1)],
                        [(codeBinding, code2), (descriptionBinding, description2)]
                    ]))
            },
            new List<DriGroundForRetention>{ new(code1, description1), new(code2, description2) },
            "has both matching grounds for retention"
        ]
    ];

}
