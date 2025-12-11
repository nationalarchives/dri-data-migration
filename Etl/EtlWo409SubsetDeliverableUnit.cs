using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlWo409SubsetDeliverableUnit(ILogger<EtlWo409SubsetDeliverableUnit> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriWo409SubsetDeliverableUnit> ingest) : Etl<DriWo409SubsetDeliverableUnit>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlAsync(offset, driSettings.Value.FetchPageSize, cancellationToken);

    internal override Task<IEnumerable<DriWo409SubsetDeliverableUnit>> GetAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    internal override Task<IEnumerable<DriWo409SubsetDeliverableUnit>> GetAsync(int offset, CancellationToken cancellationToken) =>
        Task.FromResult(driExport.GetWo409SubsetDeliverableUnits(offset, cancellationToken));
}
