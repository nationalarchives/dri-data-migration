using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class SealCatagory(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.SealCategoryName.Uri.ToString());
}
