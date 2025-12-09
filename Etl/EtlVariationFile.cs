using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlVariationFile(ILogger<EtlVariationFile> logger, IDriSqlExporter sqlExport, IStagingIngest<DriVariationFile> ingest) : Etl<DriVariationFile>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlSqlSourceAsync(sqlExport, offset, cancellationToken);

    internal override DriVariationFile Get(string id, CancellationToken cancellationToken) =>
        sqlExport.GetVariationFile(id, cancellationToken);

    internal override Task<DriVariationFile> GetAsync(Uri id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
