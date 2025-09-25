using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Change(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Id => new DynamicObjectCollection<string>(this, Vocabulary.ChangeDriId.Uri.ToString());
    public ICollection<ILiteralNode> Description => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.ChangeDescription.Uri.ToString());
    public ICollection<DateTimeOffset?> Timestamp => new DynamicObjectCollection<DateTimeOffset?>(this, Vocabulary.ChangeDateTime.Uri.ToString());
    public ICollection<Operator> Operator => new DynamicObjectCollection<Operator>(this, Vocabulary.ChangeHasOperator.Uri.ToString());
}
