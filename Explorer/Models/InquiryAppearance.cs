using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class InquiryAppearance(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> WitnessName => new DynamicObjectCollection<string>(this, Vocabulary.InquiryWitnessName.Uri.ToString());
    public ICollection<string> AppearanceDescription => new DynamicObjectCollection<string>(this, Vocabulary.InquiryWitnessAppearanceDescription.Uri.ToString());
}
