using System.Collections.Generic;
using System.Threading;

namespace Api;

public interface IDriSqlExporter
{
    IEnumerable<string> GetList(EtlStageType etlStageType, CancellationToken cancellationToken);

    DriAssetDeliverableUnit GetAssetDeliverableUnit(string id, CancellationToken cancellationToken);
    DriWo409SubsetDeliverableUnit GetWo409SubsetDeliverableUnit(string id, CancellationToken cancellationToken);
    DriVariationFile GetVariationFile(string id, CancellationToken cancellationToken);
    DriChange GetChange(string id, CancellationToken cancellationToken);
}