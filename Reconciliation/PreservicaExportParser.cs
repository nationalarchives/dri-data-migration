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

    internal static readonly Func<Dictionary<string, string>, string, string?> ToText = (dictionary, key) => dictionary.TryGetValue(key, out var value) ? string.IsNullOrWhiteSpace(value) ? null : value : null;
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
    internal static string? ToName(string? folderOrFile, string? fileName)
    {
        if (folderOrFile is not "folder")
        {
            return fileName;
        }

        return null;
    }
    internal static DateTimeOffset? ToDate(string? folderOrFile, string? date)
    {
        if (folderOrFile is not "folder")
        {
            return DateTimeOffset.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var v) ? v : null;
        }

        return null;
    }
    internal static readonly Func<string?, object?> ToTextList = txt => string.IsNullOrWhiteSpace(txt) ? null : txt.Split(',', StringSplitOptions.RemoveEmptyEntries);
    internal static readonly Func<string?, object?> ToInt = txt => int.TryParse(txt, out int v) ? v : null;
    internal static readonly Func<Dictionary<string, string>, string, object?> ToBool = (dictionary, key) => dictionary.TryGetValue(key, out var value) ? value == "TRUE" : true;
}
