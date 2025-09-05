using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;

namespace Explorer.Models;

public class DatedNote(INode node, IGraph graph) : DynamicNode(node, graph)
{
    public ICollection<string> ArchivistNote => new DynamicObjectCollection<string>(this, Vocabulary.ArchivistNote.Uri.ToString());
    public ICollection<DateYmd> Date => new DynamicObjectCollection<DateYmd>(this, Vocabulary.DatedNoteHasDate.Uri.ToString());
}
