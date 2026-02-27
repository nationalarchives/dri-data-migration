using System.Collections.Generic;
using System.Threading;

namespace Api;

public interface IDriSqlExporter
{
    IEnumerable<DriAdm158SubsetDeliverableUnit> GetAdm158SubsetDeliverableUnits(int offset, CancellationToken cancellationToken);
    IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset, CancellationToken cancellationToken);
    IEnumerable<DriWo409SubsetDeliverableUnit> GetWo409SubsetDeliverableUnits(int offset, CancellationToken cancellationToken);
    IEnumerable<DriVariationFile> GetVariationFiles(int offset, CancellationToken cancellationToken);
    IEnumerable<DriChange> GetChanges(int offset, CancellationToken cancellationToken);
}