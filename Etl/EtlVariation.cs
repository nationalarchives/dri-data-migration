using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlVariation(ILogger<EtlVariation> logger, IDriRdfExporter rdfExport, IStagingIngest<DriVariation> ingest) : Etl<DriVariation>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriVariation> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetVariationAsync(id, cancellationToken);

    internal override DriVariation Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
