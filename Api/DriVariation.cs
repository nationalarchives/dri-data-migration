using System;

namespace Api;

public record DriVariation(Uri Link, string VariationName, string AssetReference) : IDriRecord
{
    public string Id => Link.ToString();
}
