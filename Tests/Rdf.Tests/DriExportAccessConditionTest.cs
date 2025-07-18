using Api;
using FluentAssertions;
using System.Net.Http.Json;

namespace Rdf.Tests;

[TestClass]
public class DriExportAccessConditionTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads access conditions")]
    [DynamicData(nameof(ReadsAccessConditionsData))]
    public async Task ReadsAccessConditions(HttpResponseMessage message, IEnumerable<DriAccessCondition> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var accessConditions = await exporter.GetAccessConditions();

        accessConditions.Should().BeEquivalentTo(expected, because);
    }

    private const string acBinding = "c";
    private const string labelBinding = "label";
    private const string uriType = "uri";
    private const string literalType = "literal";
    private const string ac1 = "http://example.com/ac1#a1";
    private const string ac2 = "http://example.com/ac2#ac2";
    private const string label1 = "Access Condition 1";
    private const string label2 = "Access Condition 2";

    public static IEnumerable<object[]> ReadsAccessConditionsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([acBinding, labelBinding], [[(acBinding, uriType, ac1), (labelBinding, literalType, label1)]]))
            },
            new List<DriAccessCondition>{ new("a1", label1) },
            "has single matching access condition"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([acBinding, labelBinding],
                    [
                        [(acBinding, uriType, ac1), (labelBinding, literalType, label1)],
                        [(acBinding, uriType, ac2), (labelBinding, literalType, label2)]
                    ]))
            },
            new List<DriAccessCondition>{ new("a1", label1), new("ac2", label2) },
            "has both matching access conditions"
        ]
    ];

}
