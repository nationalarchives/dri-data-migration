using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class LocationMapper
{
    internal static LocationPath GetLocation(IGraph graph)
    {
        var narrowestSubset = graph.GetSingleUriNode(Vocabulary.AssetHasSubset);
        if (narrowestSubset is null)
        {
            return new();
        }

        List<LocationPath> locations = [];
        locations.Add(GetSubsetLocation(graph, narrowestSubset));
        var broader = graph.GetTriplesWithPredicateObject(Vocabulary.SubsetHasNarrowerSubset, narrowestSubset)
            .Select(t => t.Subject).Cast<IUriNode>().SingleOrDefault();
        if (broader is not null)
        {
            do
            {
                locations.Insert(0, GetSubsetLocation(graph, broader));
                broader = graph.GetTriplesWithPredicateObject(Vocabulary.SubsetHasNarrowerSubset, broader)
                    .Select(t => t.Subject).Cast<IUriNode>().SingleOrDefault();

            } while (broader is not null);
        }

        var original = locations.LastOrDefault()?.Original;
        List<string> published = [];
        var previous = string.Empty;
        foreach (var location in locations)
        {
            var segment = string.IsNullOrWhiteSpace(location.Original) ?
                string.Empty : string.IsNullOrWhiteSpace(previous) ?
                location.Original : location.Original.Replace(previous, string.Empty);
            if (!string.IsNullOrWhiteSpace(location.SensitiveName))
            {
                published.Add(location.SensitiveName.Trim('/'));
            }
            else if (!string.IsNullOrWhiteSpace(segment))
            {
                published.Add(segment.Trim('/'));
            }
            if (!string.IsNullOrWhiteSpace(location.Original))
            {
                previous = location.Original;
            }
        }

        var sensitive = string.Join('/', published);

        return new(original!, sensitive);
    }

    private static LocationPath GetSubsetLocation(IGraph graph, IUriNode subset)
    {
        var location = graph.GetSingleTransitiveLiteral(subset, Vocabulary.SubsetHasRetention, Vocabulary.ImportLocation)?.Value ?? string.Empty;
        var sensitiveName = graph.GetSingleTransitiveLiteral(subset, Vocabulary.SubsetHasSensitivityReview, Vocabulary.SensitivityReviewSensitiveName)?.Value;
        if (string.IsNullOrWhiteSpace(sensitiveName))
        {
            return new(location, string.Empty);
        }
        return new(location, sensitiveName);
    }
}