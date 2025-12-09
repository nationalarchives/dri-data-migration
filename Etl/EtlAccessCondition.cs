using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAccessCondition(ILogger<EtlAccessCondition> logger, IDriRdfExporter rdfExport, IStagingIngest<DriAccessCondition> ingest) : Etl<DriAccessCondition>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriAccessCondition> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetAccessConditionAsync(id, cancellationToken);

    internal override DriAccessCondition Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
