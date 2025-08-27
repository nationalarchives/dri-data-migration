using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Asset(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.AssetName.Uri.ToString());
    public ICollection<string> Description => new DynamicObjectCollection<string>(this, Vocabulary.AssetDescription.Uri.ToString());
    public ICollection<string> Id => new DynamicObjectCollection<string>(this, Vocabulary.AssetDriId.Uri.ToString());
    public ICollection<string> Reference => new DynamicObjectCollection<string>(this, Vocabulary.AssetReference.Uri.ToString());
    public ICollection<string> BatchId => new DynamicObjectCollection<string>(this, Vocabulary.BatchDriId.Uri.ToString());
    public ICollection<string> ConsignmentId => new DynamicObjectCollection<string>(this, Vocabulary.ConsignmentTdrId.Uri.ToString());
    public ICollection<string> RelationDescription => new DynamicObjectCollection<string>(this, Vocabulary.AssetRelationDescription.Uri.ToString());
    public ICollection<string> PhysicalDescription => new DynamicObjectCollection<string>(this, Vocabulary.AssetPhysicalDescription.Uri.ToString());
    public ICollection<string> UseRestrictionDescription => new DynamicObjectCollection<string>(this, Vocabulary.AssetUsageRestrictionDescription.Uri.ToString());
    public ICollection<string> EvidenceProvider => new DynamicObjectCollection<string>(this, Vocabulary.EvidenceProviderName.Uri.ToString());
    public ICollection<string> Investigation => new DynamicObjectCollection<string>(this, Vocabulary.InvestigationName.Uri.ToString());
    public ICollection<string> CourtSession => new DynamicObjectCollection<string>(this, Vocabulary.CourtSessionDescription.Uri.ToString());
    public ICollection<DateTimeOffset> SessionDate => new DynamicObjectCollection<DateTimeOffset>(this, Vocabulary.CourtSessionDate.Uri.ToString());
    public ICollection<ILiteralNode> Xml => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.AssetDriXml.Uri.ToString());
    public ICollection<Variation> Variations => new DynamicObjectCollection<Variation>(this, Vocabulary.AssetHasVariation.Uri.ToString());
    public ICollection<Retention> Retention => new DynamicObjectCollection<Retention>(this, Vocabulary.AssetHasRetention.Uri.ToString());
    public ICollection<Creation> Creation => new DynamicObjectCollection<Creation>(this, Vocabulary.AssetHasCreation.Uri.ToString());
    public ICollection<SensitivityReview> SensitivityReviews => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.AssetHasSensitivityReview.Uri.ToString());
    public ICollection<Subset> Subset => new DynamicObjectCollection<Subset>(this, Vocabulary.AssetHasSubset.Uri.ToString());
    public ICollection<Language> Language => new DynamicObjectCollection<Language>(this, Vocabulary.AssetHasLanguage.Uri.ToString());
    public ICollection<Copyright> Copyrights => new DynamicObjectCollection<Copyright>(this, Vocabulary.AssetHasCopyright.Uri.ToString());
    public ICollection<CourtCase> CourtCases => new DynamicObjectCollection<CourtCase>(this, Vocabulary.CourtAssetHasCourtCase.Uri.ToString());
    public ICollection<IUriNode> LegalStatus => new DynamicObjectCollection<IUriNode>(this, Vocabulary.AssetHasLegalStatus.Uri.ToString());
}
