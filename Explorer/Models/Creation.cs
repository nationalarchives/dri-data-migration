using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Creation(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<FormalBody> CreationBody => new DynamicObjectCollection<FormalBody>(this, Vocabulary.CreationHasFormalBody.Uri.ToString());
}
