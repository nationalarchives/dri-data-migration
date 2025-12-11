using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSensitivityReview(ILogger<EtlSensitivityReview> logger, IOptions<DriSettings> driSettings,
    IDriRdfExporter driExport, IStagingIngest<DriSensitivityReview> ingest) : Etl<DriSensitivityReview>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriSensitivityReview>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriSensitivityReview>> GetAsync(int offset, CancellationToken cancellationToken) =>
        driExport.GetSensitivityReviewsAsync(offset, cancellationToken);
}
