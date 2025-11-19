using System;

namespace Api;

public sealed class StagingSettings
{
    public const string Prefix = "staging";

#pragma warning disable CS8618
    public string Code { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public Uri SparqlUpdateConnectionString { get; set; }
#pragma warning restore CS8618
    public int FetchPageSize { get; set; }
}
