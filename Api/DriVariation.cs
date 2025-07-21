using System;

namespace Api;

public record DriVariation(Uri Link, string VariationName, string AssetReference)
    : DriRecord(Link.ToString());
