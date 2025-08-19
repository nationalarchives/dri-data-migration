using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Retention(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> ImportLocation => new DynamicObjectCollection<string>(this, Vocabulary.ImportLocation.Uri.ToString());
    public ICollection<FormalBody> RetentionBody => new DynamicObjectCollection<FormalBody>(this, Vocabulary.RetentionHasFormalBody.Uri.ToString());
}
