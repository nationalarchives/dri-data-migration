using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Operator(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Identifier => new DynamicObjectCollection<string>(this, Vocabulary.OperatorIdentifier.Uri.ToString());
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.PersonFullName.Uri.ToString());
}
