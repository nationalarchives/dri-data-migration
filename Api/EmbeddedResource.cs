using System.IO;
using System.Reflection;

namespace Api;

public class EmbeddedResource(Assembly assembly, string baseName)
{
    public string GetSparql(string fileName) => Get(fileName, "sparql");

    public string GetSql(string fileName) => Get(fileName, "sql");

    private string Get(string fileName, string extension)
    {
        using var stream = assembly.GetManifestResourceStream($"{baseName}.{fileName}.{extension}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
