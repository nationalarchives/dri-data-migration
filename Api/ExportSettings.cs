using System;

namespace Api;

public sealed class ExportSettings
{
    public const string Prefix = "export";

    public string Code { get; set; }
    public ExportScopeType ExportScope { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public int FetchPageSize { get; set; }
    public int RestartFromOffset { get; set; }
}
