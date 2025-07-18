using Api;
using FluentAssertions;
using System.Net.Http.Headers;

namespace Rdf.Tests;

[TestClass]
public class DriExportVariationTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads variations")]
    [DynamicData(nameof(ReadsVariationsData))]
    public async Task ReadsVariations(HttpResponseMessage message, IEnumerable<DriVariation> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var variations = await exporter.GetVariationsByCode("ignore", 0, 0);

        variations.Should().BeEquivalentTo(expected, because);
    }

    private static readonly DriVariation variation1 = new("http://example.com/v1", "v1.txt", "XYZ1");
    private static readonly DriVariation variation2 = new("http://example.com/v2", "v2.txt", "XYZ2");

    public static IEnumerable<object[]> ReadsVariationsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([variation1]),MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriVariation>{ variation1 },
            "has single matching variation"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([variation1, variation2]),MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriVariation>{ variation1, variation2 },
            "has both matching variations"
        ]
    ];

    private static string BuildResponse(DriVariation[] variations) =>
        string.Concat("@prefix ex: <http://example.com/schema/> .\r\n",
            string.Join("\r\n",
                variations.Select(s => $"""
                            <{s.Id}> ex:variationDriId <{s.Id}>;
                                ex:variationHasAsset [
                                    ex:assetReference "{s.AssetReference}"
                                ];
                                ex:variationName "{s.VariationName}".
                            """)));
}
