using Api;

namespace Orchestration;

internal static class ReconciliationEqualityComparer
{
    internal static IEnumerable<ReconciliationFieldName> Check(Dictionary<ReconciliationFieldName, object?> preservica, Dictionary<ReconciliationFieldName, object?> staging)
    {
        var difference = new List<ReconciliationFieldName>();

        foreach (var field in preservica.Keys)
        {
            if (preservica[field] is null && (!staging.ContainsKey(field) || staging[field] is null))
            {
                continue;
            }
            if (preservica[field] is not null && staging.TryGetValue(field, out var value))
            {
                if (preservica[field] is string[] preservicaArr && value is string[] stagingArr &&
                    !preservicaArr.Except(stagingArr).Any() && !stagingArr.Except(preservicaArr).Any())
                {
                    continue;
                }
                if (preservica[field]!.Equals(value))
                {
                    continue;
                }
            }
            difference.Add(field);
        }

        return difference;
    }
}
