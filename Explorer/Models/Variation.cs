using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Variation(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Id => new DynamicObjectCollection<string>(this, Vocabulary.VariationDriId.Uri.ToString());
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.VariationName.Uri.ToString());
    public ICollection<ILiteralNode> Xml => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.VariationDriXml.Uri.ToString());
    public ICollection<string> Note => new DynamicObjectCollection<string>(this, Vocabulary.VariationNote.Uri.ToString());
    public ICollection<ILiteralNode> Location => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.VariationRelativeLocation.Uri.ToString());
    public ICollection<Asset> Asset => new DynamicObjectCollection<Asset>(this, Vocabulary.VariationHasAsset.Uri.ToString());
    public ICollection<Variation> Redacted => new DynamicObjectCollection<Variation>(this, Vocabulary.VariationHasRedactedVariation.Uri.ToString());
    public ICollection<SensitivityReview> SensitivityReviews => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.VariationHasSensitivityReview.Uri.ToString());
}
