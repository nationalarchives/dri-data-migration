using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Asset(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.AssetName.Uri.ToString());
    public ICollection<string> Reference => new DynamicObjectCollection<string>(this, Vocabulary.AssetReference.Uri.ToString());
    public ICollection<Variation> Variations => new DynamicObjectCollection<Variation>(this, Vocabulary.AssetHasVariation.Uri.ToString());
    public ICollection<Retention> Retention => new DynamicObjectCollection<Retention>(this, Vocabulary.AssetHasRetention.Uri.ToString());
    public ICollection<SensitivityReview> SensitivityReviews => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.AssetHasSensitivityReview.Uri.ToString());
    public ICollection<Subset> Subset => new DynamicObjectCollection<Subset>(this, Vocabulary.AssetHasSubset.Uri.ToString());
}
