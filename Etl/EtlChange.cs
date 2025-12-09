using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlChange(ILogger<EtlChange> logger, IDriSqlExporter sqlExport, IStagingIngest<DriChange> ingest) : Etl<DriChange>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlSqlSourceAsync(sqlExport, offset, cancellationToken);

    internal override DriChange Get(string id, CancellationToken cancellationToken) =>
        sqlExport.GetChange(id, cancellationToken);

    internal override Task<DriChange> GetAsync(Uri id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
