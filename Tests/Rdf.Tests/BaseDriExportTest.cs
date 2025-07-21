using Api;
using Moq;

namespace Rdf.Tests;

public class BaseDriExportTest
{
    internal Mock<ISparqlClient> sparqlClient;

    internal void Initialize()
    {
        sparqlClient = new Mock<ISparqlClient>();
    }
}
