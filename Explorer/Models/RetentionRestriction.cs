using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class RetentionRestriction(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<long> InstrumentNumber => new DynamicObjectCollection<long>(this, Vocabulary.RetentionInstrumentNumber.Uri.ToString());
    public ICollection<DateTimeOffset> InstrumentSignedDate => new DynamicObjectCollection<DateTimeOffset>(this, Vocabulary.RetentionInstrumentSignatureDate.Uri.ToString());
    public ICollection<DateTimeOffset> ReviewDate => new DynamicObjectCollection<DateTimeOffset>(this, Vocabulary.RetentionRestrictionReviewDate.Uri.ToString());
    public ICollection<GroundForRetention> GroundForRetention => new DynamicObjectCollection<GroundForRetention>(this, Vocabulary.RetentionRestrictionHasGroundForRetention.Uri.ToString());
}
