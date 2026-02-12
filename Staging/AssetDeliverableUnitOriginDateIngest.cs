using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;

namespace Staging;

internal class AssetDeliverableUnitOriginDateIngest(ILogger logger)
{
    private readonly DateParser dateParser = new(logger);

    internal void AddOriginDates(IGraph graph, IGraph rdf, IUriNode id, IGraph existing)
    {
        var foundCoverage = rdf.GetTriplesWithPredicate(IngestVocabulary.Coverage).FirstOrDefault()?.Object;
        if (foundCoverage is null)
        {
            return;
        }
        var startNode = existing.GetSingleUriNode(id, Vocabulary.AssetHasOriginDateStart) ??
            existing.GetSingleUriNode(id, Vocabulary.AssetHasOriginApproximateDateStart) ??
            CacheClient.NewId;
        var endNode = existing.GetSingleUriNode(id, Vocabulary.AssetHasOriginDateEnd) ??
            existing.GetSingleUriNode(id, Vocabulary.AssetHasOriginApproximateDateEnd) ??
            CacheClient.NewId;
        var start = rdf.GetSingleLiteral(foundCoverage, IngestVocabulary.StartDate);
        if (start is not null && string.IsNullOrWhiteSpace(start.Value))
        {
            start = rdf.GetSingleLiteral(foundCoverage, IngestVocabulary.FullDate);
        }
        if (start is not null && !string.IsNullOrWhiteSpace(start.Value))
        {
            var startYmd = dateParser.ParseDate(start.Value);
            switch (startYmd.DateKind)
            {
                case DateParser.DateType.Date:
                    ParseDate(graph, rdf, id, foundCoverage, startNode, endNode, startYmd, start.Value);
                    break;
                case DateParser.DateType.Approximate:
                    ParseApproximateDate(graph, rdf, id, foundCoverage, startNode, endNode, startYmd, start.Value);
                    break;
            }
        }
        else
        {
            var dateRangeNode = rdf.GetSingleLiteral(foundCoverage, IngestVocabulary.DateRange);
            if (dateRangeNode is not null && !string.IsNullOrWhiteSpace(dateRangeNode.Value))
            {
                ParseDateRange(graph, id, startNode, endNode, dateRangeNode);
            }
        }
    }

    private void ParseDate(IGraph graph, IGraph rdf, INode id, INode foundCoverage,
        INode startNode, INode endNode, DateParser.YearMonthDay startYmd, string date)
    {
        graph.Assert(id, Vocabulary.AssetHasOriginDateStart, startNode);
        GraphAssert.YearMonthDay(graph, startNode, startYmd.Year, startYmd.Month, startYmd.Day, date);
        var end = rdf.GetSingleLiteral(foundCoverage, IngestVocabulary.EndDate);
        if (end is not null && string.IsNullOrWhiteSpace(end.Value))
        {
            end = rdf.GetSingleLiteral(foundCoverage, IngestVocabulary.FullDate);
        }
        if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
        {
            var endYmd = dateParser.ParseDate(end.Value);
            if (endYmd.DateKind == DateParser.DateType.Date)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, endYmd.Year, endYmd.Month, endYmd.Day, end.Value);
            }
        }
    }

    private void ParseApproximateDate(IGraph graph, IGraph rdf, INode id,
        INode foundCoverage, INode startNode, INode endNode, DateParser.YearMonthDay startYmd, string date)
    {
        graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateStart, startNode);
        GraphAssert.YearMonthDay(graph, startNode, startYmd.Year, startYmd.Month, startYmd.Day, date);
        var end = rdf.GetSingleLiteral(foundCoverage, IngestVocabulary.EndDate);
        if (end is not null && !string.IsNullOrWhiteSpace(end.Value))
        {
            var endYmd = dateParser.ParseDate(end.Value);
            if (endYmd.DateKind == DateParser.DateType.Approximate)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, endYmd.Year, endYmd.Month, endYmd.Day, end.Value);
            }
        }
    }

    private void ParseDateRange(IGraph graph, INode id, INode startNode, INode endNode, ILiteralNode dateRangeNode)
    {
        var yearRange = dateParser.ParseDateRange(null, dateRangeNode.Value);
        if (yearRange.DateRangeKind == DateParser.DateRangeType.Date)
        {
            graph.Assert(id, Vocabulary.AssetHasOriginDateStart, startNode);
            GraphAssert.YearMonthDay(graph, startNode, yearRange.FirstYear, yearRange.FirstMonth, yearRange.FirstDay, dateRangeNode.Value);
            if (yearRange.SecondYear.HasValue)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, yearRange.SecondYear, yearRange.SecondMonth, yearRange.SecondDay, dateRangeNode.Value);
            }
        }
        else if (yearRange.DateRangeKind == DateParser.DateRangeType.Approximate)
        {
            graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateStart, startNode);
            GraphAssert.YearMonthDay(graph, startNode, yearRange.FirstYear, yearRange.FirstMonth, yearRange.FirstDay, dateRangeNode.Value);
            if (yearRange.SecondYear.HasValue)
            {
                graph.Assert(id, Vocabulary.AssetHasOriginApproximateDateEnd, endNode);
                GraphAssert.YearMonthDay(graph, endNode, yearRange.SecondYear, yearRange.SecondMonth, yearRange.SecondDay, dateRangeNode.Value);
            }
        }
    }
}
