using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAssetDeliverableUnit(ILogger<EtlAssetDeliverableUnit> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriAssetDeliverableUnit> ingest) : Etl<DriAssetDeliverableUnit>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriAssetDeliverableUnit>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriAssetDeliverableUnit>> GetAsync(int offset, CancellationToken cancellationToken) =>
        Task.FromResult(driExport.GetAssetDeliverableUnits(offset, cancellationToken));
}
