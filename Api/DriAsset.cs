using System;
using System.Linq;

namespace Api;

public record DriAsset(Uri Link, string Reference, string? Directory, string SubsetReference) : IDriRecord
{
    public string Id => Link.Segments.Last();
}
