using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAdm158SubsetDeliverableUnit(ILogger<EtlAdm158SubsetDeliverableUnit> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriAdm158SubsetDeliverableUnit> ingest) : Etl<DriAdm158SubsetDeliverableUnit>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriAdm158SubsetDeliverableUnit>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriAdm158SubsetDeliverableUnit>> GetAsync(int offset, CancellationToken cancellationToken) =>
        Task.FromResult(driExport.GetAdm158SubsetDeliverableUnits(offset, cancellationToken));
}
