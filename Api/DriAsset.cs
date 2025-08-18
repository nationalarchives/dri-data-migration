using System;

namespace Api;

public record DriAsset(Uri Link, string Reference, string? Directory, string SubsetReference) : IDriRecord
{
    public string Id => Link.ToString();
}
