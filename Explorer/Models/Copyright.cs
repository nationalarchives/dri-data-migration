using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Copyright(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Title => new DynamicObjectCollection<string>(this, Vocabulary.CopyrightTitle.Uri.ToString());
}
