using Api;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Rdf.Tests;

public class BaseDriExportTest
{
    internal const string turtleMime = "text/turtle";
    internal Mock<HttpMessageHandler> handler;
    internal HttpClient httpClient;
    internal IOptions<DriSettings> options;

    internal void Initialize()
    {
        handler = new Mock<HttpMessageHandler>();
        httpClient = new HttpClient(handler.Object);
        options = Options.Create<DriSettings>(new() { SparqlConnectionString = "http://ignore" });
    }

    internal void Setup(HttpResponseMessage message)
    {
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(message);
    }
}
