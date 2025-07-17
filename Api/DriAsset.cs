namespace Api;

public record DriAsset(string Reference, string Directory, string SubsetReference) : DriRecord(Reference);
