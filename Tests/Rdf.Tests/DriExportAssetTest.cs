using Api;
using FluentAssertions;
using System.Net.Http.Headers;

namespace Rdf.Tests;

[TestClass]
public class DriExportAssetTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads assets")]
    [DynamicData(nameof(ReadsAssetsData))]
    public async Task ReadsAssets(HttpResponseMessage message, IEnumerable<DriAsset> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var assets = await exporter.GetAssetsByCode("ignore", 0, 0);

        assets.Should().BeEquivalentTo(expected, because);
    }

    private static readonly DriAsset asset1 = new("ABC 1", "abc/def", "XYZ1");
    private static readonly DriAsset asset2 = new("ABC 2", "folder2/folder2b", "XYZ2");

    public static IEnumerable<object[]> ReadsAssetsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([asset1]),MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriAsset>{ asset1 },
            "has single matching asset"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([asset1, asset2]),MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriAsset>{ asset1, asset2 },
            "has both matching assets"
        ]
    ];

    private static string BuildResponse(DriAsset[] assets) =>
        string.Concat("@prefix ex: <http://example.com/schema/> .\r\n",
            string.Join("\r\n",
                assets.Select(s => $"""
                            [] ex:assetReference "{s.Reference}";
                                ex:assetHasSubset [
                                    ex:subsetReference "{s.SubsetReference}"
                            ];
                            ex:assetHasRetention [
                                ex:importLocation "{s.Directory}"
                            ].
                            """)));
}
