using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlVariation(ILogger<EtlVariation> logger, IOptions<DriSettings> driSettings,
    IDriRdfExporter driExport, IStagingIngest<DriVariation> ingest) : Etl<DriVariation>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriVariation>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriVariation>> GetAsync(int offset, CancellationToken cancellationToken) =>
        driExport.GetVariationsAsync(offset, cancellationToken);
}
