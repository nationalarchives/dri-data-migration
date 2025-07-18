using Api;
using FluentAssertions;
using System.Net.Http.Json;

namespace Rdf.Tests;

[TestClass]
public class DriExportLegislationTest : BaseDriExportTest
{
    [TestInitialize]
    public void TestInitialize()
    {
        Initialize();
    }

    [TestMethod("Reads legislations")]
    [DynamicData(nameof(ReadsLegislationsData))]
    public async Task ReadsLegislations(HttpResponseMessage message, IEnumerable<DriLegislation> expected, string because)
    {
        Setup(message);

        var exporter = new DriExport(httpClient, options);

        var legislations = await exporter.GetLegislations();

        legislations.Should().BeEquivalentTo(expected, because);
    }

    private const string legislationBinding = "legislation";
    private const string labelBinding = "label";
    private const string uriType = "uri";
    private const string literalType = "literal";
    private const string legislation1 = "http://example.com/ac1";
    private const string legislation2 = "http://example.com/ac2";
    private const string section1 = "Section 1";
    private const string section2 = "Section 2";

    public static IEnumerable<object[]> ReadsLegislationsData => [
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([legislationBinding, labelBinding], [[(legislationBinding, uriType, legislation1), (labelBinding, literalType, section1)]]))
            },
            new List<DriLegislation>{ new(new Uri(legislation1), section1) },
            "has single matching legislation"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([legislationBinding], [[(legislationBinding, uriType, legislation1)]]))
            },
            new List<DriLegislation>{ new(new Uri(legislation1), null) },
            "allows missing label"
        ],
        [
            new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create<ResultSetBinding>(new([legislationBinding, labelBinding],
                    [
                        [(legislationBinding, uriType, legislation1), (labelBinding, literalType, section1)],
                        [(legislationBinding, uriType, legislation2), (labelBinding, literalType, section2)]
                    ]))
            },
            new List<DriLegislation>{ new(new Uri(legislation1), section1), new(new Uri(legislation2), section2) },
            "has both matching legislations"
        ]
    ];

}
