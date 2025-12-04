using Api;
using VDS.RDF;

namespace Exporter;

internal static class YmdMapper
{
    internal static string? GetYmd(IGraph graph, IUriNode relationshipPredicate) =>
        GetYmd(graph, null, relationshipPredicate);

    internal static string? GetYmd(IGraph graph, IUriNode? subject, IUriNode relationshipPredicate)
    {
        var ymd = subject is null ?
            graph.GetSingleUriNode(relationshipPredicate) :
            graph.GetSingleUriNode(subject, relationshipPredicate);
        if (ymd is null)
        {
            return null;
        }

        var year = graph.GetSingleText(ymd, Vocabulary.Year);
        if (string.IsNullOrWhiteSpace(year))
        {
            return null;
        }
        var sb = new System.Text.StringBuilder();
        sb.Append(year);

        var month = graph.GetSingleText(ymd, Vocabulary.Month);
        if (!string.IsNullOrWhiteSpace(month))
        {
            sb.Append('-');
            sb.Append(month.Replace("--", string.Empty));
            var day = graph.GetSingleText(ymd, Vocabulary.Day);
            if (!string.IsNullOrWhiteSpace(day))
            {
                sb.Append('-');
                sb.Append(day.Replace("---", string.Empty));
            }
        }

        return sb.ToString();
    }
}