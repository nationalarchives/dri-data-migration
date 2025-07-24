using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api;

public interface IStagingIngest<T> where T : IDriRecord
{
    Task SetAsync(IEnumerable<T> dri);
}