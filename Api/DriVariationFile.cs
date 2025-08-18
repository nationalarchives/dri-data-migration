namespace Api;

public record DriVariationFile(string Id, string Location, string Name, string Xml) : IDriRecord;
