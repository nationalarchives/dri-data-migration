using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class UkLegislation(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<IUriNode> Legislation => new DynamicObjectCollection<IUriNode>(this, Vocabulary.LegislationHasUkLegislation.Uri.ToString());
    public ICollection<string> Reference => new DynamicObjectCollection<string>(this, Vocabulary.LegislationSectionReference.Uri.ToString());
}
