using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class DateYmd(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<ILiteralNode> Year => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.Year.Uri.ToString());
    public ICollection<ILiteralNode> Month => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.Month.Uri.ToString());
    public ICollection<ILiteralNode> Day => new DynamicObjectCollection<ILiteralNode>(this, Vocabulary.Day.Uri.ToString());

    public string? ToDate()
    {
        if (Year.Count == 0)
        {
            return null;
        }

        var sb = new System.Text.StringBuilder();
        sb.Append(Year.Single().Value);
        if (Month.Count > 0)
        {
            sb.Append(Month.Single().Value.Replace("--", "-").PadLeft(2, '0'));
        }
        if (Day.Count > 0)
        {
            sb.Append(Day.Single().Value.Replace("---", "-").PadLeft(2, '0'));
        }
        return sb.ToString();
    }
}
