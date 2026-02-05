namespace Exporter;

internal class CoveringDate
{
    public string? FullStart { get; set; }
    public string? FullEnd { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
    public string? Text { get; set; }

    public bool IsTextExported() => Text is not null &&
        (Text.Contains('[') || Text.Contains(']') ||
        Text.Contains("c ") || Text.Contains('U'));
}
