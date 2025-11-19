using System;

namespace Api;

public sealed class ExportSettings
{
    public const string Prefix = "export";

#pragma warning disable CS8618
    public string Code { get; set; }
    public ExportScopeType ExportScope { get; set; }
    public Uri SparqlConnectionString { get; set; }
#pragma warning restore CS8618
    public int FetchPageSize { get; set; }
    public int RestartFromOffset { get; set; }
}
