using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Variation(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Id => new DynamicObjectCollection<string>(this, Vocabulary.VariationDriId.Uri.ToString());
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.VariationName.Uri.ToString());
    public ICollection<string> PastName => new DynamicObjectCollection<string>(this, Vocabulary.VariationPastName.Uri.ToString());
    public ICollection<long?> RedactedSequence => new DynamicObjectCollection<long?>(this, Vocabulary.RedactedVariationSequence.Uri.ToString());
    public ICollection<string> Note => new DynamicObjectCollection<string>(this, Vocabulary.VariationNote.Uri.ToString());
    public ICollection<ILiteralNode> Location => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.VariationRelativeLocation.Uri.ToString());
    public ICollection<string> PhysicalConditionDescription => new DynamicObjectCollection<string>(this, Vocabulary.VariationPhysicalConditionDescription.Uri.ToString());
    public ICollection<string> ReferenceGoogleId => new DynamicObjectCollection<string>(this, Vocabulary.VariationReferenceGoogleId.Uri.ToString());
    public ICollection<string> ReferenceParentGoogleId => new DynamicObjectCollection<string>(this, Vocabulary.VariationReferenceParentGoogleId.Uri.ToString());
    public ICollection<string> ScannerOperatorIdentifier => new DynamicObjectCollection<string>(this, Vocabulary.ScannerOperatorIdentifier.Uri.ToString());
    public ICollection<string> ScannerIdentifier => new DynamicObjectCollection<string>(this, Vocabulary.ScannerIdentifier.Uri.ToString());
    public ICollection<DatedNote> DatedNote => new DynamicObjectCollection<DatedNote>(this, Vocabulary.VariationHasDatedNote.Uri.ToString());
    public ICollection<GeographicalPlace> ScannerGeographicalPlace => new DynamicObjectCollection<GeographicalPlace>(this, Vocabulary.ScannedVariationHasScannerGeographicalPlace.Uri.ToString());
    public ICollection<IUriNode> ScannedVariationHasImageSplit => new DynamicObjectCollection<IUriNode>(this, Vocabulary.ScannedVariationHasImageSplit.Uri.ToString());
    public ICollection<IUriNode> ScannedVariationHasImageCrop => new DynamicObjectCollection<IUriNode>(this, Vocabulary.ScannedVariationHasImageCrop.Uri.ToString());
    public ICollection<IUriNode> ScannedVariationHasImageDeskew => new DynamicObjectCollection<IUriNode>(this, Vocabulary.ScannedVariationHasImageDeskew.Uri.ToString());
    public ICollection<Asset> Asset => new DynamicObjectCollection<Asset>(this, Vocabulary.VariationHasAsset.Uri.ToString());
    public ICollection<SensitivityReview> SensitivityReviews => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.VariationHasSensitivityReview.Uri.ToString());
    public ICollection<ILiteralNode> Xml => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.VariationDriXml.Uri.ToString());
}
