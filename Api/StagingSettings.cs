using System;

namespace Api;

public sealed class StagingSettings
{
    public const string Prefix = "staging";

    public string Code { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public Uri SparqlUpdateConnectionString { get; set; }
    public int FetchPageSize { get; set; }
}
