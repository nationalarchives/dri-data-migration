using System.IO;
using System.Reflection;

namespace Rdf;

public class EmbeddedSparqlResource(Assembly assembly, string baseName)
{
    public string GetSparql(string fileName)
    {
        using var stream = assembly.GetManifestResourceStream($"{baseName}.{fileName}.sparql");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
