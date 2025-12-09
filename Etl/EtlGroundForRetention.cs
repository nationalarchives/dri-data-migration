using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlGroundForRetention(ILogger<EtlGroundForRetention> logger, IDriRdfExporter rdfExport, IStagingIngest<DriGroundForRetention> ingest) : Etl<DriGroundForRetention>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriGroundForRetention> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetGroundForRetentionAsync(id, cancellationToken);

    internal override DriGroundForRetention Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
