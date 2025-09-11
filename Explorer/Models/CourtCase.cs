using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class CourtCase(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> Reference => new DynamicObjectCollection<string>(this, Vocabulary.CourtCaseReference.Uri.ToString());
    public ICollection<string> Name => new DynamicObjectCollection<string>(this, Vocabulary.CourtCaseName.Uri.ToString());
    public ICollection<string> Summary => new DynamicObjectCollection<string>(this, Vocabulary.CourtCaseSummary.Uri.ToString());
    public ICollection<string> SummaryJudgment => new DynamicObjectCollection<string>(this, Vocabulary.CourtCaseSummaryJudgment.Uri.ToString());
    public ICollection<string> SummaryReasonsForJudgment => new DynamicObjectCollection<string>(this, Vocabulary.CourtCaseSummaryReasonsForJudgment.Uri.ToString());
    public ICollection<DateTimeOffset?> HearingStartDate => new DynamicObjectCollection<DateTimeOffset?>(this, Vocabulary.CourtCaseHearingStartDate.Uri.ToString());
    public ICollection<DateTimeOffset?> HearingEndDate => new DynamicObjectCollection<DateTimeOffset?>(this, Vocabulary.CourtCaseHearingEndDate.Uri.ToString());
}
