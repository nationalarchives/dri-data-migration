using System;

namespace Api;

public sealed class DriSettings
{
    public const string Prefix = "dri";

#pragma warning disable CS8618
    public string Code { get; set; }
    public string SqlConnectionString { get; set; }
    public Uri SparqlConnectionString { get; set; }
#pragma warning restore CS8618
    public int FetchPageSize { get; set; }
    public EtlStageType? RestartFromStage { get; set; }
    public int RestartFromOffset { get; set; }
}
