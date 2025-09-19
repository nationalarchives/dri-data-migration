using System;

namespace Api;

public sealed class DriSettings
{
    public const string Prefix = "dri";

    public string Code { get; set; }
    public string SqlConnectionString { get; set; }
    public Uri SparqlConnectionString { get; set; }
    public int FetchPageSize { get; set; }
    public EtlStageType? RestartFromStage { get; set; }
    public int RestartFromOffset { get; set; }
}
