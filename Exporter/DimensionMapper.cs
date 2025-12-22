using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class DimensionMapper
{
    internal static RecordOutput.Dimension? GetDimension(IGraph graph, IUriNode relationshipPredicate)
    {
        var dimension = graph.GetSingleUriNode(relationshipPredicate);
        if (dimension is null)
        {
            return null;
        }

        if (dimension.Equals(Vocabulary.FragmentDimension))
        {
            return new() { IsFragment = true };
        }

        var firstDimension = graph.GetSingleNumber(dimension, Vocabulary.FirstDimensionMillimetre);
        var secondDimension = graph.GetSingleNumber(dimension, Vocabulary.SecondDimensionMillimetre);
        if (firstDimension is null && secondDimension is null)
        {
            return null;
        }

        return new()
        {
            First = firstDimension,
            Second = secondDimension
        };
    }
}