using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class Restriction(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<DateTimeOffset?> ReviewDate => new DynamicObjectCollection<DateTimeOffset?>(this, Vocabulary.SensitivityReviewRestrictionReviewDate.Uri.ToString());
    public ICollection<DateTimeOffset?> CalculationStartDate => new DynamicObjectCollection<DateTimeOffset?>(this, Vocabulary.SensitivityReviewRestrictionCalculationStartDate.Uri.ToString());
    public ICollection<TimeSpan?> Duration => new DynamicObjectCollection<TimeSpan?>(this, Vocabulary.SensitivityReviewRestrictionDuration.Uri.ToString());
    public ICollection<ILiteralNode> EndYear => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.SensitivityReviewRestrictionEndYear.Uri.ToString());
    public ICollection<string> Description => new DynamicObjectCollection<string>(this, Vocabulary.SensitivityReviewRestrictionDescription.Uri.ToString());
    public ICollection<UkLegislation> UkLegislations => new DynamicObjectCollection<UkLegislation>(this, Vocabulary.SensitivityReviewRestrictionHasLegislation.Uri.ToString());
    public ICollection<RetentionRestriction> RetentionRestriction => new DynamicObjectCollection<RetentionRestriction>(this, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction.Uri.ToString());
}
