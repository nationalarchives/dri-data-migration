using System;
using System.IO;
using System.Reflection;
using VDS.RDF;

namespace Rdf;

static class SparqlResource
{
    private static readonly Uri idNamespace = new("http://id.example.com/");
    
    internal static string GetEmbeddedSparql(Assembly assembly, string baseName, string fileName)
    {
        using var stream = assembly.GetManifestResourceStream($"{baseName}.{fileName}.sparql");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));

}
