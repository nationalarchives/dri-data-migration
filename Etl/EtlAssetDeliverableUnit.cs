using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAssetDeliverableUnit(ILogger<EtlAssetDeliverableUnit> logger, IDriSqlExporter sqlExport, IStagingIngest<DriAssetDeliverableUnit> ingest) : Etl<DriAssetDeliverableUnit>(logger, ingest), IEtl
{
    public Task RunAsync(int offset, CancellationToken cancellationToken) =>
        EtlSqlSourceAsync(sqlExport, offset, cancellationToken);

    internal override DriAssetDeliverableUnit Get(string id, CancellationToken cancellationToken) =>
        sqlExport.GetAssetDeliverableUnit(id, cancellationToken);

    internal override Task<DriAssetDeliverableUnit> GetAsync(Uri id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

}
