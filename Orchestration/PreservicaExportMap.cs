using Api;

namespace Orchestration;

public static partial class PreservicaExportMap
{
    internal static Dictionary<string, ReconciliationRow>? GetMap(MapType mapType) => mapType switch
    {
        MapType.Metadata => Metadata,
        MapType.Closure => Closure,
        _ => null
    };

    internal static Dictionary<string, ReconciliationRow> Metadata => new()
        {
            { "identifier", new(ReconciliationFieldName.ImportLocation, PreservicaExportParser.ToLocation) },
            { "file_name", new(ReconciliationFieldName.VariationName, PreservicaExportParser.ToText) },
            { "folder", new(ReconciliationFieldName.FileFolder, PreservicaExportParser.ToText) }
        };

    internal static Dictionary<string, ReconciliationRow> Closure => new()
        {
            { "identifier", new(ReconciliationFieldName.ImportLocation, PreservicaExportParser.ToLocation) },
            { "folder", new(ReconciliationFieldName.FileFolder, PreservicaExportParser.ToText) },
            { "closure_type", new(ReconciliationFieldName.AccessConditionName, PreservicaExportParser.ToText) },
            { "closure_period", new(ReconciliationFieldName.SensitivityReviewDuration, PreservicaExportParser.ToInt) },
            { "closure_start_date", new(ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, PreservicaExportParser.ToDate) },
            { "foi_exemption_code", new(ReconciliationFieldName.LegislationSectionReference, PreservicaExportParser.ToTextList) },
            { "foi_exemption_asserted", new(ReconciliationFieldName.SensitivityReviewDate, PreservicaExportParser.ToDate) },
            { "title_public", new(ReconciliationFieldName.IsPublicName, PreservicaExportParser.ToBool) },
            { "title_alternate", new(ReconciliationFieldName.SensitivityReviewSensitiveName, PreservicaExportParser.ToText) }
        };
}

