using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class SensitivityReview(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> DriId => new DynamicObjectCollection<string>(this, Vocabulary.SensitivityReviewDriId.Uri.ToString());
    public ICollection<AccessCondition> AccessCondition => new DynamicObjectCollection<AccessCondition>(this, Vocabulary.SensitivityReviewHasAccessCondition.Uri.ToString());
    public ICollection<DateTimeOffset> Date => new DynamicObjectCollection<DateTimeOffset>(this, Vocabulary.SensitivityReviewDate.Uri.ToString());
    public ICollection<string> SensitiveName => new DynamicObjectCollection<string>(this, Vocabulary.SensitivityReviewSensitiveName.Uri.ToString());
    public ICollection<string> SensitiveDescription => new DynamicObjectCollection<string>(this, Vocabulary.SensitivityReviewSensitiveDescription.Uri.ToString());
    public ICollection<SensitivityReview> Past => new DynamicObjectCollection<SensitivityReview>(this, Vocabulary.SensitivityReviewHasPastSensitivityReview.Uri.ToString());
    public ICollection<Restriction> Restriction => new DynamicObjectCollection<Restriction>(this, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction.Uri.ToString());
}