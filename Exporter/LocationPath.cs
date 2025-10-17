namespace Exporter;

internal class LocationPath
{
    public LocationPath()
    {
        Original = string.Empty;
        SensitiveName = string.Empty;
    }

    public LocationPath(string original, string sensitiveName)
    {
        Original = original;
        SensitiveName = sensitiveName;
    }

    public string Original { get; }
    public string SensitiveName { get; }
}
