using System.Collections.Generic;

namespace Api;

public interface IDriSqlExporter
{
    IEnumerable<DriAssetDeliverableUnit> GetAssetDeliverableUnits(int offset);
    IEnumerable<DriVariationFile> GetVariationFiles(int offset);
}