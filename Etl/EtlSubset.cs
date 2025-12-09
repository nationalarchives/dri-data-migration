using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSubset(ILogger<EtlSubset> logger, IDriRdfExporter rdfExport, IStagingIngest<DriSubset> ingest) : Etl<DriSubset>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriSubset> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetSubsetAsync(id, cancellationToken);

    internal override DriSubset Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
