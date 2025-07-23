using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api;

public interface IStagingReconciliationClient
{
    Task<IEnumerable<Dictionary<ReconciliationFieldName, object>>> FetchAsync(string code, int pageSize, int offset);
}
