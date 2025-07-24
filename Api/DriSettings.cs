namespace Api;

public sealed class DriSettings
{
    public string SqlConnectionString { get; set; }
    public string SparqlConnectionString { get; set; }
    public int FetchPageSize { get; set; }
}
