using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace Api;

public interface ISparqlClient
{
    Task<IGraph> GetGraphAsync(string sparql, Dictionary<string, object> parameters);
    Task<IGraph> GetGraphAsync(string sparql, object id);
    Task<SparqlResultSet> GetResultSetAsync(string sparql);
    Task<IUriNode?> GetSubjectAsync(string sparql, object id);
    Task<Dictionary<string, IUriNode>> GetDictionaryAsync(string sparql);
    Task ApplyDiffAsync(GraphDiffReport diffReport);
}
