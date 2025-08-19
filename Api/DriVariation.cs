using System;
using System.Linq;

namespace Api;

public record DriVariation(Uri Link, string VariationName, string AssetReference) : IDriRecord
{
    public string Id => Link.Segments.Last();
}
