using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class CopyrightMapper
{
    internal static List<string>? GetCopyrights(IGraph graph)
    {
        var copyrights = graph.GetUriNodes(Vocabulary.AssetHasCopyright);
        if (!copyrights.Any())
        {
            return null;
        }

        var copyrightTitles = new List<string>();
        foreach (var copyright in copyrights)
        {
            var title = graph.GetSingleText(copyright, Vocabulary.CopyrightTitle);
            if (!string.IsNullOrEmpty(title))
            {
                copyrightTitles.Add(title);
            }
        }

        return copyrightTitles;
    }
}