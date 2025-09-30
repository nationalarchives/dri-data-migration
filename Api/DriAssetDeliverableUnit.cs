namespace Api;

public record DriAssetDeliverableUnit(string Id, string Reference, string Xml, string FilesJson) : IDriRecord;
