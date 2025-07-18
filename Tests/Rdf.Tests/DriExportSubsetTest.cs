using Api;
using FluentAssertions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Rdf.Tests;

[TestClass]
public class DriExportSubsetTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads top level subsets")]
    [DynamicData(nameof(ReadsTopSubsetsData))]
    public async Task ReadsTopSubsets(HttpResponseMessage message, IEnumerable<DriSubset> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var subsets = await exporter.GetBroadestSubsets();

        subsets.Should().BeEquivalentTo(expected, because);
    }

    [TestMethod("Reads subsets")]
    [DynamicData(nameof(ReadsSubsetsData))]
    public async Task ReadsSubsets(HttpResponseMessage message, IEnumerable<DriSubset> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var subsets = await exporter.GetSubsetsByCode("ignore", 0, 0);

        subsets.Should().BeEquivalentTo(expected, because);
    }

    private static readonly string codeBinding = "code";
    private static readonly string topLevel1 = "Subset Top 1";
    private static readonly string topLevel2 = "Subset Top 2";
    private static readonly DriSubset subset1 = new("ABC 1", "abc/def");
    private static readonly DriSubset subset2 = new("ABC 2", "abc/def", "ABC 1");

    public static IEnumerable<object[]> ReadsTopSubsetsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([codeBinding], [[(codeBinding, topLevel1)]]))
            },
            new List<DriSubset>{ new(topLevel1, topLevel1) },
            "has single matching subset"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([codeBinding], [[(codeBinding, topLevel1)], [(codeBinding, topLevel2)]]))
            },
            new List<DriSubset>{ new(topLevel1, topLevel1), new(topLevel2, topLevel2) },
            "has both matching subsets"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([codeBinding, "ignore"], [[(codeBinding, topLevel1),("ignore",Guid.NewGuid().ToString())]]))
            },
            new List<DriSubset>{ new(topLevel1, topLevel1) },
            "ignores additional binding"
        ]
    ];

    public static IEnumerable<object[]> ReadsSubsetsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([subset1]),MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriSubset>{ subset1 },
            "has single matching subset"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(BuildResponse([subset1, subset2]),MediaTypeHeaderValue.Parse(turtleMime))
            },
            new List<DriSubset>{ subset1, subset2 },
            "has both matching subsets"
        ]
    ];

    private static string BuildResponse(DriSubset[] subsets) =>
        string.Concat("@prefix ex: <http://example.com/schema/> .\r\n",
            string.Join("\r\n",
                subsets.Select(s => $"""
                            <urn:{s.Id.Replace(' ', '-')}> ex:subsetReference "{s.Reference}";
                                ex:subsetHasBroaderSubset [
                                    {ConditionalFormat("subsetReference", s.ParentReference)}
                            ];
                            ex:subsetHasRetention [
                                ex:importLocation "{s.Directory}"
                            ].
                            """)));

    private static string ConditionalFormat(string predicate, string? value) => value is null ? string.Empty : $"ex:{predicate} \"{value}\";";
}
