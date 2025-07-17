namespace Api;

public record DriVariation(string Id, string VariationName, string AssetReference)
    : DriRecord(Id);
