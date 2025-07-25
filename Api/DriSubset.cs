namespace Api;

public record DriSubset(string Reference, string Directory, string? ParentReference = null) : IDriRecord
{
    public string Id => Reference;
}
