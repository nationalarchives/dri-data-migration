using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlGroundForRetention(ILogger<EtlGroundForRetention> logger, IDriRdfExporter driExport,
    IStagingIngest<DriGroundForRetention> ingest) : Etl<DriGroundForRetention>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(cancellationToken);

    internal override Task<IEnumerable<DriGroundForRetention>> GetAsync(CancellationToken cancellationToken) =>
        driExport.GetGroundsForRetentionAsync(cancellationToken);

    internal override Task<IEnumerable<DriGroundForRetention>> GetAsync(int offset, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
