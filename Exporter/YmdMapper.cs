using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class YmdMapper
{
    internal static string? GetTextDate(IGraph graph, IUriNode relationshipPredicate) =>
        GetTextDate(graph, null, relationshipPredicate);

    internal static string? GetTextDate(IGraph graph, IUriNode? subject, IUriNode relationshipPredicate)
    {
        var ymd = GetYmd(graph, subject, relationshipPredicate);
        if (ymd is null)
        {
            return null;
        }

        return ymd.ToTextDate();
    }

    internal static Ymd? GetYmd(IGraph graph, IUriNode? subject, IUriNode relationshipPredicate)
    {
        var ymdSubject = subject is null ?
            graph.GetSingleUriNode(relationshipPredicate) :
            graph.GetSingleUriNode(subject, relationshipPredicate);
        if (ymdSubject is null)
        {
            return null;
        }

        var year = graph.GetSingleText(ymdSubject, Vocabulary.Year);
        if (string.IsNullOrWhiteSpace(year))
        {
            return null;
        }
        var ymd = new Ymd
        {
            Year = Convert.ToInt32(year)
        };

        var month = graph.GetSingleText(ymdSubject, Vocabulary.Month);
        if (!string.IsNullOrWhiteSpace(month))
        {
            ymd.Month = Convert.ToInt32(month.Replace("--", string.Empty));
            var day = graph.GetSingleText(ymdSubject, Vocabulary.Day);
            if (!string.IsNullOrWhiteSpace(day))
            {
                ymd.Day = Convert.ToInt32(day.Replace("---", string.Empty));
            }
        }

        ymd.Verbatim = graph.GetSingleText(ymdSubject, Vocabulary.DateVerbatim);

        return ymd;
    }
}