namespace Api;

public record DriSubset(string Reference, string Directory, string? ParentReference = null) : DriRecord(Reference);
