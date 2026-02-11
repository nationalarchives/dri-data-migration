using System;

namespace Api;

public record DriAsset(Uri Link, string Reference, string? Directory, string SubsetReference,
    Uri? TransferringBody, Uri? CreationBody) : IDriRecord
{
    public string Id => Link.LastSegment();
}
