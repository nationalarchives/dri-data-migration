using System.Collections.Generic;
using System.Threading;

namespace Api;

public interface IDriSqlExporter
{
    IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset, CancellationToken cancellationToken);
    IEnumerable<DriVariationFile> GetVariationFiles(int offset, CancellationToken cancellationToken);
    IEnumerable<DriChange> GetChanges(int offset, CancellationToken cancellationToken);
}