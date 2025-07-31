using Api;
using System.Globalization;
using System.Web;

namespace Orchestration;

internal static class PreservicaExportParser
{
    internal static IEnumerable<Dictionary<ReconciliationFieldName, object>?> Parse(string fileLocation, Dictionary<string, ReconciliationRow> Map)
    {
        using (var csv = new Microsoft.VisualBasic.FileIO.TextFieldParser(fileLocation))
        {
            csv.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            csv.Delimiters = [","];
            csv.HasFieldsEnclosedInQuotes = true;
            var headers = !csv.EndOfData ? csv.ReadFields()?.ToList() : null;
            if (headers is null)
            {
                yield return null;
            }
            while (!csv.EndOfData)
            {
                var line = csv.ReadFields();
                if (line is null)
                {
                    continue;
                }
                yield return Map.Where(kv => headers!.Contains(kv.Key))
                    .Select(kv => new KeyValuePair<ReconciliationFieldName, object?>(kv.Value.Field, kv.Value.Conversion(line[headers!.IndexOf(kv.Key)])))
                    .Where(kv => kv.Value is not null)
                    .ToDictionary(kv => kv.Key, kv => kv.Value!);
            }
        }
    }

    internal static readonly Func<string?, string?> ToText = txt => txt;
    internal static readonly Func<string?, string?> ToLocation = txt => string.IsNullOrWhiteSpace(txt) ? null : HttpUtility.UrlDecode(txt);
    internal static readonly Func<string?, object?> ToTextList = txt => string.IsNullOrWhiteSpace(txt) ? null : txt.Split(',', StringSplitOptions.RemoveEmptyEntries);
    internal static readonly Func<string?, object?> ToDate = txt => DateTimeOffset.TryParse(txt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var v) ? v : null; //TODO: Test offset
    internal static readonly Func<string?, object?> ToInt = txt => int.TryParse(txt, out int v) ? v : null;
    internal static readonly Func<string?, object?> ToBool = txt => string.IsNullOrWhiteSpace(txt) ? null : txt.Equals("TRUE") ? true : txt.Equals("FALSE") ? false : null;
}
