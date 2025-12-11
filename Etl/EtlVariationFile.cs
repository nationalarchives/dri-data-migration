using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlVariationFile(ILogger<EtlVariationFile> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriVariationFile> ingest) : Etl<DriVariationFile>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriVariationFile>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriVariationFile>> GetAsync(int offset, CancellationToken cancellationToken) =>
        Task.FromResult(driExport.GetVariationFiles(offset, cancellationToken));
}
