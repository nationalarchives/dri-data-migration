using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAsset(ILogger<EtlAsset> logger, IDriRdfExporter rdfExport, IStagingIngest<DriAsset> ingest) : Etl<DriAsset>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriAsset> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetAssetAsync(id, cancellationToken);

    internal override DriAsset Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
