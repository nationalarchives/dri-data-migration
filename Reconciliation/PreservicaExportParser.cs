using System.Globalization;
using System.Web;

namespace Reconciliation;

internal static class PreservicaExportParser
{
    internal static List<Dictionary<string, string>> Parse(string fileLocation)
    {
        List<Dictionary<string, string>> data = [];

        using (var csv = new Microsoft.VisualBasic.FileIO.TextFieldParser(fileLocation))
        {
            csv.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            csv.Delimiters = [","];
            csv.HasFieldsEnclosedInQuotes = true;
            var headers = !csv.EndOfData ? csv.ReadFields()?.ToList() : null;
            if (headers is not null)
            {
                while (!csv.EndOfData)
                {
                    var line = csv.ReadFields();
                    if (line is null || line.Length != headers.Count)
                    {
                        //TODO: logging
                        continue;
                    }
                    data.Add(
                        headers.Select((name, index) => new KeyValuePair<string, string>(name, line[index]))
                            .Where(kv => kv.Value is not null)
                            .ToDictionary(kv => kv.Key, kv => kv.Value!));
                }
            }
        }
        return data;
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
    internal static readonly Func<Dictionary<string, string>, string, string?> ToText = (dictionary, key) => (string?)To(dictionary, key, v => v);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    internal static string? ToLocation(string? txt, string code)
    {
        if (string.IsNullOrWhiteSpace(txt))
        {
            return null;
        }
        var location = HttpUtility.UrlDecode(txt);
        location = location.Substring(location.IndexOf("/content/"))
            .Replace("/content/", $"{code}/");

        return location;
    }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    internal static readonly Func<Dictionary<string, string>, string, object?> ToDate = (dictionary, key) => To(dictionary, key, v => DateTimeOffset.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt) ? dt : null);
    internal static readonly Func<Dictionary<string, string>, string, object?> ToIntDate = (dictionary, key) => To(dictionary, key, v => DateTimeOffset.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt) ? int.TryParse(dt.ToString("yyyyMMdd"), out int i) ? i : null : null);
    internal static readonly Func<Dictionary<string, string>, string, object?> ToTextList = (dictionary, key) => To(dictionary, key, v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
    internal static readonly Func<Dictionary<string, string>, string, object?> ToInt = (dictionary, key) => To(dictionary, key, v => int.TryParse(v, out int i) ? i : null);
    internal static readonly Func<Dictionary<string, string>, string, object?> ToBool = (dictionary, key) => To(dictionary, key, v => v == "TRUE") ?? true;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    private static readonly Func<Dictionary<string, string>, string, Func<string, object?>, object?> To = (dictionary, key, f) => dictionary.TryGetValue(key, out var value) ? f(value) : null;
}
