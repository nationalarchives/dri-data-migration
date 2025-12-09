using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlWo409SubsetDeliverableUnit(ILogger<EtlWo409SubsetDeliverableUnit> logger, IDriSqlExporter sqlExport, IStagingIngest<DriWo409SubsetDeliverableUnit> ingest) : Etl<DriWo409SubsetDeliverableUnit>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlSqlSourceAsync(sqlExport, offset, cancellationToken);

    internal override DriWo409SubsetDeliverableUnit Get(string id, CancellationToken cancellationToken) =>
        sqlExport.GetWo409SubsetDeliverableUnit(id, cancellationToken);

    internal override Task<DriWo409SubsetDeliverableUnit> GetAsync(Uri id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
