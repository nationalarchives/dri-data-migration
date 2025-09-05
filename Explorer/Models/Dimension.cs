using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Dimension(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<long?> FirstDimensionMillimetre => new DynamicObjectCollection<long?>(this, Vocabulary.FirstDimensionMillimetre.Uri.ToString());
    public ICollection<long?> SecondDimensionMillimetre => new DynamicObjectCollection<long?>(this, Vocabulary.SecondDimensionMillimetre.Uri.ToString());
}
