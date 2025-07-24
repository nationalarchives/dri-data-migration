using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api;

public interface IStagingIngest<T> where T : IDriRecord
{
    Task<int> SetAsync(IEnumerable<T> dri);
}