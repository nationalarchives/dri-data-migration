using System;

namespace Api;

public sealed class StagingSettings
{
    public Uri SparqlConnectionString { get; set; }
    public Uri SparqlUpdateConnectionString { get; set; }
}
