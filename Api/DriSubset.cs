using System;

namespace Api;

public record DriSubset(string Reference, string? Directory = null, string? ParentReference = null, Uri? TransferringBody = null) : IDriRecord
{
    public string Id => Reference;
}
