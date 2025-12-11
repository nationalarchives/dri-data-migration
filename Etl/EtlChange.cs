using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlChange(ILogger<EtlChange> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriChange> ingest) : Etl<DriChange>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriChange>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriChange>> GetAsync(int offset, CancellationToken cancellationToken) =>
        Task.FromResult(driExport.GetChanges(offset, cancellationToken));
}
