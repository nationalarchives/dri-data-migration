namespace Api;

public record DriAsset(string Reference, string? Directory, string SubsetReference) : IDriRecord
{
    public string Id => Reference;
}
