using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlLegislation(ILogger<EtlLegislation> logger, IDriRdfExporter rdfExport, IStagingIngest<DriLegislation> ingest) : Etl<DriLegislation>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriLegislation> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetLegislationAsync(id, cancellationToken);

    internal override DriLegislation Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
