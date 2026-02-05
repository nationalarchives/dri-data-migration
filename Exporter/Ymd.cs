namespace Exporter;

internal class Ymd
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public string? Verbatim{ get; set; }

    public string? ToTextDate() => $"{Year}-{Month:D2}-{Day:D2}".Trim('-');
}
