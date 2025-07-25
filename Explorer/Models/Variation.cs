using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Variation(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Id => new DynamicObjectCollection<string>(this, Vocabulary.VariationDriId.Uri.ToString());
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.VariationName.Uri.ToString());
    public ICollection<Asset> Asset => new DynamicObjectCollection<Asset>(this, Vocabulary.VariationHasAsset.Uri.ToString());
    public ICollection<SensitivityReview> SensitivityReviews => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.VariationHasSensitivityReview.Uri.ToString());
}
