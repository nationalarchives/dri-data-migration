using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class GroundForRetention(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Code => new DynamicObjectCollection<string>(this, Vocabulary.GroundForRetentionCode.Uri.ToString());
    public ICollection<string> Description => new DynamicObjectCollection<string>(this, Vocabulary.GroundForRetentionDescription.Uri.ToString());
}
