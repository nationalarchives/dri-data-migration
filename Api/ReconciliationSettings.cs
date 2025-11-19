using System;
using System.Collections.Generic;

namespace Api;

public sealed class ReconciliationSettings
{
    public const string Prefix = "reconciliation";

#pragma warning disable CS8618
    public string Code { get; set; }
    public IEnumerable<string> FileLocation { get; set; }
    public ReconciliationMapType MapKind { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public Uri SearchRecordUri { get; set; }
#pragma warning restore CS8618
    public int FetchPageSize { get; set; }
}
