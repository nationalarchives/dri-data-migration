using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSensitivityReview(ILogger<EtlSensitivityReview> logger, IDriRdfExporter rdfExport, IStagingIngest<DriSensitivityReview> ingest) : Etl<DriSensitivityReview>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlRdfSourceAsync(rdfExport, offset, cancellationToken);

    internal override Task<DriSensitivityReview> GetAsync(Uri id, CancellationToken cancellationToken) =>
        rdfExport.GetSensitivityReviewAsync(id, cancellationToken);

    internal override DriSensitivityReview Get(string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}