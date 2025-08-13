using System;

namespace Api;

public sealed class ReconciliationSettings
{
    public const string Prefix = "reconciliation";

    public string Code { get; set; }
    public string FilePrefix { get; set; }
    public string FileLocation { get; set; }
    public MapType MapKind { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public Uri SearchRecordUri { get; set; }
    public int FetchPageSize { get; set; }
}
