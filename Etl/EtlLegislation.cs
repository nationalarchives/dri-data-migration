using Api;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlLegislation(ILogger<EtlLegislation> logger, IDriRdfExporter driExport,
    IStagingIngest<DriLegislation> ingest) : Etl<DriLegislation>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(cancellationToken);

    internal override Task<IEnumerable<DriLegislation>> GetAsync(CancellationToken cancellationToken) =>
        driExport.GetLegislationsAsync(cancellationToken);

    internal override Task<IEnumerable<DriLegislation>> GetAsync(int offset, CancellationToken cancellationToken) =>
        throw new System.NotImplementedException();
}
