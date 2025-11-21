using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VDS.RDF;
using VDS.RDF.Query;

namespace Rdf.Tests;

[TestClass]
public sealed class SparqlClientReadOnlyTest
{
    private static readonly Uri baseUri = new("http://example.com/");

    [TestMethod(DisplayName = "Reads graph")]
    public async Task ReadsGraph()
    {
        var subject = "urn:subject";
        var predicate = "urn:predicate";
        var obj = "urn:object";
        var sparql = "construct { ?s ?p ?o } where { ?s ?p ?o }";
        var handler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage()
        {
            Content = new StringContent($"<{subject}> <{predicate}> <{obj}>.", new MediaTypeHeaderValue(MimeTypesHelper.Turtle[0])),
            StatusCode = System.Net.HttpStatusCode.OK
        };
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var http = new HttpClient(handler.Object);
        var client = new TestSparqlClientReadOnly(http);
        var expected = new Graph()
        {
            BaseUri = baseUri
        };
        expected.Assert(expected.CreateUriNode(new Uri(subject)),
            expected.CreateUriNode(new Uri(predicate)),
            expected.CreateUriNode(new Uri(obj)));

        var graph = await client.GetGraphAsync(sparql, string.Empty, CancellationToken.None);

        graph.Should().BeEquivalentTo(expected);
    }

    [TestMethod(DisplayName = "Reads results")]
    public async Task ReadsResults()
    {
        var sparql = "select ?o where { ?s ?p ?o }";
        var name = "o";
        var oValue = "o-value";
        var sparqlResponse = new SparqlResultResponse()
        {
            head = new()
            {
                vars = [name]
            },
            results = new()
            {
                bindings = [
                    new Dictionary<string, ResultItem>()
                    {
                        [name]=new()
                        {
                            type="literal",
                            value=oValue
                        }
                    }
                ]
            }
        };
        var handler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage()
        {
            Content = JsonContent.Create(sparqlResponse, new MediaTypeHeaderValue(MimeTypesHelper.SparqlResultsJson[0])),
            StatusCode = System.Net.HttpStatusCode.OK
        };
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var http = new HttpClient(handler.Object);
        var client = new TestSparqlClientReadOnly(http);
        var expected = new SparqlResult([new(name, new LiteralNode(oValue))]);

        var result = await client.GetResultSetAsync(sparql, CancellationToken.None);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(expected, o => o.ComparingByValue<KeyValuePair<string, INode>>());
    }

    [TestMethod(DisplayName = "Reads subject")]
    public async Task ReadsSubject()
    {
        var subject = "urn:subject";
        var sparql = "construct { ?s ?p ?o } where { ?s ?p ?o }";
        var handler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage()
        {
            Content = new StringContent($"<{subject}> <urn:predicate> <urn:object>.", new MediaTypeHeaderValue(MimeTypesHelper.Turtle[0])),
            StatusCode = System.Net.HttpStatusCode.OK
        };
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var http = new HttpClient(handler.Object);
        var client = new TestSparqlClientReadOnly(http);
        var expected = new UriNode(new Uri(subject));

        var result = await client.GetSubjectAsync(sparql, [], CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
    }

    [TestMethod(DisplayName = "Reads dictionary")]
    public async Task ReadsDictionary()
    {
        var subject = "urn:subject";
        var obj = "object";
        var sparql = "construct { ?s ?p ?o } where { ?s ?p ?o }";
        var handler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage()
        {
            Content = new StringContent($"<{subject}> <urn:predicate> \"{obj}\".", new MediaTypeHeaderValue(MimeTypesHelper.Turtle[0])),
            StatusCode = System.Net.HttpStatusCode.OK
        };
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var http = new HttpClient(handler.Object);
        var client = new TestSparqlClientReadOnly(http);
        var expected = new Dictionary<string, IUriNode>
        {
            [obj] = new UriNode(new Uri(subject))
        };

        var dictionary = await client.GetDictionaryAsync(sparql, CancellationToken.None);

        dictionary.Should().BeEquivalentTo(expected);
    }

    class TestSparqlClientReadOnly(HttpClient httpClient) : SparqlClientReadOnly(httpClient, baseUri);

    public class SparqlResultResponse
    {
        public required Head head { get; set; }
        public required Results results { get; set; }
    }

    public class Head
    {
        public required string[] vars { get; set; }
    }

    public class Results
    {
        public required Dictionary<string, ResultItem>[] bindings { get; set; }
    }

    public class ResultItem
    {
        public required string type { get; set; }
        public required string value { get; set; }
    }

}
