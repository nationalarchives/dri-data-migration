using Api;
using VDS.RDF;

namespace Exporter;

internal static class CourtCaseMapper
{
    internal static List<RecordOutput.CourtCase>? GetCourtCases(IGraph graph, IUriNode subject)
    {
        var courtCases = graph.GetUriNodes(subject, Vocabulary.CourtAssetHasCourtCase);
        if (!courtCases.Any())
        {
            return null;
        }

        var cases = new List<RecordOutput.CourtCase>();
        foreach (var courtCase in courtCases)
        {
            var courtCaseSequence = graph.GetSingleNumber(courtCase, Vocabulary.CourtCaseSequence);
            var courtCaseReference = graph.GetSingleText(courtCase, Vocabulary.CourtCaseReference);
            var courtCaseName = graph.GetSingleText(courtCase, Vocabulary.CourtCaseName);
            var courtCaseSummary = graph.GetSingleText(courtCase, Vocabulary.CourtCaseSummary);
            var courtCaseSummaryJudgment = graph.GetSingleText(courtCase, Vocabulary.CourtCaseSummaryJudgment);
            var courtCaseSummaryReasonsForJudgment = graph.GetSingleText(courtCase, Vocabulary.CourtCaseSummaryReasonsForJudgment);
            var courtCaseHearingStartDate = graph.GetSingleDate(courtCase, Vocabulary.CourtCaseHearingStartDate);
            var courtCaseHearingEndDate = graph.GetSingleDate(courtCase, Vocabulary.CourtCaseHearingEndDate);

            cases.Add(new()
            {
                Sequence = courtCaseSequence,
                Reference = courtCaseReference,
                Name = courtCaseName,
                Summary = courtCaseSummary,
                SummaryJudgment = courtCaseSummaryJudgment,
                SummaryReasonsForJudgment = courtCaseSummaryReasonsForJudgment,
                HearingStartDate = RecordMapper.ToDate(courtCaseHearingStartDate),
                HearingEndDate = RecordMapper.ToDate(courtCaseHearingEndDate)
            });
        }

        return cases;
    }
}