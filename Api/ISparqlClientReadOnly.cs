using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace Api;

public interface ISparqlClientReadOnly
{
    Task<IGraph> GetGraphAsync(string sparql, Dictionary<string, object> parameters, CancellationToken cancellationToken);
    Task<IGraph> GetGraphAsync(string sparql, object id, CancellationToken cancellationToken);
    Task<SparqlResultSet> GetResultSetAsync(string sparql, CancellationToken cancellationToken);
    Task<SparqlResultSet> GetResultSetAsync(string sparql, string id, CancellationToken cancellationToken);
    Task<IUriNode?> GetSubjectAsync(string sparql, Dictionary<string, string> parameters, CancellationToken cancellationToken);
    Task<Dictionary<string, IUriNode>> GetDictionaryAsync(string sparql, CancellationToken cancellationToken);
}

public interface IDriSparqlClient : ISparqlClientReadOnly;
public interface IReconciliationSparqlClient : ISparqlClientReadOnly;
public interface IExportSparqlClient : ISparqlClientReadOnly;
