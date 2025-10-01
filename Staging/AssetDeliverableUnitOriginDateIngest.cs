using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class AssetDeliverableUnitOriginDateIngest(ILogger logger)
{
    private readonly DateParser dateParser = new(logger);

    public void AddOriginDates(IGraph graph, IGraph rdf, INode id, IGraph existing)
    {
        var foundCoverage = rdf.GetTriplesWithPredicate(IngestVocabulary.Coverage).FirstOrDefault()?.Object;
        var startNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginDateStart).SingleOrDefault()?.Object ??
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginApproximateDateStart).SingleOrDefault()?.Object ??
            CacheClient.NewId;
        var endNode = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginDateEnd).SingleOrDefault()?.Object ??
            existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasOriginApproximateDateEnd).SingleOrDefault()?.Object ??
            CacheClient.NewId;
        if (foundCoverage is not null)
        {
            var start = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.StartDate).FirstOrDefault()?.Object as ILiteralNode;
            if (start is not null && string.IsNullOrWhiteSpace(start.Value))
            {
                start = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.FullDate).FirstOrDefault()?.Object as ILiteralNode;
            }
            if (start is not null && !string.IsNullOrWhiteSpace(start.Value))
            {
                var startYmd = dateParser.ParseDate(start.Value);
                switch (startYmd.DateKind)
                {
                    case DateParser.DateType.Date:
                        ParseDate(graph, rdf, id, foundCoverage, startNode, endNode, startYmd);
                        break;
                    case DateParser.DateType.Approximate:
                        ParseApproximateDate(graph, rdf, id, foundCoverage, startNode, endNode, startYmd);
                        break;
                }
            }
            else
            {
                var dateRangeNode = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.DateRange).FirstOrDefault()?.Object as ILiteralNode;
                if (dateRangeNode is not null && !string.IsNullOrWhiteSpace(dateRangeNode.Value))
                {
                    ParseDateRange(graph, id, startNode, endNode, dateRangeNode);
                }
            }
        }
    }


    private void ParseDate(IGraph graph, IGraph rdf, INode id, INode foundCoverage,
        INode startNode, INode endNode, DateParser.YearMonthDay startYmd)
    {
        graph.Assert(id, Vocabulary.AssetHasOriginDateStart, startNode);
        GraphAssert.YearMonthDay(graph, startNode, startYmd.Year, startYmd.Month, startYmd.Day);
        var end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.EndDate).FirstOrDefault()?.Object as ILiteralNode;
        if (end is not null && string.IsNullOrWhiteSpace(end.Value))
        {
            end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.FullDate).FirstOrDefault()?.Object as ILiteralNode;
        }
        if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
        {
            var endYmd = dateParser.ParseDate(end.Value);
            if (endYmd.DateKind == DateParser.DateType.Date)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, endYmd.Year, endYmd.Month, endYmd.Day);
            }
        }
    }

    private void ParseApproximateDate(IGraph graph, IGraph rdf, INode id,
        INode foundCoverage, INode startNode, INode endNode, DateParser.YearMonthDay startYmd)
    {
        graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateStart, startNode);
        GraphAssert.YearMonthDay(graph, startNode, startYmd.Year, startYmd.Month, startYmd.Day);
        var end = rdf.GetTriplesWithSubjectPredicate(foundCoverage, IngestVocabulary.EndDate).FirstOrDefault()?.Object as ILiteralNode;
        if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
        {
            var endYmd = dateParser.ParseDate(end.Value);
            if (endYmd.DateKind == DateParser.DateType.Approximate)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, endYmd.Year, endYmd.Month, endYmd.Day);
            }
        }
    }

    private void ParseDateRange(IGraph graph, INode id, INode startNode, INode endNode, ILiteralNode dateRangeNode)
    {
        var yearRange = dateParser.ParseDateRange(null, dateRangeNode.Value);
        if (yearRange.DateRangeKind == DateParser.DateRangeType.Date)
        {
            graph.Assert(id, Vocabulary.AssetHasOriginDateStart, startNode);
            GraphAssert.YearMonthDay(graph, startNode, yearRange.FirstYear, yearRange.FirstMonth, yearRange.FirstDay);
            if (yearRange.SecondYear.HasValue)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, yearRange.SecondYear, yearRange.SecondMonth, yearRange.SecondDay);
            }
        }
    }
}
