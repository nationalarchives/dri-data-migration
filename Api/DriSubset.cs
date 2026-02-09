namespace Api;

public record DriSubset(string Reference, string? Directory = null, string? ParentReference = null) : IDriRecord
{
    public string Id => Reference;
}
