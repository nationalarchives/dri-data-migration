using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Subset(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.SubsetName.Uri.ToString());
    public ICollection<string> Reference => new DynamicObjectCollection<string>(this, Vocabulary.SubsetReference.Uri.ToString());
    public ICollection<Subset> Narrower => new DynamicObjectCollection<Subset>(this, Vocabulary.SubsetHasNarrowerSubset.Uri.ToString());
    public ICollection<Subset> Broader => new DynamicObjectCollection<Subset>(this, Vocabulary.SubsetHasBroaderSubset.Uri.ToString());
    public ICollection<Asset> Assets => new DynamicObjectCollection<Asset>(this, Vocabulary.SubsetHasAsset.Uri.ToString());
    public ICollection<SensitivityReview> SensitivityReviews => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.SubsetHasSensitivityReview.Uri.ToString());
    public ICollection<Retention> Retention => new DynamicObjectCollection<Retention>(this, Vocabulary.SubsetHasRetention.Uri.ToString());
}
public class Subsets(IGraph graph) : DynamicGraph(graph)
{
    public ICollection<Subset> Items => new DynamicSubjectCollection<Subset>("rdf:type", this[Vocabulary.Subset.Uri] as DynamicNode);
}