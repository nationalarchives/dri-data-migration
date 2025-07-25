using System;

namespace Api;

public sealed class DriSettings
{
    public string SqlConnectionString { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public int FetchPageSize { get; set; }
}
