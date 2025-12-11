using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAccessCondition(ILogger<EtlAccessCondition> logger, IDriRdfExporter driExport,
    IStagingIngest<DriAccessCondition> ingest) : Etl<DriAccessCondition>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(cancellationToken);

    internal override Task<IEnumerable<DriAccessCondition>> GetAsync(CancellationToken cancellationToken) =>
        driExport.GetAccessConditionsAsync(cancellationToken);

    internal override Task<IEnumerable<DriAccessCondition>> GetAsync(int offset, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
